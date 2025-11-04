using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Candy.Unity.Editor
{
    /// <summary>
    /// Toggles package dependencies between external (GitHub) and local (file://) paths.
    /// Useful for development workflow: use local paths when iterating on candy-sharp packages,
    /// then switch to external paths before making a pull request.
    /// </summary>
    public static class PackageDependencyToggler
    {
        private const string GITHUB_BASE_URL = "https://github.com/glowdragon/candy-sharp.git?path=";
        private const string LOCAL_BASE_PATH = "file:../../../../candy-sharp/";
        private const string MANIFEST_PATH = "Packages/manifest.json";

        [MenuItem("Tools/Candy/Package Management/Toggle to Local")]
        public static void ToggleToLocal()
        {
            ToggleDependencies(toLocal: true);
        }

        [MenuItem("Tools/Candy/Package Management/Toggle to External")]
        public static void ToggleToExternal()
        {
            ToggleDependencies(toLocal: false);
        }

        [MenuItem("Tools/Candy/Package Management/Toggle to Local", true)]
        [MenuItem("Tools/Candy/Package Management/Toggle to External", true)]
        private static bool ValidateToggleDependencies()
        {
            return File.Exists(MANIFEST_PATH);
        }

        private static void ToggleDependencies(bool toLocal)
        {
            try
            {
                if (!File.Exists(MANIFEST_PATH))
                {
                    EditorUtility.DisplayDialog("Error", 
                        $"Could not find manifest.json at {MANIFEST_PATH}", "OK");
                    return;
                }

                string manifestContent = File.ReadAllText(MANIFEST_PATH);
                string originalContent = manifestContent;
                int conversionCount = 0;

                if (toLocal)
                {
                    // Convert from external to local
                    // Pattern: "package-name": "https://github.com/glowdragon/candy-sharp.git?path=/some/path"
                    var pattern = $@"""([^""]+)"":\s*""{Regex.Escape(GITHUB_BASE_URL)}(/[^""]+)""";
                    var matches = Regex.Matches(manifestContent, pattern);
                    
                    foreach (Match match in matches)
                    {
                        string packageName = match.Groups[1].Value;
                        string gitPath = match.Groups[2].Value; // e.g., "/candy-unity" or "/unity-assets/debug-log-extensions"
                        
                        // Remove leading slash from git path
                        string relativePath = gitPath.TrimStart('/');
                        
                        // Special case: candy-unity package points to /candy-unity/unity subdirectory
                        if (relativePath == "candy-unity")
                        {
                            relativePath = "candy-unity/unity";
                        }
                        
                        string localPath = LOCAL_BASE_PATH + relativePath;
                        string oldValue = $@"""{packageName}"": ""{GITHUB_BASE_URL}{gitPath}""";
                        string newValue = $@"""{packageName}"": ""{localPath}""";
                        
                        manifestContent = manifestContent.Replace(oldValue, newValue);
                        conversionCount++;
                        Debug.Log($"Converted {packageName} to local path: {localPath}");
                    }
                }
                else
                {
                    // Convert from local to external
                    // Pattern: "package-name": "file:../../../../candy-sharp/some/path"
                    var pattern = $@"""([^""]+)"":\s*""{Regex.Escape(LOCAL_BASE_PATH)}([^""]+)""";
                    var matches = Regex.Matches(manifestContent, pattern);
                    
                    foreach (Match match in matches)
                    {
                        string packageName = match.Groups[1].Value;
                        string localPath = match.Groups[2].Value; // e.g., "candy-unity/unity" or "unity-assets/debug-log-extensions"
                        
                        // Special case: candy-unity/unity subdirectory should map to /candy-unity in git
                        string gitPath;
                        if (localPath == "candy-unity/unity")
                        {
                            gitPath = "/candy-unity";
                        }
                        else
                        {
                            gitPath = "/" + localPath;
                        }
                        
                        string externalPath = GITHUB_BASE_URL + gitPath;
                        string oldValue = $@"""{packageName}"": ""{LOCAL_BASE_PATH}{localPath}""";
                        string newValue = $@"""{packageName}"": ""{externalPath}""";
                        
                        manifestContent = manifestContent.Replace(oldValue, newValue);
                        conversionCount++;
                        Debug.Log($"Converted {packageName} to external path: {externalPath}");
                    }
                }

                if (conversionCount == 0)
                {
                    EditorUtility.DisplayDialog("No Changes", 
                        $"No candy-sharp packages found to convert to {(toLocal ? "local" : "external")} format.", "OK");
                    return;
                }

                // Write the modified content back
                File.WriteAllText(MANIFEST_PATH, manifestContent);

                // Force Unity's Package Manager to resolve packages
                // var resolveRequest = Client.Resolve();
                
                EditorUtility.DisplayDialog("Success", 
                    $"Converted {conversionCount} package(s) to {(toLocal ? "local" : "external")} format.\n\n" +
                    "Unity is now resolving the packages...", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to toggle dependencies: {ex.Message}", "OK");
                Debug.LogError($"PackageDependencyToggler error: {ex}");
            }
        }
    }
}

