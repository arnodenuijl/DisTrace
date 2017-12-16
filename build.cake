#addin "nuget:?package=NuGet.Core&version=2.8.6"
#addin "Cake.FileHelpers"
#tool "nuget:?package=xunit.runner.console&version=2.2.0"

var target        = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildDir      = Directory("./build");
var solution      = "./DisTrace.sln";


Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("RestorePackages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    MSBuild(solution, settings => settings
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
    );
});

Task("RunTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2($"./test/DisTrace.AspNetCore.Tests/bin/{configuration}/**/*Tests.dll");
    XUnit2($"./test/DisTrace.WebApi.Tests/bin/{configuration}/**/*Tests.dll");
});

Task("Default")
    .IsDependentOn("RunTests");

RunTarget(target);