//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var projectDir = Directory("./src/CompressedStaticFiles");

var buildDir = projectDir + Directory("bin") + Directory(configuration);

// Define files.
var projectFile = projectDir + File("project.json");

//////////////////////////////////////////////////////////////////////
// Output variables
//////////////////////////////////////////////////////////////////////

Information("target: {0}", target);
Information("configuration: {0}", configuration);
Information("projectDir: {0}", projectDir);
Information("buildDir: {0}", buildDir);
Information("projectFile: {0}", projectFile);

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
    DotNetCoreRestore(projectFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings() {
        ArgumentCustomization = args => args.Append("--configuration " + configuration)
    };
    DotNetCoreBuild(projectFile, settings);
});

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings() {
        ArgumentCustomization = args => args.Append("--configuration " + configuration)
    };
    DotNetCorePack(projectFile, settings);
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Create-NuGet-Packages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);