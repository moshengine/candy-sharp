using System.IO;
using System.Linq;
using UnityEditor;

public static class BuildTemplate
{
    public static void BuildAndroid(string basePath, string displayName, bool production)
    {
        string fileExtension;
        BuildOptions options;

        // Warning: Not tested yet
        if (production)
        {
            fileExtension = "aab";
            options = BuildOptions.None;
            EditorUserBuildSettings.buildAppBundle = true;
        }
        // Warning: Not tested yet
        else
        {
            fileExtension = "apk";
            options = BuildOptions.AutoRunPlayer;
            EditorUserBuildSettings.buildAppBundle = false;
        }

        Build(basePath, "android", BuildTarget.Android, $"{displayName}.{fileExtension}", options);
    }

    public static void BuildIOS(string basePath, string displayName, bool production)
    {
        BuildOptions options;

        // Warning: Not tested yet
        if (production)
        {
            options = BuildOptions.None;
        }
        // Warning: Not tested yet
        else
        {
            options = BuildOptions.None;
        }

        Build(basePath, "ios", BuildTarget.iOS, $"{displayName}.xcodeproj", options);
    }

    public static void BuildWebGL(string basePath, string internalName, bool production)
    {
        BuildOptions options;

        // Warning: Not tested yet
        if (production)
        {
            options = BuildOptions.None;
        }
        // Warning: Not tested yet
        else
        {
            options = BuildOptions.AutoRunPlayer;
        }

        Build(basePath, "webgl", BuildTarget.WebGL, internalName, options);
    }

    private static void Build(
        string basePath,
        string outputDirectoryName,
        BuildTarget buildTarget,
        string executableFileName,
        BuildOptions options = BuildOptions.None)
    {
        var buildDirectory = Path.Combine(basePath, outputDirectoryName);
        Directory.CreateDirectory(buildDirectory);

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray(),
            locationPathName = Path.Combine(buildDirectory, executableFileName),
            target = buildTarget,
            options = options
        });
    }
}