using System.CommandLine;
using Spectre.Console;

namespace NuGetScanner.Console;

public class Program
{
    public static int Main(string[] args)
    {
        var projectOption = new Option<string[]>(
            "--project",
            "The path to the solution or project file to scan.")
        {
            IsRequired = false,
            Arity = ArgumentArity.ZeroOrMore
        };

        var includePrereleaseOption = new Option<bool>(
            "--include-prerelease",
            "Include pre-release versions in the scan.")
        {
            IsRequired = false,
            Arity = ArgumentArity.Zero
        };

        var rootCommand = new RootCommand("NuGet Scanner CLI");
        rootCommand.AddOption(projectOption);
        rootCommand.AddOption(includePrereleaseOption);

        var success = true;
        rootCommand.SetHandler(async (project, includePrerelease) =>
        {
            try
            {
                var scanner = new Scanner(project);
                scanner.Scan();

                foreach (var proj in scanner.Projects)
                {
                    var projectTree = new Tree($"[bold]{proj.Path}[/]");
                    await proj.CheckForUpdates(includePrerelease);

                    foreach (var package in proj.CurrentPackages)
                    {
                        projectTree.AddNode(proj.AvailableUpdates.TryGetValue(package.Key, out var update)
                            ? $"{package.Key} [yellow][[{package.Value}]][/] :right_arrow: [bold][blue][[{update}]][/][/]"
                            : $"{package.Key} [green][[{package.Value}]][/]");
                    }

                    AnsiConsole.Write(projectTree);
                }
            }
            catch (Exception e)
            {
                await System.Console.Error.WriteLineAsync($"ERROR: {e.Message}");
                success = false;
                throw;
            }
        }, projectOption, includePrereleaseOption);

        rootCommand.Invoke(args);

        return success ? 0 : 1;
    }
}
