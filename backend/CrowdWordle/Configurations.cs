namespace CrowdWordle;

public sealed class GameConfiguration
{
    public TimeSpan VotingDuration { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan GameInterval { get; set; } = TimeSpan.FromSeconds(10);
    public byte MaxRounds { get; set; } = 6;
}

public sealed class TokenConfiguration
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CrowdWordle";
    public string Audience { get; set; } = "CrowdWordle.Client";
}