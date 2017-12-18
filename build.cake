#addin "nuget:?package=NuGet.Core&version=2.8.6"
#addin "Cake.FileHelpers"
#addin nuget:?package=Cake.Git
#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"

var createPackage = Argument("createPackage", false);
var target        = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildDir      = Directory("./build");
var solution      = "./DisTrace.sln";

var branch = GitBranchCurrent(DirectoryPath.FromString("."));
var branchName = branch.FriendlyName;
var sha = branch.Tip.Sha.Substring(0,7);

GitVersion gitVersion;

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


Task("ShowVersion")
    .IsDependeeOf("Build")
    .Does(() => 
    {
        gitVersion = GitVersion();
   
        Information(gitVersion.FullSemVer);
        Information(gitVersion.InformationalVersion );
        Information(gitVersion.AssemblySemVer );
        Information(gitVersion.SemVer);        
    });

Task("Patch")
    .IsDependeeOf("Build")
    .WithCriteria(createPackage)
    .Does(() => 
    {
        gitVersion = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true
        });

                ReplaceRegexInFiles("./**/*.csproj",  
                        @"\<Version\>.*\<\/Version\>", 
                        @"<Version>"+ gitVersion.SemVer + "</Version>");
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
    var projects = GetFiles("./test/DisTrace.AspNetCore.Tests/*.csproj");
        foreach(var project in projects)
        {
            DotNetCoreTest(
                project.FullPath,
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true
                });
        }

    XUnit2($"./test/DisTrace.WebApi.Tests/bin/{configuration}/**/*Tests.dll");
});

Task("Pack")
    .IsDependentOn("RunTests")
    .WithCriteria(createPackage)
    .Does(() => 
    {
        var settings = new NuGetPackSettings 
        {
            OutputDirectory = buildDir.Path,
            Version = gitVersion.AssemblySemVer,
            Properties = new Dictionary<string, string>(){
                {
                    "Configuration", configuration
                }
            }
        };
        NuGetPack("./src/DisTrace.WebApi/DisTrace.WebApi.csproj", settings);
        NuGetPack("./src/DisTrace.WebApi.SeriLog/DisTrace.WebApi.SeriLog.csproj", settings);
        
        var dotNetCorePackSettings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = buildDir,
            MSBuildSettings = new DotNetCoreMSBuildSettings().SetVersion(gitVersion.AssemblySemVer)
        };

        DotNetCorePack("./src/DisTrace.Core/DisTrace.Core.csproj", dotNetCorePackSettings);
        DotNetCorePack("./src/DisTrace.AspNetCore/DisTrace.AspNetCore.csproj", dotNetCorePackSettings);
        DotNetCorePack("./src/DisTrace.AspNetCore.SeriLog/DisTrace.AspNetCore.SeriLog.csproj", dotNetCorePackSettings);
        DotNetCorePack("./src/DisTrace.HttpClient", dotNetCorePackSettings);
    });

Task("Push")
    .IsDependentOn("Pack")
    .WithCriteria(createPackage)
    .WithCriteria(() => new string[] {"master", "develop"}.Contains(gitVersion.BranchName) )
    .Does(() =>
    {
            // Get the paths to the packages.
            var packages = GetFiles($"./{buildDir}/*.nupkg");
            if(gitVersion.BranchName == "develop") {
                // Push the package.
                NuGetPush(packages, new NuGetPushSettings {
                    Source = "https://www.myget.org/F/distrace/api/v2/package",
                    ApiKey = EnvironmentVariable("MYGET_APIKEY")
                });
            }
            else if(gitVersion.BranchName == "master") {
                // Push the package.
                NuGetPush(packages, new NuGetPushSettings {
                    Source = "https://api.nuget.org/v3/index.json",
                    ApiKey = EnvironmentVariable("NUGET_APIKEY")
                });
            }

    });
Task("Default")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

RunTarget(target);