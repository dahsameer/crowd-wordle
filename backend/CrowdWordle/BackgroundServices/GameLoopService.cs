using CrowdWordle.Services;

namespace CrowdWordle.BackgroundServices;

public sealed class GameLoopService(
    GameEngine gameEngine,
    VotingService votingService,
    ConnectionManager connectionManager,
    VoteStreamingService voteStreamingService) : BackgroundService
{
    private static readonly TimeSpan TICK_INTERVAL = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan BROADCAST_INTERVAL = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan STREAM_INTERVAL = TimeSpan.FromSeconds(1);

    private uint lastVotes = 0;
    private uint lastCount = 0;
    private Vote[] lastTops = [];

    private DateTime _nextBroadCastTime = DateTime.MinValue;
    private DateTime _nextStreamingTime = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TICK_INTERVAL, stoppingToken);

            var now = DateTime.UtcNow;

            if (gameEngine.IsGameOver())
            {
                var nextGameTime = gameEngine.GetNextEventTime();
                if (nextGameTime.HasValue && now >= nextGameTime.Value)
                {
                    HandleGameRestart();
                    continue;
                }
            }

            var votingEndTime = votingService.GetVotingEndTime();
            if (votingEndTime.HasValue && now >= votingEndTime.Value)
            {
                voteStreamingService.ClearAll();
                HandleVotingTimeout();
                continue;
            }

            if (_nextBroadCastTime <= now)
            {
                SendLiveUpdates();
                _nextBroadCastTime = now.Add(BROADCAST_INTERVAL);
            }

            if (_nextStreamingTime <= now)
            {
                SendStream();
                _nextStreamingTime = now.Add(STREAM_INTERVAL);
            }
        }
    }

    private void HandleGameRestart()
    {
        gameEngine.StartNewGame();
        votingService.ClearVotes();

        //var game = gameEngine.GetCurrentGame();
        var nextEventTime = gameEngine.GetNextEventTime();
        var message = EncodingHelper.PackNewGame(nextEventTime ?? DateTime.UtcNow);

        _ = connectionManager.BroadcastAsync(message.AsMemory());
    }

    private void HandleVotingTimeout()
    {
        var totalVotes = votingService.GetTotalVotes();
        if (totalVotes == 0)
        {
            votingService.ClearVotes();
            return;
        }

        var topWord = votingService.GetTopVote();
        bool clearVotes = gameEngine.ProcessVoteResult(topWord.Word);
        if (clearVotes)
            votingService.ClearVotes();
        else
            votingService.EndVoting();

        var game = gameEngine.GetCurrentGame();
        var timeToNextGame = gameEngine.GetNextEventTime();
        var message = EncodingHelper.PackGameUpdate(in game, timeToNextGame);

        _ = connectionManager.BroadcastAsync(message.AsMemory());
    }

    private void SendLiveUpdates()
    {
        var totalVotes = votingService.GetTotalVotes();
        var topVotes = votingService.GetTop3Votes();
        var userCount = connectionManager.GetConnectionCount();
        if (totalVotes == lastVotes && userCount == lastCount && AreEqual(topVotes, lastTops))
        {
            return;
        }
        ref readonly var game = ref gameEngine.GetCurrentGame();

        var message = EncodingHelper.PackLiveData(in game, userCount, totalVotes, topVotes.AsSpan());

        _ = connectionManager.BroadcastAsync(message.AsMemory());
        lastVotes = totalVotes;
        lastCount = userCount;
        lastTops = topVotes;
    }

    private void SendStream()
    {
        var words = voteStreamingService.GetNextBatch();
        if (words.Count == 0)
            return;
        var message = EncodingHelper.PackVoteStream(words);
        _ = connectionManager.BroadcastAsync(message.AsMemory());
    }

    private static bool AreEqual(Vote[] a, Vote[] b)
    {
        if (a == b) return true;
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        if (a.Length == b.Length) return true;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].Word != b[i].Word || a[i].Count != b[i].Count)
                return false;
        }

        return true;
    }
}