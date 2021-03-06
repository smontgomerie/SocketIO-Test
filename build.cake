#tool nuget:?package=NUnit.ConsoleRunner&version=3.12.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build-SocketIO")
    .Does(() =>
{
    var settings = new MSBuildSettings
    {
        Configuration = "Release",
        PlatformTarget = PlatformTarget.MSIL,
        Restore = true
    };
    
    MSBuild("SocketIO.csproj", settings);
});

//////////////////////////////////////////////////////////////////////
// Utilities
//////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build-SocketIO");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
