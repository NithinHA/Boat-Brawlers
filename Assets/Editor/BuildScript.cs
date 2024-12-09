using System.IO;
using UnityEditor;
using System;

public class BuildScript
{
    private const string PLATFORM_ANDROID = "Android";
    private const string PLATFORM_WIN64 = "Win64";
    private const string PLATFORM_WEBGL = "WebGL";

    private const string CMD_LINE_BUILD_TARGET = "buildTarget";

    /// <summary>
    /// This function gets called from Jenkins. It must have 0 parameters.
    /// </summary>
    public static void PerformBuild()
    {
        string buildTargetStr = GetArgument(CMD_LINE_BUILD_TARGET);    // Read the build target from command-line arguments
        if (string.IsNullOrEmpty(buildTargetStr))
        {
            throw new ArgumentException("Build target not specified. Use -buildTarget <platform>.");
        }
        ProcessBuildTargetAndTrigger(buildTargetStr);
    }

    /// <summary>
    /// This is a common function used to trigger build using Editor ContextMenus.
    /// </summary>
    private static void PerformBuild_local(string buildTargetStr = null)
    {
        ProcessBuildTargetAndTrigger(buildTargetStr);
    }

    private static void ProcessBuildTargetAndTrigger(string buildTargetStr)
    {
        string buildPath = $"Builds/{buildTargetStr}/";   // Build destination folder
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        BuildTarget buildTarget;
        switch (buildTargetStr)
        {
            case PLATFORM_ANDROID:
                buildTarget = BuildTarget.Android;
                buildPath += "TidalTakedown.apk";
                break;
            case PLATFORM_WIN64:
                buildTarget = BuildTarget.StandaloneWindows64;
                buildPath += "TidalTakedown.exe";
                break;
            case PLATFORM_WEBGL:
                buildTarget = BuildTarget.WebGL;
                break;
            default:
                throw new ArgumentException($"Unsupported build target: {buildTargetStr}");
        }

        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, buildTarget, BuildOptions.None);
        UnityEngine.Debug.Log("Build completed successfully!");
    }

    /// <summary>
    /// Retrives command line arguments.
    /// </summary>
    private static string GetArgument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == $"-{name}" && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null;
    }

#region Context Menus

    [MenuItem("Tools/Build/Win64")]
    public static void BuildForWindows()
    {
        PerformBuild_local(PLATFORM_WIN64);
    }

    [MenuItem("Tools/Build/Android")]
    public static void BuildForAndroid()
    {
        PerformBuild_local(PLATFORM_ANDROID);
    }

    [MenuItem("Tools/Build/WebGL")]
    public static void BuildForWebGL()
    {
        PerformBuild_local(PLATFORM_WEBGL);
    }

#endregion
}
