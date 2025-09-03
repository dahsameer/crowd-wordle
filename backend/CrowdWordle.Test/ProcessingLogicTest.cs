using CrowdWordle.Services;

namespace CrowdWordle.Test;

public class ProcessingLogicTest
{
    [Theory]
    [InlineData("pumas", "punas", "ccacc")]
    [InlineData("hello", "hello", "ccccc")]
    [InlineData("world", "world", "ccccc")]
    [InlineData("about", "about", "ccccc")]
    [InlineData("abcde", "fghij", "aaaaa")]
    [InlineData("quick", "fonts", "aaaaa")]
    [InlineData("zebra", "might", "aaaaa")]
    [InlineData("abcde", "edbca", "ppppp")]
    [InlineData("heart", "earth", "ppppp")]
    [InlineData("trade", "dater", "ppppp")]
    [InlineData("spurn", "spins", "ccaap")]
    [InlineData("boost", "obese", "ppaca")]
    [InlineData("cheer", "albee", "aapca")]
    [InlineData("tests", "sweet", "pppaa")]
    [InlineData("geese", "speed", "apcpa")]
    [InlineData("books", "soaks", "acacc")]
    [InlineData("steel", "slate", "cppap")]
    [InlineData("mamma", "alarm", "ppaap")]
    [InlineData("fuzzy", "fizzy", "caccc")]
    [InlineData("eerie", "creep", "pppaa")]
    [InlineData("alley", "llama", "pcpaa")]
    [InlineData("bobby", "lobby", "acccc")]
    [InlineData("error", "erase", "ccaaa")]
    [InlineData("queen", "erase", "aappa")]
    [InlineData("seems", "geese", "pccaa")]
    [InlineData("level", "loved", "cacca")]
    [InlineData("wheel", "hello", "appap")]
    [InlineData("asses", "sassy", "ppcap")]
    [InlineData("paper", "peers", "caapp")]
    [InlineData("keeps", "speed", "apcpp")]
    [InlineData("added", "daddy", "ppcap")]
    [InlineData("adieu", "audio", "cppap")]
    [InlineData("tears", "stare", "ppccp")]
    [InlineData("roast", "toast", "acccc")]
    [InlineData("crane", "trace", "pccac")]
    [InlineData("slate", "later", "apppp")]
    [InlineData("mound", "sound", "acccc")]
    [InlineData("phone", "stone", "aaccc")]
    [InlineData("grape", "paper", "apppp")]
    [InlineData("frost", "sport", "apcpc")]
    [InlineData("abcde", "bacde", "ppccc")]
    [InlineData("tiger", "right", "pccap")]
    [InlineData("smile", "miles", "ppppp")]
    [InlineData("llama", "medal", "pappa")]
    [InlineData("daddy", "today", "apcac")]
    [InlineData("mommy", "money", "ccaac")]
    [InlineData("kayak", "kayak", "ccccc")]
    [InlineData("level", "revel", "acccc")]
    [InlineData("radar", "moral", "paaca")]
    [InlineData("geese", "genre", "ccaac")]
    [InlineData("loose", "moose", "acccc")]
    [InlineData("goose", "those", "aaccc")]
    [InlineData("start", "smart", "caccc")]
    [InlineData("brand", "grand", "acccc")]
    [InlineData("track", "crack", "acccc")]
    [InlineData("audio", "adieu", "cpppa")]
    [InlineData("aorta", "extra", "aappc")]
    [InlineData("arena", "alien", "cappa")]
    [InlineData("crypt", "truly", "acpap")]
    [InlineData("flask", "blank", "accac")]
    [InlineData("frost", "trust", "acacc")]
    [InlineData("aaaaa", "bbbbb", "aaaaa")]
    [InlineData("aaaaa", "aabbb", "ccaaa")]
    [InlineData("aaaaa", "ababa", "cacac")]
    [InlineData("eater", "trees", "papcp")]
    [InlineData("sores", "roses", "pcpcc")]
    [InlineData("least", "tales", "ppppp")]
    [InlineData("jazzy", "fizzy", "aaccc")]
    [InlineData("quake", "equal", "pppap")]
    [InlineData("xerox", "boxer", "ppppa")]
    [InlineData("baker", "break", "cpppp")]
    [InlineData("dream", "armed", "pcppp")]
    [InlineData("ocean", "canoe", "ppppp")]
    [InlineData("steam", "teams", "ppppp")]
    [InlineData("heart", "hater", "cpppp")]
    [InlineData("bread", "beard", "cpppc")]
    [InlineData("mouse", "house", "acccc")]
    [InlineData("light", "might", "acccc")]
    [InlineData("phone", "honey", "apppp")]
    [InlineData("grape", "great", "ccpap")]
    [InlineData("abbey", "babel", "ppcca")]
    [InlineData("vivid", "david", "aaccc")]
    [InlineData("civic", "panic", "aaacc")]
    [InlineData("house", "mouse", "acccc")]
    [InlineData("money", "honey", "acccc")]
    [InlineData("water", "later", "acccc")]
    [InlineData("green", "genre", "cpppp")]
    [InlineData("black", "blank", "cccac")]
    [InlineData("white", "write", "caccc")]
    [InlineData("table", "cable", "acccc")]
    [InlineData("chair", "chain", "cccca")]
    [InlineData("floor", "color", "appcc")]
    [InlineData("xylem", "lemon", "aappp")]
    [InlineData("fjord", "major", "apppa")]
    [InlineData("waltz", "blitz", "aapcc")]
    [InlineData("nymph", "lymph", "acccc")]
    [InlineData("apple", "apple", "ccccc")]
    [InlineData("apple", "apron", "ccaaa")]
    [InlineData("crane", "crown", "ccapa")]
    [InlineData("flame", "frame", "caccc")]
    [InlineData("spore", "store", "caccc")]
    [InlineData("latch", "catch", "acccc")]
    [InlineData("stone", "notes", "ppppp")]
    [InlineData("sling", "cling", "acccc")]
    [InlineData("trend", "tread", "cccac")]
    [InlineData("greet", "great", "cccac")]
    [InlineData("piano", "paint", "cppca")]
    [InlineData("shiny", "spiny", "caccc")]
    [InlineData("beast", "feast", "acccc")]
    [InlineData("frost", "front", "cccac")]
    [InlineData("brave", "grape", "accac")]
    [InlineData("tiger", "timer", "ccacc")]
    [InlineData("night", "thing", "ppppp")]
    [InlineData("mango", "gamin", "pcppa")]
    [InlineData("click", "clock", "ccacc")]
    [InlineData("blunt", "stunt", "aaccc")]
    [InlineData("crazy", "brazy", "acccc")]
    [InlineData("happy", "harpy", "ccacc")]
    [InlineData("quick", "quack", "ccacc")]
    [InlineData("lucky", "pucky", "acccc")]
    [InlineData("vocal", "local", "acccc")]
    [InlineData("drink", "bring", "accca")]
    [InlineData("fable", "table", "acccc")]
    [InlineData("speak", "sneak", "caccc")]
    [InlineData("plant", "plate", "cccap")]
    [InlineData("stone", "tones", "ppppp")]
    [InlineData("shout", "touch", "apppp")]
    [InlineData("brace", "trace", "acccc")]
    [InlineData("sugar", "guars", "pcppp")]
    [InlineData("glide", "slide", "acccc")]
    [InlineData("trick", "brick", "acccc")]
    [InlineData("froze", "forze", "cppcc")]
    [InlineData("beach", "reach", "acccc")]
    [InlineData("brush", "crush", "acccc")]
    [InlineData("candy", "dandy", "acccc")]
    [InlineData("flock", "block", "acccc")]
    [InlineData("proud", "round", "apppc")]
    [InlineData("drain", "brain", "acccc")]
    [InlineData("spike", "skimp", "cpcpa")]
    [InlineData("trend", "tends", "cappp")]
    public void TestExperimentalLogic(string guessString, string targetString, string statesString)
    {
        uint guess = EncodingHelper.PackFromString(guessString);
        uint target = EncodingHelper.PackFromString(targetString);
        var statesBlock = statesString.ToCharArray().Select(x => x switch
        {
            'a' => BlockState.Absent,
            'p' => BlockState.Present,
            'c' => BlockState.Correct,
            _ => throw new Exception("invalid format")
        }).ToArray();
        uint states = EncodingHelper.PackStates(statesBlock);
        uint processed = GameEngine.ProcessWordExperimental(guess, target);
        var decodedStates = DecodeStatesExplicit(processed);
        Assert.Equal(statesString, decodedStates);
    }

    private static string DecodeStatesExplicit(uint states)
    {
        // Extract each position's 2 bits explicitly
        uint pos0 = states & 0x3;         // bits 0-1
        uint pos1 = (states >> 2) & 0x3;  // bits 2-3
        uint pos2 = (states >> 4) & 0x3;  // bits 4-5
        uint pos3 = (states >> 6) & 0x3;  // bits 6-7
        uint pos4 = (states >> 8) & 0x3;  // bits 8-9

        char[] result = {
        pos0 switch { 0 => 'a', 1 => 'p', 2 => 'c', _ => '?' },
        pos1 switch { 0 => 'a', 1 => 'p', 2 => 'c', _ => '?' },
        pos2 switch { 0 => 'a', 1 => 'p', 2 => 'c', _ => '?' },
        pos3 switch { 0 => 'a', 1 => 'p', 2 => 'c', _ => '?' },
        pos4 switch { 0 => 'a', 1 => 'p', 2 => 'c', _ => '?' }
    };

        return new string(result);
    }
}
