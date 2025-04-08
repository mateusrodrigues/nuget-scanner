using Microsoft.Build.Framework;

namespace Build.Tasks;

public class ReleaseNotesTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public required string ChangelogFile { get; set; }

    [Required]
    public required string VersionPrefix { get; set; }

    [Required]
    public required string OutputFile { get; set; }

    public override bool Execute()
    {
        try
        {
            var lines = File.ReadAllLines(ChangelogFile);
            var versionTag = $"v{VersionPrefix}";
            var inVersion = false;

            var releaseNotes = lines.TakeWhile(line =>
                {
                    if (line.StartsWith('v') && line != versionTag)
                    {
                        if (inVersion) return false;
                    }
                    if (line == versionTag) inVersion = true;
                    return true;
                })
                .Where(line => inVersion && !string.IsNullOrWhiteSpace(line))
                .ToList();

            File.WriteAllLines(OutputFile, releaseNotes);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to extract release notes: {ex.Message}");
            return false;
        }
    }
}
