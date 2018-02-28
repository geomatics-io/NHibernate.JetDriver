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
    var nuGetPackSettings   = new NuGetPackSettings {
        Id                      = "Geomatics.IO.NHibernate.JetDriver",
        Version                 = "4.1.1.4000-b10",
        Title                   = "Geomatics.IO.NHibernate.JetDriver",
        Owners                  = new[] {"christianjunk", "geomatics"},
        Authors                 = new[] {"Christian Junk", "NHibernate.org"},
        Description             = "The Jet driver and dialect for NHibernate.",
        //Summary                 = "Excellent summary of what the package does",
        ProjectUrl              = new Uri("https://github.com/geomatics-io/NHibernate.JetDriver"),
        IconUrl                 = new Uri("https://github.com/geomatics-io/NHibernate.JetDriver/raw/master/NHibernate.JetDriver.png"),
        LicenseUrl              = new Uri("https://raw.githubusercontent.com/geomatics-io/NHibernate.JetDriver/master/LICENSE.md"),
        //Copyright               = "Some company 2015",
        //ReleaseNotes            = new [] {"Bug fixes", "Issue fixes", "Typos"},
        Tags                    = new [] {"NHibernate", "jet", "driver", "csharp", "csharp-library", "dotnet"},
        RequireLicenseAcceptance= false,
        //Symbols                 = false,
        NoPackageAnalysis       = true,
        Files                   = new [] {new NuSpecContent {Source = "NHibernate.JetDriver.dll", Target = "bin"},},
        BasePath                = "./src/NHibernate.JetDriver/bin/Release",
        OutputDirectory         = "./artifacts",
        IncludeReferencedProjects = true,
        Properties = new Dictionary<string, string>
        {
            { "Configuration", "Release" }
        },
        Dependencies            = new []{ new NuSpecDependency {
                                             Id              = "NHibernate",
                                             Version         = "[4.0.0.4000,4.1.1.4000]"
                                          },
                                          new NuSpecDependency {
                                             Id              = "log4net",
                                             Version         = "[2.0.0,2.0.8]"
                                          },
                                        },      
    };

    MSBuild("./src/NHibernate.JetDriver.Tests/NHibernate.JetDriver.Tests.csproj", new MSBuildSettings().SetConfiguration("Release").SetMSBuildPlatform(MSBuildPlatform.x86));

    MSBuild("./src/NHibernate.JetDriver/NHibernate.JetDriver.csproj", new MSBuildSettings().SetConfiguration("Release"));
    NuGetPack(nuGetPackSettings);
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
