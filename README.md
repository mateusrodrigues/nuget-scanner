# nuget-scanner

A .NET tool that scans all NuGet packages in a solution and reports available updates. 🚀

## Features
- Scans all projects within a solution.
- Detects outdated NuGet packages.
- Outputs a report showing which packages have updates available.
- Supports checking for pre-release versions.

## Installation
You can install the tool globally using:

```sh
dotnet tool install --global nuget-scanner
```

Or install it as a local tool within a project:

```sh
dotnet new tool-manifest # If not already created
dotnet tool install --local nuget-scanner
```

## Usage
Run the tool in the root of your solution:

```sh
nuget-scanner
```

### Options
- `--project <path>`: Specify a specific project or solution file to scan.
- `--include-prerelease`: Include pre-release versions when checking for updates.

> [!NOTE]
> Packages that have a pre-release version installed are automatically checked with
> pre-release enabled.

## Building from Source
To build the project locally:

```sh
dotnet restore
dotnet build --configuration Release
```

## Contributing
Contributions are welcome! Feel free to submit issues or pull requests.

## License
This project is licensed under the [GPLv3 License](LICENSE).

