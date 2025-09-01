using System.Collections.Concurrent;

namespace CrowdWordle.Services;

public class VoteStreamingService(int maxWordsPerSecond = 5, int maxQueueSize = 1000)
{
    private readonly ConcurrentQueue<uint> _votes = new();
    private int _currentQueueSize = 0;

    public void AddVote(uint word)
    {
        if (_currentQueueSize >= maxQueueSize)
        {
            if (_votes.TryDequeue(out _))
            {
                Interlocked.Decrement(ref _currentQueueSize);
            }
        }

        _votes.Enqueue(word);
        Interlocked.Increment(ref _currentQueueSize);
    }

    public List<uint> GetNextBatch()
    {
        var wordsToSend = new List<uint>();

        var maxWords = maxWordsPerSecond * 2;

        for (int i = 0; i < maxWords && _votes.TryDequeue(out var word); i++)
        {
            wordsToSend.Add(word);
            Interlocked.Decrement(ref _currentQueueSize);
        }

        return wordsToSend;
    }

    public void ClearAll()
    {
        _votes.Clear();
        _currentQueueSize = 0;
    }

    public int QueueCount => _currentQueueSize;
}
