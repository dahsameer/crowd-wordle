using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrowdWordle.Services;

public sealed class WordService
{
    private readonly FrozenSet<uint> _validWords;
    private readonly uint[] _gameWords;
    private readonly Random _random = new();
    private readonly Lock _indexLock = new();
    private readonly ILogger<WordService> _logger;

    private int _currentIndex;

    public WordService(ILogger<WordService> logger)
    {
        var allWords = LoadWordsFromResource("words.txt");
        _validWords = allWords.ToFrozenSet();
        _gameWords = [.. allWords];
        ShuffleArray(_gameWords);
        _logger = logger;
    }

    public uint GetNextWord()
    {
        lock (_indexLock)
        {
            if (_currentIndex >= _gameWords.Length)
            {
                ShuffleArray(_gameWords);
                _currentIndex = 0;
            }

            var selectedWord = _gameWords[_currentIndex++];
            _logger.LogInformation("Selected a new word: {Word}", EncodingHelper.UnpackToString(selectedWord));
            return selectedWord;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValidWord(uint word) => word != 0 && _validWords.Contains(word);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTotalWordCount() => _gameWords.Length;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ShuffleArray(uint[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private static uint[] LoadWordsFromResource(string resourceFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = FindResourceName(assembly, resourceFileName);

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not open resource stream for '{resourceName}'.");

        using var reader = new StreamReader(stream, Encoding.UTF8);

        var words = new List<uint>(15000);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length == 5 && IsValidWordFormat(line))
            {
                var packed = EncodingHelper.PackFromString(line.AsSpan());
                if (packed != 0)
                {
                    words.Add(packed);
                }
            }
        }

        if (words.Count == 0)
            throw new InvalidOperationException($"No valid words found in resource '{resourceFileName}'.");

        return [.. words.Distinct().OrderBy(x => x)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidWordFormat(ReadOnlySpan<char> word)
    {
        if (word.Length != 5) return false;

        foreach (var c in word)
        {
            if (c < 'a' || c > 'z') return false;
        }
        return true;
    }

    private static string FindResourceName(Assembly assembly, string resourceFileName)
    {
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase));

        return resourceName ?? throw new FileNotFoundException(
            $"Resource '{resourceFileName}' not found. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
    }

    public string GetTagLine()
    {
        string[] taglines = [
            "Democracy: now with more typos.",
            "The only democracy Plato would approve of.",
            "The Republic of Bad Guesses.",
            "Direct democracy in five letters.",
            "Filibuster your way to the right word.",
            "Now with blockchain...",
            "Like GitHub, but for letters.",
            "Now with fewer ads than Medium.",
            "Everyone is wrong together.",
            "Collective action at its least effective.",
            "The wisdom of crowds, heavily diluted.",
            "Yes, this was a bad idea.",
            "Making democracy fun again (for once).",
            "Now in beta forever.",
            "Because you can't spell 'crowd' without 'ow'",
            "Five letters, zero consensus.",
            "The people have spoken, and they're wrong.",
            "Democracy: where everyone loses equally.",
            "One mob, one dream, zero accuracy.",
            "Collectively failing faster.",
            "Stronger together... but still wrong.",
            "We the people... can't spell.",
            "The loudest guess wins.",
            "Chaotic neutral as a service.",
            "Series A funded by bad guesses.",
            "All the fun of democracy, none of the governance.",
            "Like Stack Overflow, but with voting that matters even less.",
            "As scalable as your AWS bill.",
            "We did the MVP, you do the QA.",
            "Crowdsourcing: humanity's favorite denial-of-service attack.",
            "No ads, no paywall, no cookies, no sense.",
            "Now with more uptime than your favorite crypto exchange.",
            "A neural network made from real people.",
            "Not serverless, but definitely brainless.",
            "Vim keybindings not included... yet.",
            "More stable than your crypto portfolio.",
            "Shipped to prod on a Friday.",
            "Stateless votes, stateful regret.",
            "Distributed computing for distributed guessing.",
            "Peer review, but faster and more wrong.",
            "Hypothesis: Crowds are smarter. Status: TBD.",
            "Disrupting the word-guessing industrial complex.",
            "Growth hacking the English language.",
            "Machine learning can't save us now.",
            "Statistical significance meets statistical suffering.",
            "Crowdsourcing intelligence, achieving the opposite.",
            "Make America Guess Again.",
            "NATO Article 5, but for wrong letters.",
            "Liquidity crisis in the vowel market.",
            "Nihilism: Nothing matters, especially your guess.",
            "The absurdist's guide to five-letter words.",
            "Those who don't learn from history are doomed to guess MOIST again.",
            "Control group shows signs of intelligence.",
            "Betteridge's Law: If the headline ends in a question mark, the answer is piaNO.",
            "LinkedIn influencer energy: 'Agree?'. ",
            "Synergistic approach to alphabetical alignment.",
            "Think inside the box.",
            "Scientists baffled by collective intelligence producing collective confusion.",
            "Hive mind discovers it has no mind.",
            "Democracy working as intended (poorly).",
            "Feature creep is a democratic right.",
            "Latency is the new voter fraud.",
            "Now with 0% blockchain.",
            "The tragedy of the commons, but fun this time.",
          ];
        return taglines[_random.Next(taglines.Length)];
    }
}