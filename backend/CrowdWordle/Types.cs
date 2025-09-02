using System.Runtime.InteropServices;

namespace CrowdWordle;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Game
{
    public PlayedWord Word0;
    public PlayedWord Word1;
    public PlayedWord Word2;
    public PlayedWord Word3;
    public PlayedWord Word4;
    public PlayedWord Word5;
    public GameState State;
    public uint SelectedWord;
    public byte Round;
    public DateTime StartedTime;

    public readonly bool GameOver => State is GameState.Won or GameState.Lost;

    public readonly PlayedWord GetWord(int index) => index switch
    {
        0 => Word0,
        1 => Word1,
        2 => Word2,
        3 => Word3,
        4 => Word4,
        5 => Word5,
        _ => default
    };

    public void SetWord(int index, PlayedWord word)
    {
        switch (index)
        {
            case 0: Word0 = word; break;
            case 1: Word1 = word; break;
            case 2: Word2 = word; break;
            case 3: Word3 = word; break;
            case 4: Word4 = word; break;
            case 5: Word5 = word; break;
        }
    }
}

public enum GameState : byte
{
    WaitingForVote,
    VotingInProgress,
    Won,
    Lost
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Word
{
    public uint Packed;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayedWord
{
    public uint Packed;
    public uint States; // 2 bits per state, 5 states = 10 bits total
}

public enum BlockState : byte
{
    Absent,
    Present,
    Correct
}

public enum RequestResponseType : byte
{
    Success,
    VotingNotAllowed,
    InvalidVote,
    AlreadyVoted
}

public record Status(uint UserCount, string? GameState, byte Round);

public enum ServerMessageType : byte
{
    InitialState,
    VotingStarted,
    GameUpdate,
    LiveData,
    GameStarting,
    VoteStream,
    Response
}

public enum ClientMessageType : byte
{
    Vote,
    RequestInitial,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vote
{
    public uint Word;
    public uint Count;
}

public struct GameStateUpdate
{
    public GameUpdateType UpdateType;
    public PlayedWord PlayedWord;
    public bool IsWin;
    public Word TargetWord;
}

public enum GameUpdateType : byte
{
    NoChange,
    WordPlayed,
    GameEnd
}

public sealed record VerifyRequest(string? Token);