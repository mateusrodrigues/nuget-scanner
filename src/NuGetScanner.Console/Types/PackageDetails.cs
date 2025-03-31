using NuGet.Versioning;

namespace NuGetScanner.Console.Types;

public class PackageDetails
{
    public required string Id { get; set; }
    public required NuGetVersion Version { get; set; }
}
