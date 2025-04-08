using NuGet.Versioning;

namespace Mateus.Tech.NuGetScanner.Types;

public class PackageDetails
{
    public required string Id { get; set; }
    public required NuGetVersion Version { get; set; }
}
