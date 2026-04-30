namespace MotorCare.App.Configuration;

public sealed class BuildInfoOptions
{
    public const string SectionName = "BuildInfo";

    public string Version { get; set; } = "v0.2";

    public string Sprint { get; set; } = "Sprint 2";

    public string CommitSha { get; set; } = "unknown";

    public string BuildTime { get; set; } = "unknown";
}
