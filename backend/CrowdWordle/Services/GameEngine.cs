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
            //var packedStates = PackStates(states);
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
        // Extract letters
        uint g0 = guess & 0x1F, g1 = (guess >> 5) & 0x1F, g2 = (guess >> 10) & 0x1F;
        uint g3 = (guess >> 15) & 0x1F, g4 = (guess >> 20) & 0x1F;

        uint t0 = target & 0x1F, t1 = (target >> 5) & 0x1F, t2 = (target >> 10) & 0x1F;
        uint t3 = (target >> 15) & 0x1F, t4 = (target >> 20) & 0x1F;

        // Exact matches
        uint eq0 = (uint)-(g0 == t0 ? 1 : 0);
        uint eq1 = (uint)-(g1 == t1 ? 1 : 0);
        uint eq2 = (uint)-(g2 == t2 ? 1 : 0);
        uint eq3 = (uint)-(g3 == t3 ? 1 : 0);
        uint eq4 = (uint)-(g4 == t4 ? 1 : 0);

        // Build frequency table (3 bits per letter)
        ulong freq = 0;
        freq += 1UL << ((int)t0 * 3);
        freq += 1UL << ((int)t1 * 3);
        freq += 1UL << ((int)t2 * 3);
        freq += 1UL << ((int)t3 * 3);
        freq += 1UL << ((int)t4 * 3);

        // Subtract exact matches
        freq -= (eq0 & 1UL) << ((int)g0 * 3);
        freq -= (eq1 & 1UL) << ((int)g1 * 3);
        freq -= (eq2 & 1UL) << ((int)g2 * 3);
        freq -= (eq3 & 1UL) << ((int)g3 * 3);
        freq -= (eq4 & 1UL) << ((int)g4 * 3);

        // Inline saturating subtraction & presence checks
        ulong c0 = (freq >> ((int)g0 * 3)) & 7UL; uint pr0 = (uint)-((eq0 == 0 && c0 > 0) ? 1 : 0); freq -= ((c0 > 0 ? 1UL : 0UL) << ((int)g0 * 3));
        ulong c1 = (freq >> ((int)g1 * 3)) & 7UL; uint pr1 = (uint)-((eq1 == 0 && c1 > 0) ? 1 : 0); freq -= ((c1 > 0 ? 1UL : 0UL) << ((int)g1 * 3));
        ulong c2 = (freq >> ((int)g2 * 3)) & 7UL; uint pr2 = (uint)-((eq2 == 0 && c2 > 0) ? 1 : 0); freq -= ((c2 > 0 ? 1UL : 0UL) << ((int)g2 * 3));
        ulong c3 = (freq >> ((int)g3 * 3)) & 7UL; uint pr3 = (uint)-((eq3 == 0 && c3 > 0) ? 1 : 0); freq -= ((c3 > 0 ? 1UL : 0UL) << ((int)g3 * 3));
        ulong c4 = (freq >> ((int)g4 * 3)) & 7UL; uint pr4 = (uint)-((eq4 == 0 && c4 > 0) ? 1 : 0); freq -= ((c4 > 0 ? 1UL : 0UL) << ((int)g4 * 3));

        // Pack into 2-bit per letter result
        return ((eq0 << 1) & 2u) | (pr0 & 1u) |
               (((eq1 << 1) & 2u) | (pr1 & 1u)) << 2 |
               (((eq2 << 1) & 2u) | (pr2 & 1u)) << 4 |
               (((eq3 << 1) & 2u) | (pr3 & 1u)) << 6 |
               (((eq4 << 1) & 2u) | (pr4 & 1u)) << 8;
    }
}