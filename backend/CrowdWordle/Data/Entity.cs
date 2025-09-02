namespace CrowdWordle.Data;

public record Migration(int Id, string Name, DateTimeOffset AppliedAt);
public record GameRecord(long Id, DateTimeOffset StartedTime, uint SelectedWord, byte Round, uint PlayedWord1, uint PlayedWord2, uint PlayedWord3, uint PlayedWord4, uint PlayedWord5, uint PlayedWord6, bool Won, DateTimeOffset EndedTime, int TotalUsers);
public record SystemRecord(ulong UserIdIndex, ulong HighestUserCount);