using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace CrowdWordle.Services;

public sealed class VotingService(IOptions<GameConfiguration> config, WordService wordService, VoteStreamingService voteStreamingService)
{
    private readonly GameConfiguration _config = config.Value;
    private readonly ConcurrentDictionary<uint, uint> _userVotes = new();
    private readonly ConcurrentDictionary<uint, uint> _votes = new();
    private readonly Lock _votingLock = new();

    private volatile bool _votingActive = false;
    private DateTime? _votingEndTime;

    public RequestResponseType Vote(uint userId, uint word, ref Game game, out bool first)
    {
        first = false;
        if (game.State != GameState.WaitingForVote && game.State != GameState.VotingInProgress)
            return RequestResponseType.VotingNotAllowed;

        if (!EncodingHelper.IsValid(word) || !wordService.IsValidWord(word))
            return RequestResponseType.InvalidVote;

        if (_userVotes.TryGetValue(userId, out var existingVote))
            return RequestResponseType.AlreadyVoted;

        if (!_userVotes.TryAdd(userId, word))
            return RequestResponseType.AlreadyVoted;

        _votes.AddOrUpdate(word, 1, (_, oldValue) => oldValue + 1);
        voteStreamingService.AddVote(word);
        lock (_votingLock)
        {
            if (!_votingActive && game.State == GameState.WaitingForVote)
            {
                game.State = GameState.VotingInProgress;
                _votingActive = true;
                _votingEndTime = DateTime.UtcNow.Add(_config.VotingDuration);
                first = true;
            }
        }

        return RequestResponseType.Success;
    }

    public void EndVoting()
    {
        lock (_votingLock)
        {
            _votingActive = false;
            _votingEndTime = null;
        }
    }

    public void ClearVotes()
    {
        _votes.Clear();
        _userVotes.Clear();

        lock (_votingLock)
        {
            _votingActive = false;
            _votingEndTime = null;
        }
    }

    public void StartVoting()
    {
        lock (_votingLock)
        {
            _votingActive = true;
            _votingEndTime = DateTime.UtcNow.Add(_config.VotingDuration);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsVotingActive() => _votingActive;

    public DateTime? GetVotingEndTime()
    {
        lock (_votingLock)
        {
            return _votingEndTime;
        }
    }

    public uint GetTotalVotes()
    {
        uint total = 0;
        foreach (var vote in _votes)
        {
            total += vote.Value;
        }
        return total;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vote GetTopVote()
    {
        Vote topVote = new()
        {
            Word = 0,
            Count = 0
        };
        foreach (var vote in _votes)
        {
            if (vote.Value > topVote.Count)
            {
                topVote.Word = vote.Key;
                topVote.Count = vote.Value;
            }
        }
        return topVote;
    }

    public Vote[] GetTop3Votes()
    {
        if (_votes.IsEmpty)
            return [];

        uint firstCount = uint.MinValue, secondCount = uint.MinValue, thirdCount = uint.MinValue;
        uint firstWord = 0, secondWord = 0, thirdWord = 0;

        foreach (var kvp in _votes)
        {
            uint word = kvp.Key;
            uint count = kvp.Value;

            if (count > firstCount)
            {
                (thirdCount, thirdWord) = (secondCount, secondWord);
                (secondCount, secondWord) = (firstCount, firstWord);
                (firstCount, firstWord) = (count, word);
            }
            else if (count > secondCount)
            {
                (thirdCount, thirdWord) = (secondCount, secondWord);
                (secondCount, secondWord) = (count, word);
            }
            else if (count > thirdCount)
            {
                (thirdCount, thirdWord) = (count, word);
            }
        }
        int wordCount = 0;
        if (firstCount != 0) wordCount++;
        if (secondCount != 0) wordCount++;
        if (thirdCount != 0) wordCount++;

        var result = new Vote[wordCount];
        int i = 0;
        if (firstCount != 0) result[i++] = new Vote { Word = firstWord, Count = firstCount };
        if (secondCount != 0) result[i++] = new Vote { Word = secondWord, Count = secondCount };
        if (thirdCount != 0) result[i++] = new Vote { Word = thirdWord, Count = thirdCount };

        return result;
    }

    public uint GetUserVote(uint userId) => _userVotes.TryGetValue(userId, out var vote) ? vote : 0;

}