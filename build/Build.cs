using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tooling.ProcessTasks;

[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    static AbsolutePath Source => RootDirectory / "src";
    static AbsolutePath Demo => RootDirectory / "demo";
    static AbsolutePath PublishDir => RootDirectory / "publish";
    static readonly string DOTNET = "dotnet";
    static readonly string NODE = "node";
    static readonly string NPM = "npm";

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => task => 
      task  
        .Executes(() =>
        {
            var directories = new List<string> {
                Source / "bin",
                Source / "obj", 
                Demo / "bin",
                Demo / "obj",
                PublishDir
            };

            foreach(var dir in directories) {
                DeleteDirectory(dir);
            }
        });

    Target Restore => task =>
      task
        .DependsOn(Clean)
        .Executes(() => 
        {
            StartProcess(NPM, "install", RootDirectory).AssertZeroExitCode();
        });

    Target Compile => task =>
      task
        .DependsOn(Restore)
        .Executes(() => {
            StartProcess(DOTNET, "restore --no-cache", Source).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", Demo).AssertZeroExitCode();
            StartProcess(NPM, "run build", RootDirectory).AssertZeroExitCode();
        });

    Target Pack => task =>
      task 
        .DependsOn(Clean)
        .Executes(() => 
        {
            var packCmd = $"pack -c Release -o {PublishDir}";
            StartProcess(DOTNET, packCmd, Source).AssertZeroExitCode();
        });

    Target Publish => task =>
      task
        .DependsOn(Pack) 
        .Executes(() => 
        {
            var nugetFile = Directory.GetFiles(PublishDir).FirstOrDefault() ?? "";
            if (!nugetFile.EndsWith(".nupkg"))
            {
                Logger.Error("No nuget package found");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            Logger.Info($"About to publish nuget package: {nugetFile}");
            var nugetApiKey = EnsureVariable("NUGET_KEY") ?? "";

            if (string.IsNullOrWhiteSpace(nugetApiKey))
            {
                Logger.Error("Nuget API Key was not setup on your local machine, missing environment variable NUGET_KEY");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            var nugetFileName = new FileInfo(nugetFile).Name;
            StartProcess(DOTNET, $"nuget push {nugetFileName} -s https://api.nuget.org/v3/index.json -k {nugetApiKey}", PublishDir).AssertZeroExitCode();
        });
}
