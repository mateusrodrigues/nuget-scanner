using System.CommandLine;

namespace NuGetScanner.Console;

public class Program
{
    public static int Main(string[] args)
    {
        var projectOption = new Option<string[]>(
            "--project",
            "The path to the project file to scan.")
        {
            IsRequired = false,
            Arity = ArgumentArity.ZeroOrMore
        };

        var rootCommand = new RootCommand("NuGet Scanner CLI");
        rootCommand.AddOption(projectOption);

        var success = true;
        rootCommand.SetHandler(async (project) =>
        {
            try
            {
                var scanner = new Scanner(project);
                scanner.Scan();

                foreach (var proj in scanner.Projects)
                {
                    System.Console.WriteLine($"Project: {proj.Path}");

                    await proj.CheckForUpdates();
                    foreach (var package in proj.CurrentPackages)
                    {
                        System.Console.WriteLine(proj.AvailableUpdates.TryGetValue(package.Key, out var update)
                            ? $"\tPackage: {package.Key} - Current: {package.Value} - Available: {update}"
                            : $"\tPackage: {package.Key} - Current: {package.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine($"ERROR: {e.Message}");
                success = false;
                throw;
            }
        }, projectOption);

        rootCommand.Invoke(args);

        return success ? 0 : 1;
    }
}
