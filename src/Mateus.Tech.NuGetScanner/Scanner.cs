using System.Collections.ObjectModel;
using Microsoft.Build.Construction;
using Mateus.Tech.NuGetScanner.Exceptions;

namespace Mateus.Tech.NuGetScanner;

public class Scanner
{
    private readonly HashSet<string> _projectPaths = [];
    public ReadOnlyCollection<string> ProjectPaths => _projectPaths.ToList().AsReadOnly();

    private readonly List<Project> _projects = [];
    public ReadOnlyCollection<Project> Projects => _projects.ToList().AsReadOnly();

    public Scanner()
    { }

    public Scanner(params string[] projects)
    {
        foreach (var project in projects.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            _projectPaths.Add(project);
        }
    }

    public void Scan()
    {
        if (_projectPaths.Count == 0)
        {
            foreach (var project in FindProjects())
            {
                _projectPaths.Add(project);
            }
        }
        else
        {
            foreach (var projectPath in _projectPaths.ToList())
            {
                if (!projectPath.EndsWith(".sln")) continue;

                var projects = GetProjectsFromSlnFile(projectPath);

                _projectPaths.Remove(projectPath);
                foreach (var project in projects)
                {
                    _projectPaths.Add(project);
                }
            }
        }

        _projects.AddRange(_projectPaths.Select(path => new Project(path)));
    }

    private static string[] FindProjects()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var csprojFiles = Directory.GetFiles(currentDirectory, "*.*proj", SearchOption.TopDirectoryOnly);
        var slnFile = Directory.GetFiles(currentDirectory, "*.sln", SearchOption.TopDirectoryOnly);

        if (slnFile.Length > 0)
        {
            var projects = GetProjectsFromSlnFile(slnFile[0]);
            return projects;
        }

        var project = csprojFiles.Length switch
        {
            0 => throw new NuGetScannerException("No project file found."),
            _ => csprojFiles
        };

        return project;
    }

    private static string[] GetProjectsFromSlnFile(string slnFile)
    {
        if (!File.Exists(slnFile))
        {
            throw new NuGetScannerException($"Solution file '{slnFile}' does not exist.");
        }

        var solution = SolutionFile.Parse(slnFile);
        return solution.ProjectsInOrder
            .Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
            .Select(p => p.AbsolutePath)
            .ToArray();
    }
}
