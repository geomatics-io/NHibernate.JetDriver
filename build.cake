#tool nuget:?package=NUnit.Runners&version=2.6.4
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/NHibernate.JetDriver/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/NHibernate.JetDriver.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/NHibernate.JetDriver.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./src/NHibernate.JetDriver.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnitSettings {
        NoResults = true, X86 = true
        });
});

Task("BuildPackages")
    .IsDependentOn("Build")
    //.IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    var nuGetPackSettings = new NuGetPackSettings
    {
        OutputDirectory = "./artifacts",
        IncludeReferencedProjects = true,
        Properties = new Dictionary<string, string>
        {
            { "Configuration", "Release" }
        }
    };

    MSBuild("./src/NHibernate.JetDriver.Tests/NHibernate.JetDriver.Tests.csproj", new MSBuildSettings().SetConfiguration("Release").SetMSBuildPlatform(MSBuildPlatform.x86));

    MSBuild("./src/NHibernate.JetDriver/NHibernate.JetDriver.csproj", new MSBuildSettings().SetConfiguration("Release"));
    NuGetPack("./src/NHibernate.JetDriver/NHibernate.JetDriver.csproj", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("BuildPackages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
