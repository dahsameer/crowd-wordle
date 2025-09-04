using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CrowdWordle.Services;

public sealed class GameEngine
{
    private readonly GameConfiguration _config;
    private readonly WordService _wordService;
    private readonly Lock _gameLock = new();
    private Game _currentGame;
    private DateTime? _nextEventTime;
    private const uint AllCorrectPacked = 0x2AA;

    public GameEngine(IOptions<GameConfiguration> config, WordService wordService)
    {
        _config = config.Value;
        _wordService = wordService;
        _currentGame = CreateNewGame();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Game GetCurrentGame() => ref _currentGame;

    public bool ProcessVoteResult(uint topWord)
    {
        lock (_gameLock)
        {
            if (_currentGame.GameOver)
                return true;
            var packedStates = ProcessWordExperimental(topWord, _currentGame.SelectedWord);
            //var states = ProcessWord(topWord, _currentGame.SelectedWord);
            //var packedStates = EncodingHelper.PackStates(states);
            var playedWord = new PlayedWord
            {
                Packed = topWord,
                States = packedStates
            };

            _currentGame.SetWord(_currentGame.Round, playedWord);
            _currentGame.Round++;

            bool isWin = IsAllCorrect(packedStates);
            bool isGameOver = isWin || _currentGame.Round >= _config.MaxRounds;

            if (isGameOver)
            {
                _currentGame.State = isWin ? GameState.Won : GameState.Lost;
                _nextEventTime = DateTime.UtcNow.Add(_config.GameInterval);
                return false;
            }
            else
            {
                _currentGame.State = GameState.WaitingForVote;
                _nextEventTime = null;
                return true;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartNewGame()
    {
        _currentGame = CreateNewGame();
        _nextEventTime = null;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGameOver() => _currentGame.GameOver;

    public DateTime? GetNextEventTime()
    {
        lock (_gameLock)
        {
            return _nextEventTime;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Game CreateNewGame()
    {
        return new Game
        {
            State = GameState.WaitingForVote,
            SelectedWord = _wordService.GetNextWord(),
            Round = 0,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAllCorrect(uint packed) => packed == AllCorrectPacked;


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BlockState[] ProcessWord(uint guess, uint target)
    {
        var result = new BlockState[5];

        if (guess == target)
        {
            Array.Fill(result, BlockState.Correct);
            return result;
        }

        var guessLetters = EncodingHelper.Unpack(guess);
        var targetLetters = EncodingHelper.Unpack(target);

        Span<int> counts = stackalloc int[26];

        // First pass: exact matches
        for (int i = 0; i < 5; i++)
        {
            if (guessLetters[i] == targetLetters[i])
            {
                result[i] = BlockState.Correct;
            }
            else
            {
                counts[targetLetters[i]]++;
            }
        }

        // Second pass: present/absent
        for (int i = 0; i < 5; i++)
        {
            if (result[i] == BlockState.Correct)
                continue;

            int letterIndex = guessLetters[i];
            if (counts[letterIndex] > 0)
            {
                result[i] = BlockState.Present;
                counts[letterIndex]--;
            }
        }

        return result;
    }

    public static uint ProcessWordExperimental(uint guess, uint target)
    {
        // Unpack guess and target
        uint g0 = guess & 0x1F, g1 = (guess >> 5) & 0x1F, g2 = (guess >> 10) & 0x1F;
        uint g3 = (guess >> 15) & 0x1F, g4 = (guess >> 20) & 0x1F;
        uint t0 = target & 0x1F, t1 = (target >> 5) & 0x1F, t2 = (target >> 10) & 0x1F;
        uint t3 = (target >> 15) & 0x1F, t4 = (target >> 20) & 0x1F;

        // Count target letter frequencies
        Span<byte> freq = stackalloc byte[26];
        freq[(int)t0]++; freq[(int)t1]++; freq[(int)t2]++; freq[(int)t3]++; freq[(int)t4]++;

        // Determine exact matches (greens)
        uint eq0 = (uint)-(g0 == t0 ? 1 : 0);
        uint eq1 = (uint)-(g1 == t1 ? 1 : 0);
        uint eq2 = (uint)-(g2 == t2 ? 1 : 0);
        uint eq3 = (uint)-(g3 == t3 ? 1 : 0);
        uint eq4 = (uint)-(g4 == t4 ? 1 : 0);

        if ((eq0 & 1) != 0) freq[(int)g0]--;
        if ((eq1 & 1) != 0) freq[(int)g1]--;
        if ((eq2 & 1) != 0) freq[(int)g2]--;
        if ((eq3 & 1) != 0) freq[(int)g3]--;
        if ((eq4 & 1) != 0) freq[(int)g4]--;

        // Presence (yellow) checks
        uint pr0 = (uint)-((eq0 == 0 && freq[(int)g0] > 0) ? 1 : 0); if ((pr0 & 1) != 0) freq[(int)g0]--;
        uint pr1 = (uint)-((eq1 == 0 && freq[(int)g1] > 0) ? 1 : 0); if ((pr1 & 1) != 0) freq[(int)g1]--;
        uint pr2 = (uint)-((eq2 == 0 && freq[(int)g2] > 0) ? 1 : 0); if ((pr2 & 1) != 0) freq[(int)g2]--;
        uint pr3 = (uint)-((eq3 == 0 && freq[(int)g3] > 0) ? 1 : 0); if ((pr3 & 1) != 0) freq[(int)g3]--;
        uint pr4 = (uint)-((eq4 == 0 && freq[(int)g4] > 0) ? 1 : 0); if ((pr4 & 1) != 0) freq[(int)g4]--;

        // Pack results into 2-bit per letter
        return ((eq0 << 1) & 2u) | (pr0 & 1u) |
               ((((eq1 << 1) & 2u) | (pr1 & 1u)) << 2) |
               ((((eq2 << 1) & 2u) | (pr2 & 1u)) << 4) |
               ((((eq3 << 1) & 2u) | (pr3 & 1u)) << 6) |
               ((((eq4 << 1) & 2u) | (pr4 & 1u)) << 8);
    }
}