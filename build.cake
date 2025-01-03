#tool "dotnet:?package=GitVersion.Tool&version=5.12.0"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// Setup
///////////////////////////////////////////////////////////////////////////////
string solution = "Cake.GitHooks.sln";
GitVersion version;
string buildId;

var dotNetVerbosity = DotNetVerbosity.Minimal;

var msBuildSettings = new DotNetMSBuildSettings()
    .SetMaxCpuCount(0);

Setup(context =>
{
    if (context == null)
        Error("Context null");

    version = context.GitVersion();

    if (version == null)
        Error("Could not determine version from GitVersion.");

    Information($"Solution: {solution}");
    Information($"Version:  {version.FullSemVer}");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("CleanArtifacts")
    .Does(() =>
{
    CleanDirectory("./artifacts");
    CreateDirectory("./artifacts/nuget");
});

Task("Clean")
   .WithCriteria(c => HasArgument("rebuild"))
   .Does(() =>
{
   var objs = GetDirectories($"./**/obj");
   var bins = GetDirectories($"./**/bin");

   CleanDirectories(objs.Concat(bins));
});

Task("RestorePackages")
    .Does(() =>
{
    DotNetRestore(new DotNetRestoreSettings 
        { 
            Verbosity = dotNetVerbosity,
            MSBuildSettings = msBuildSettings,
        });
});

Task("Compile")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    var buildSettings = new DotNetBuildSettings
        {
            Configuration = configuration,
            NoRestore = true,
            Verbosity = dotNetVerbosity,
            MSBuildSettings = msBuildSettings,
        };

    DotNetBuild(solution, buildSettings);
});

Task("Publish")
    .IsDependentOn("Compile")
    .Does(() => {

        // Get all projects that have a Dockerfile in the same directory.
        var projectFiles = GetFiles("**/*Cake*.csproj");

        foreach(var project in projectFiles)
        {
            Information($"Publish {project}...");

            var publishDirectory = $"./artifacts/{project.GetFilenameWithoutExtension()}";

            DotNetPublish(
                project.FullPath,
                new DotNetPublishSettings
                {
                    Configuration = configuration,
                    NoRestore = true,
                    NoBuild = true,
                    Verbosity = dotNetVerbosity,
                    MSBuildSettings = msBuildSettings,
                    OutputDirectory = publishDirectory
                }
            );

            DotNetPack(
                project.FullPath,
                new DotNetPackSettings
                {
                    Configuration = configuration,
                    NoRestore = true,
                    NoBuild = true,
                    Verbosity = dotNetVerbosity,
                    MSBuildSettings = msBuildSettings,
                    OutputDirectory = "./artifacts/nuget",
                    IncludeSymbols = false
                }
            );
        }
    });

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("CleanArtifacts")
  .IsDependentOn("Publish");

RunTarget(target);