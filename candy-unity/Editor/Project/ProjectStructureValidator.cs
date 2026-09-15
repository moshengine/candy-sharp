using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace MoshEngine.Candy.Unity.Editor
{
    public class ProjectStructureValidator : EditorWindow
    {
        private List<string> referenceFolders;
        private List<string> referenceRootFolders;
        private List<string> referenceFiles;
        private List<string> missingFolders;
        private List<string> missingFiles;
        private List<string> unexpectedRootFolders;
        private List<string> misplacedFiles;
        private Vector2 scrollPosition;
        private bool validationPerformed = false;
        private bool structureValid = false;
        private Dictionary<string, List<string>> extensionFolders;
        private List<string> allowAllExtensionsFolders;
        private TextAsset projectStructureAsset;

        [MenuItem("Tools/Candy/Project Structure Validator")]
        public static void ShowWindow()
        {
            GetWindow<ProjectStructureValidator>("Project Structure Validator");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            projectStructureAsset =
                EditorGUILayout.ObjectField("Config File", projectStructureAsset, typeof(TextAsset), false)
                as TextAsset;
            if (projectStructureAsset != null && GUILayout.Button("Open", GUILayout.Width(60)))
            {
                AssetDatabase.OpenAsset(projectStructureAsset);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Validate Project Structure"))
            {
                ValidateProjectStructure();
            }

            if (validationPerformed)
            {
                if (structureValid)
                {
                    EditorGUILayout.HelpBox("All folders and files seem to be in place.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Issues found in project structure. See details below.",
                        MessageType.Warning
                    );
                }
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (missingFolders != null && missingFolders.Count > 0)
            {
                EditorGUILayout.LabelField("Missing Folders:", EditorStyles.boldLabel);
                foreach (string folder in missingFolders)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(folder);
                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                    {
                        CreateFolder(folder);
                        ValidateProjectStructure();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (missingFiles != null && missingFiles.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Missing Files:", EditorStyles.boldLabel);
                foreach (string file in missingFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(file);
                    string folderPath = "Assets/" + Path.GetDirectoryName(file);
                    if (Directory.Exists(folderPath) && AssetDatabase.IsValidFolder(folderPath))
                    {
                        if (GUILayout.Button("Show Folder", GUILayout.Width(80)))
                        {
                            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(folderPath));
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(folderPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (unexpectedRootFolders != null && unexpectedRootFolders.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unexpected Root Folders:", EditorStyles.boldLabel);
                foreach (string folder in unexpectedRootFolders)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(folder);
                    if (GUILayout.Button("Restructure", GUILayout.Width(80)))
                    {
                        RestructureRootFolder(folder);
                        ValidateProjectStructure();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (misplacedFiles != null && misplacedFiles.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Misplaced Files:", EditorStyles.boldLabel);
                foreach (string file in misplacedFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(file);
                    if (GUILayout.Button("Show", GUILayout.Width(60)))
                    {
                        string assetPath = "Assets/" + file;
                        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                        if (asset != null)
                        {
                            EditorGUIUtility.PingObject(asset);
                            Selection.activeObject = asset;
                        }
                        else
                        {
                            Debug.LogWarning($"Asset not found: {assetPath}");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RestructureRootFolder(string rootFolder)
        {
            string rootFolderFullPath = Path.Combine("Assets", rootFolder);
            if (!Directory.Exists(rootFolderFullPath))
            {
                Debug.LogError($"Folder not found: {rootFolderFullPath}");
                return;
            }

            foreach (string subFolder in Directory.GetDirectories(rootFolderFullPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = subFolder.Replace("Assets/", "");
                string folderName = Path.GetFileName(subFolder);

                if (referenceRootFolders.Contains(folderName))
                {
                    string parentFolderName = Path.GetFileName(Path.GetDirectoryName(subFolder));
                    string newPath = Path.Combine("Assets", folderName, parentFolderName);

                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }

                    foreach (string file in Directory.GetFiles(subFolder))
                    {
                        string fileName = Path.GetFileName(file);
                        string newFilePath = Path.Combine(newPath, fileName);
                        File.Move(file, newFilePath);
                    }

                    if (Directory.GetFiles(subFolder).Length == 0 && Directory.GetDirectories(subFolder).Length == 0)
                    {
                        Directory.Delete(subFolder, false);
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private void ValidateProjectStructure()
        {
            // Load the reference structure
            LoadReferenceStructure();

            // Get current folders and files in Assets
            string[] currentFolders = Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories);
            string[] currentFiles = Directory.GetFiles("Assets", "*", SearchOption.AllDirectories);

            // Get files outside Assets folder
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            string[] externalFiles = referenceFiles
                .Where(f => f.StartsWith("../"))
                .Select(f => Path.Combine(projectPath, f.Substring(3)))
                .ToArray();

            // Normalize paths
            for (int i = 0; i < currentFolders.Length; i++)
            {
                currentFolders[i] = currentFolders[i].Replace("\\", "/").Replace("Assets/", "");
            }
            for (int i = 0; i < currentFiles.Length; i++)
            {
                currentFiles[i] = currentFiles[i].Replace("\\", "/").Replace("Assets/", "");
            }

            // Check for missing folders
            missingFolders = new List<string>();
            foreach (string folder in referenceFolders)
            {
                string adjustedFolder = AdjustFolderName(folder);
                if (!System.Array.Exists(currentFolders, element => element.Equals(adjustedFolder)))
                {
                    missingFolders.Add(adjustedFolder);
                }
            }

            // Check for missing files
            missingFiles = new List<string>();
            foreach (string file in referenceFiles)
            {
                string adjustedFile = AdjustFileName(file);
                if (adjustedFile.StartsWith("../"))
                {
                    if (!File.Exists(Path.Combine(projectPath, adjustedFile.Substring(3))))
                    {
                        missingFiles.Add(adjustedFile);
                    }
                }
                else if (!System.Array.Exists(currentFiles, element => element.Equals(adjustedFile)))
                {
                    missingFiles.Add(adjustedFile);
                }
            }

            // Check for unexpected root folders
            unexpectedRootFolders = new List<string>();
            string[] currentRootFolders = Directory.GetDirectories("Assets", "*", SearchOption.TopDirectoryOnly);
            foreach (string folder in currentRootFolders)
            {
                string folderName = Path.GetFileName(folder);
                if (!referenceRootFolders.Contains(folderName) && folderName != "_App")
                {
                    unexpectedRootFolders.Add(folderName);
                }
            }

            // Check for misplaced files
            misplacedFiles = new List<string>();
            foreach (string file in currentFiles)
            {
                string extension = Path.GetExtension(file);
                string folder = Path.GetDirectoryName(file).Replace('\\', '/');
                string rootFolder = file.Split('/')[0];

                // Check if the file is in a folder that allows all extensions
                if (allowAllExtensionsFolders.Any(f => file.StartsWith(AdjustFolderName(f))))
                {
                    continue;
                }

                if (extensionFolders.TryGetValue(extension, out List<string> allowedFolders))
                {
                    bool isAllowed = false;
                    foreach (string allowedFolder in allowedFolders)
                    {
                        string adjustedAllowedFolder = AdjustFolderName(allowedFolder);
                        if (file.StartsWith(adjustedAllowedFolder))
                        {
                            isAllowed = true;
                            break;
                        }
                    }
                    if (!isAllowed)
                    {
                        misplacedFiles.Add(file);
                    }
                }
                else if (extension != ".meta")
                {
                    // If the extension is not specified in the rules and it's not a .meta file, consider it misplaced
                    misplacedFiles.Add(file);
                }
            }

            // Set validation flags
            validationPerformed = true;
            structureValid =
                missingFolders.Count == 0
                && missingFiles.Count == 0
                && unexpectedRootFolders.Count == 0
                && misplacedFiles.Count == 0;

            // Repaint the window to show the results
            Repaint();

            // Output results to console
            if (!structureValid)
            {
                Debug.LogWarning(
                    "Issues found in project structure. Check the Project Structure Validator window for details."
                );
            }
        }

        private void LoadReferenceStructure()
        {
            string json;
            if (projectStructureAsset != null)
            {
                json = projectStructureAsset.text;
            }
            else
            {
                string[] guids = AssetDatabase.FindAssets("ProjectStructure t:TextAsset");
                if (guids.Length == 0)
                {
                    throw new FileNotFoundException(
                        "Project structure reference file 'ProjectStructure.json' not found in the package."
                    );
                }

                string jsonPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (string.IsNullOrEmpty(jsonPath))
                {
                    throw new FileNotFoundException("Unable to locate the path for 'ProjectStructure.json'.");
                }

                json = File.ReadAllText(jsonPath);
                projectStructureAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
            }

            var data = JsonConvert.DeserializeObject<ReferenceData>(json);
            referenceFolders = data.Folders;
            referenceRootFolders = data.Root;
            referenceFiles = data.Files;
            extensionFolders = new Dictionary<string, List<string>>();
            foreach (var extension in data.Extensions)
            {
                foreach (var kvp in extension)
                {
                    extensionFolders[kvp.Key] = kvp.Value;
                }
            }
            allowAllExtensionsFolders = data.AllowAllExtensions;
        }

        private void CreateFolder(string folder)
        {
            string path = Path.Combine("Assets", folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        private string AdjustFolderName(string folder)
        {
            return folder.Replace("_Game", Directory.Exists("Assets/_App") ? "_App" : "_Game");
        }

        private string AdjustFileName(string file)
        {
            return file.Replace("_Game", Directory.Exists("Assets/_App") ? "_App" : "_Game");
        }

        private class ReferenceData
        {
            public List<string> Root { get; set; }
            public List<string> Folders { get; set; }
            public List<string> Files { get; set; }
            public List<Dictionary<string, List<string>>> Extensions { get; set; }
            public List<string> AllowAllExtensions { get; set; }
        }
    }
}
