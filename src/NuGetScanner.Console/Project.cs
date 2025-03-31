using System.Xml.Linq;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetScanner.Console.Types;

namespace NuGetScanner.Console;

public class Project
{
    private readonly RegistrationResourceV3 _registrationResource;

    private readonly Dictionary<string, NuGetVersion> _currentPackages;
    public IReadOnlyDictionary<string, NuGetVersion> CurrentPackages => _currentPackages;

    private readonly Dictionary<string, NuGetVersion> _availableUpdates;
    public IReadOnlyDictionary<string, NuGetVersion> AvailableUpdates => _availableUpdates;

    public string Path { get; }

    public Project(string path)
    {
        Path = path;

        _currentPackages = GetNuGetPackagesFromProject();
        _availableUpdates = [];

        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        _registrationResource = repository.GetResourceAsync<RegistrationResourceV3>()
            .GetAwaiter()
            .GetResult();
    }

    public async Task CheckForUpdates(CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task<PackageDetails?>>();
        foreach (var (packageId, _) in _currentPackages)
        {
            tasks.Add(GetLatestAvailablePackageVersion(packageId, cancellationToken: cancellationToken));
        }

        await Task.WhenAll(tasks);

        foreach (var packageDetail in tasks.Select(t => t.Result))
        {
            if (packageDetail is null)
            {
                continue;
            }

            if (packageDetail.Version > _currentPackages[packageDetail.Id])
            {
                _availableUpdates.Add(packageDetail.Id, packageDetail.Version);
            }
        }
    }

    private async Task<PackageDetails?> GetLatestAvailablePackageVersion(string packageId,
        bool includePrerelease = false, bool includeUnlisted = false, CancellationToken cancellationToken = default)
    {
        var response = await _registrationResource.GetPackageMetadata(
            packageId,
            includePrerelease,
            includeUnlisted,
            NullSourceCacheContext.Instance,
            NullLogger.Instance,
            cancellationToken);

        var packageDetails = response?.LastOrDefault()?.ToObject<PackageDetails>(new JsonSerializer
        {
            Converters = { new NuGetVersionConverter() }
        });

        return packageDetails;
    }

    private Dictionary<string, NuGetVersion> GetNuGetPackagesFromProject()
    {
        var packages = new Dictionary<string, NuGetVersion>();

        var doc = XDocument.Load(Path);
        var packageReferences = doc.Descendants("PackageReference")
            .Select(pr => new
            {
                Include = pr.Attribute("Include")?.Value,
                Version = pr.Attribute("Version")?.Value
            }).ToList();

        packageReferences.ForEach(pkg =>
        {
            var packageName = pkg.Include;
            if (!string.IsNullOrWhiteSpace(packageName) &&
                NuGetVersion.TryParse(pkg.Version, out var packageVersion))
            {
                packages.Add(packageName, packageVersion);
            }
        });

        return packages;
    }
}
