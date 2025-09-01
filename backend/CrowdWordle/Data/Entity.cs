namespace CrowdWordle.Data;

public record Migration(int Id, string Name, DateTimeOffset AppliedAt);
public record GameRecord(long Id, DateTimeOffset EndedTime, uint SelectedWord, byte Round, bool Won, int TotalUsers);
public record RoundRecord(long GameId, byte Round, uint PlayedWord, uint TotalVotes);
public record VoteRecord(long GameId, byte Round, byte VoteIndex, uint VotedWord, uint Votes);
public record SystemRecord(ulong UserIdIndex, ulong HighestUserCount);