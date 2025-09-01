using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrowdWordle;

public static class EncodingHelper
{
    private const int WORD_LENGTH = 5;
    private const char BASE_CHAR = 'a';
    private const int BITS_PER_CHAR = 5;
    private const int BITS_PER_WORD = BITS_PER_CHAR * WORD_LENGTH;
    private const int BITS_PER_BLOCK = 2;
    private const uint BITMASK = (1u << BITS_PER_CHAR) - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackFromString(ReadOnlySpan<char> word)
    {
        if (word.Length != WORD_LENGTH) return 0;

        uint packed = 0;
        for (int i = 0; i < WORD_LENGTH; i++)
        {
            int value = word[i] - BASE_CHAR;
            if (value < 0 || value > 25) return 0;
            packed |= (uint)(value & BITMASK) << (i * BITS_PER_CHAR);
        }
        return packed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Unpack(uint packed)
    {
        var result = new byte[WORD_LENGTH];
        for (int i = 0; i < WORD_LENGTH; i++)
        {
            byte value = (byte)((packed >> (i * BITS_PER_CHAR)) & BITMASK);
            result[i] = value;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UnpackToString(uint packed)
    {
        var result = new char[WORD_LENGTH];
        for (int i = 0; i < WORD_LENGTH; i++)
        {
            byte value = (byte)((packed >> (i * BITS_PER_CHAR)) & BITMASK);
            result[i] = (char)(value + BASE_CHAR);
        }
        return new string(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(uint packed)
    {
        uint v0 = packed & 0x1F;
        uint v1 = (packed >> 5) & 0x1F;
        uint v2 = (packed >> 10) & 0x1F;
        uint v3 = (packed >> 15) & 0x1F;
        uint v4 = (packed >> 20) & 0x1F;

        // 25 - v gives >= 0 if valid, < 0 if invalid.
        // So the sign bit (bit 31) is 0 when valid, 1 when invalid.
        uint flags =
            ((25 - v0) & 0x80000000) |
            ((25 - v1) & 0x80000000) |
            ((25 - v2) & 0x80000000) |
            ((25 - v3) & 0x80000000) |
            ((25 - v4) & 0x80000000);

        return flags == 0;
    }


    public static byte[] PackInitialMessage(in Game game, DateTime? nextEventTime, uint userCount, uint totalVotes, ReadOnlySpan<Vote> topWords, uint myWord)
    {
        Span<byte> buffer = stackalloc byte[64];
        var writer = new BitWriter(buffer);

        writer.WriteBits((uint)ServerMessageType.InitialState, 3);
        writer.WriteBits((uint)game.State, 2);
        writer.WriteBits(userCount, 16);

        if (game.GameOver || game.State == GameState.VotingInProgress)
        {
            writer.WriteBits(CalculateTimeRemaining(nextEventTime), 4);
        }

        writer.WriteBits(game.Round, 3);

        if (game.GameOver)
            writer.WriteBits(game.SelectedWord, BITS_PER_WORD);

        PackBoardState(ref writer, in game);

        if (game.State == GameState.VotingInProgress || game.GameOver)
        {
            PackVotingData(ref writer, totalVotes, topWords);
        }

        writer.WriteBits(myWord != 0 ? 1u : 0u, 1);
        if (myWord != 0)
        {
            writer.WriteBits(myWord, BITS_PER_WORD);
        }

        return buffer[..writer.Finish()].ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] PackAcknowledgement(RequestResponseType voteResult)
    {
        Span<byte> buffer = stackalloc byte[1];
        var writer = new BitWriter(buffer);
        writer.WriteBits((uint)ServerMessageType.Response, 3);
        writer.WriteBits((uint)voteResult, 2);
        return buffer[..writer.Finish()].ToArray();
    }

    public static byte[] PackVotingStarted(in Game game, DateTime? nextEventTime)
    {
        Span<byte> buffer = stackalloc byte[2];
        var writer = new BitWriter(buffer);
        writer.WriteBits((uint)ServerMessageType.VotingStarted, 3);
        writer.WriteBits(game.Round, 3);
        writer.WriteBits(CalculateTimeRemaining(nextEventTime), 4);
        return buffer[..writer.Finish()].ToArray();
    }

    public static unsafe byte[] PackGameUpdate(in Game game, DateTime? nextEvent)
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = new BitWriter(buffer);

        writer.WriteBits((uint)ServerMessageType.GameUpdate, 3);
        writer.WriteBits((uint)game.State, 2);

        if (!game.GameOver)
            writer.WriteBits(game.Round, 3);
        if(game.GameOver)
        {
            writer.WriteBits(CalculateTimeRemaining(nextEvent), 4);
        }

        var word = game.GetWord(game.Round - 1);
        writer.WriteBits(word.Packed, BITS_PER_WORD);
        writer.WriteBits(word.States, 10); // 5 states * 2 bits each

        if (game.State == GameState.Lost)
        {
            writer.WriteBits(game.SelectedWord, BITS_PER_WORD);
        }

        return buffer[..writer.Finish()].ToArray();
    }

    public static byte[] PackLiveData(in Game game, uint userCount, uint totalVotes, ReadOnlySpan<Vote> topWords)
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = new BitWriter(buffer);

        writer.WriteBits((uint)ServerMessageType.LiveData, 3);
        writer.WriteBits((uint)game.State, 2);
        writer.WriteBits(userCount, 16);

        if (game.State == GameState.VotingInProgress || game.GameOver)
        {
            PackVotingData(ref writer, totalVotes, topWords);
        }

        return buffer[..writer.Finish()].ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] PackNewGame(DateTime nextEventTime)
    {
        Span<byte> buffer = stackalloc byte[1];
        var writer = new BitWriter(buffer);
        writer.WriteBits((uint)ServerMessageType.GameStarting, 3);
        return buffer[..writer.Finish()].ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] PackVoteStream(List<uint> words)
    {
        Span<byte> buffer = stackalloc byte[33];
        var writer = new BitWriter(buffer);
        writer.WriteBits((uint)ServerMessageType.VoteStream, 3);
        writer.WriteBits((uint)words.Count, 4);
        foreach (var word in words)
        {
            writer.WriteBits(word, BITS_PER_WORD);
        }
        return buffer[..writer.Finish()].ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint CalculateTimeRemaining(DateTime? eventTime)
    {
        if (!eventTime.HasValue)
            return 0;

        var remaining = Math.Max(0, Math.Floor((eventTime.Value - DateTime.UtcNow).TotalSeconds));
        return Math.Min((uint)remaining, 15);
    }

    private static unsafe void PackBoardState(ref BitWriter writer, in Game game)
    {
        for (int i = 0; i < game.Round; i++)
        {
            var word = game.GetWord(i);
            writer.WriteBits(word.Packed, BITS_PER_WORD);
            writer.WriteBits(word.States, 10); // 5 states * 2 bits each
        }
    }

    private static void PackVotingData(ref BitWriter writer, uint totalVotes, ReadOnlySpan<Vote> topWords)
    {
        writer.WriteBits(totalVotes, 16);
        writer.WriteBits((uint)topWords.Length, 2);

        foreach (ref readonly var vote in topWords)
        {
            writer.WriteBits(vote.Word, BITS_PER_WORD);
            writer.WriteBits(vote.Count, 16);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private ref struct BitWriter
    {
        private Span<byte> _buffer;
        private int _bytePosition;
        private ulong _bitBuffer;
        private int _bitCount;

        public BitWriter(Span<byte> buffer)
        {
            _buffer = buffer;
            _bytePosition = 0;
            _bitBuffer = 0;
            _bitCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(uint value, int bits)
        {
            _bitBuffer |= (value & ((1UL << bits) - 1)) << _bitCount;
            _bitCount += bits;

            while (_bitCount >= 8 && _bytePosition < _buffer.Length)
            {
                _buffer[_bytePosition++] = (byte)_bitBuffer;
                _bitBuffer >>= 8;
                _bitCount -= 8;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Finish()
        {
            if (_bitCount > 0 && _bytePosition < _buffer.Length)
            {
                _buffer[_bytePosition++] = (byte)_bitBuffer;
            }
            return _bytePosition;
        }
    }
}