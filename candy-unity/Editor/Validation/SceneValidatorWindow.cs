using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MoshEngine.Candy.Unity.Editor
{
    /// <summary>
    /// Editor Window for GameObject naming validation with interactive UI.
    /// </summary>
    public class SceneValidatorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<NamingViolation> violations = new();
        private bool showWarnings = true;
        private bool showErrors = true;
        private bool showFixed = true;
        private bool includeInactive = true;
        private string searchFilter = "";
        private bool autoValidate;

        private static readonly Regex SnakeCasePattern = new(@"^[a-z]+(_[a-z]+)+$");
        private static readonly Regex AllUppercasePattern = new(@"^[A-Z_\-]+$");
        private const string AutoValidateKey = "GameObjectNamingValidator.AutoValidate";

        [MenuItem("Tools/Candy/Scene Validator")]
        public static void ShowWindow()
        {
            if (!GameObjectNamingValidator.IsEnabled)
            {
                EditorUtility.DisplayDialog("Scene Validator Disabled",
                    "Scene Validator is temporarily disabled globally.",
                    "OK");
                return;
            }

            var window = GetWindow<SceneValidatorWindow>("Scene Validator");
            window.minSize = new Vector2(500, 300);
            window.Show();
            window.ValidateAllGameObjects();
        }

        [MenuItem("Tools/Candy/Scene Validator", true)]
        public static bool ValidateShowWindow()
        {
            return GameObjectNamingValidator.IsEnabled;
        }

        private void OnEnable()
        {
            autoValidate = GameObjectNamingValidator.IsEnabled && EditorPrefs.GetBool(AutoValidateKey, false);
        }

        private void OnGUI()
        {
            if (!GameObjectNamingValidator.IsEnabled)
            {
                DrawDisabledState();
                return;
            }

            DrawHeader();
            DrawControls();
            DrawViolationsList();
        }

        private static void DrawDisabledState()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("Scene Validator is temporarily disabled globally.", MessageType.Info);
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Scene Validator", headerStyle);
            EditorGUILayout.Space(5);
            
            var subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray }
            };
            
            EditorGUILayout.LabelField("Enforces naming conventions: Spaces between words", subtitleStyle);
            EditorGUILayout.Space(10);
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginVertical("box");
            
            // Validate Button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Validate All GameObjects", GUILayout.Height(30)))
            {
                ValidateAllGameObjects();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Filters
            EditorGUILayout.BeginHorizontal();
            includeInactive = EditorGUILayout.ToggleLeft("Include Inactive", includeInactive, GUILayout.Width(120));
            showErrors = EditorGUILayout.ToggleLeft("Show Errors", showErrors, GUILayout.Width(100));
            showWarnings = EditorGUILayout.ToggleLeft("Show Warnings", showWarnings, GUILayout.Width(120));
            showFixed = EditorGUILayout.ToggleLeft("Show Fixed", showFixed, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Auto-validate toggle
            EditorGUILayout.BeginHorizontal();
            bool newAutoValidate = EditorGUILayout.ToggleLeft("Auto-Validate on Hierarchy Changes", autoValidate);
            if (newAutoValidate != autoValidate)
            {
                autoValidate = newAutoValidate;
                EditorPrefs.SetBool(AutoValidateKey, autoValidate);
                GameObjectNamingValidator.UpdateAutoValidation(autoValidate);
            }
            EditorGUILayout.EndHorizontal();

            // Search
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                searchFilter = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawViolationsList()
        {
            if (violations.Count == 0)
            {
                EditorGUILayout.Space(20);
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.3f, 0.8f, 0.3f) }
                };
                EditorGUILayout.LabelField("✅ No violations found!", style);
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("All GameObject names follow the naming convention.", MessageType.Info);
                return;
            }

            // Filter violations
            var filteredViolations = violations
                .Where(v => 
                {
                    if (v.IsFixed) return showFixed;
                    return (showErrors && v.Severity == ViolationSeverity.Error) ||
                           (showWarnings && v.Severity == ViolationSeverity.Warning);
                })
                .Where(v => string.IsNullOrEmpty(searchFilter) ||
                           v.GameObject.name.ToLower().Contains(searchFilter.ToLower()) ||
                           v.SuggestedName.ToLower().Contains(searchFilter.ToLower()) ||
                           (v.OriginalName != null && v.OriginalName.ToLower().Contains(searchFilter.ToLower())))
                .ToList();

            // Summary
            int errorCount = violations.Count(v => !v.IsFixed && v.Severity == ViolationSeverity.Error);
            int warningCount = violations.Count(v => !v.IsFixed && v.Severity == ViolationSeverity.Warning);
            int fixedCount = violations.Count(v => v.IsFixed);
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Total: {violations.Count} | ❌ Errors: {errorCount} | ⚠️ Warnings: {warningCount} | ✅ Fixed: {fixedCount}");
            
            if (GUILayout.Button("Fix All", GUILayout.Width(100)))
            {
                int unfixedCount = filteredViolations.Count(v => !v.IsFixed);
                if (unfixedCount > 0 && EditorUtility.DisplayDialog("Fix All Violations",
                    $"This will rename {unfixedCount} GameObjects. Continue?",
                    "Yes", "Cancel"))
                {
                    FixAllViolations(filteredViolations);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // List
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var violation in filteredViolations)
            {
                DrawViolation(violation);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawViolation(NamingViolation violation)
        {
            Color backgroundColor = violation.IsFixed
                ? new Color(0.3f, 1f, 0.3f, 0.2f)
                : violation.Severity == ViolationSeverity.Error
                    ? new Color(1f, 0.3f, 0.3f, 0.2f)
                    : new Color(1f, 0.8f, 0.3f, 0.2f);

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = originalColor;

            // Header
            EditorGUILayout.BeginHorizontal();
            
            string icon = violation.IsFixed ? "✅" : (violation.Severity == ViolationSeverity.Error ? "❌" : "⚠️");
            EditorGUILayout.LabelField(icon, GUILayout.Width(20));
            
            var boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            string displayName = violation.IsFixed ? violation.GameObject.name : violation.GameObject.name;
            EditorGUILayout.LabelField(displayName, boldStyle);
            
            if (violation.IsFixed && violation.OriginalName != null)
            {
                var strikethroughStyle = new GUIStyle(GUI.skin.label) 
                { 
                    normal = { textColor = Color.gray },
                    fontSize = 10
                };
                EditorGUILayout.LabelField($"(was: {violation.OriginalName})", strikethroughStyle);
            }
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = violation.GameObject;
                EditorGUIUtility.PingObject(violation.GameObject);
            }
            
            if (!violation.IsFixed && GUILayout.Button("Rename", GUILayout.Width(60)))
            {
                FixViolation(violation);
            }
            
            EditorGUILayout.EndHorizontal();

            if (!violation.IsFixed)
            {
                // Details
                EditorGUILayout.LabelField($"Issue: {violation.Message}", EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rename to:", GUILayout.Width(80));
                
                // Use unique control name to prevent GUI state confusion
                string controlName = $"TextField_{violation.GameObject.GetInstanceID()}";
                GUI.SetNextControlName(controlName);
                violation.SuggestedName = EditorGUILayout.TextField(violation.SuggestedName);
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                var successStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = new Color(0.2f, 0.7f, 0.2f) },
                    fontStyle = FontStyle.Italic
                };
                EditorGUILayout.LabelField("✓ Successfully renamed", successStyle);
            }

            // Scene path
            string scenePath = GetGameObjectPath(violation.GameObject);
            var pathStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                normal = { textColor = Color.gray }
            };
            EditorGUILayout.LabelField($"Path: {scenePath}", pathStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void ValidateAllGameObjects()
        {
            violations.Clear();
            
            var allGameObjects = includeInactive 
                ? Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                : Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (var go in allGameObjects)
            {
                var violation = ValidateGameObjectName(go);
                if (violation != null)
                {
                    violations.Add(violation);
                }
            }

            // Sort by severity, then by name
            violations = violations
                .OrderBy(v => v.Severity)
                .ThenBy(v => v.GameObject.name)
                .ToList();

            Repaint();
        }

        private NamingViolation ValidateGameObjectName(GameObject go)
        {
            string name = go.name;

            if (IsPartOfImportedModel(go))
                return null;

            if (Regex.IsMatch(name, @"^[A-Z][a-z]+[A-Z]") && !name.Contains(" "))
            {
                string withSpaces = Regex.Replace(name, @"([a-z])([A-Z])", "$1 $2");
                return new NamingViolation
                {
                    GameObject = go,
                    Message = "Uses PascalCase (should use spaces)",
                    SuggestedName = withSpaces,
                    OriginalName = name,
                    Severity = ViolationSeverity.Warning,
                    IsFixed = false
                };
            }

            if (SnakeCasePattern.IsMatch(name))
            {
                string fixedName = ConvertSnakeCaseToPascalWithSpaces(name);
                return new NamingViolation
                {
                    GameObject = go,
                    Message = "Uses snake_case (anti-pattern)",
                    SuggestedName = fixedName,
                    OriginalName = name,
                    Severity = ViolationSeverity.Error,
                    IsFixed = false
                };
            }

            if (AllUppercasePattern.IsMatch(name) && name.Length > 3)
            {
                return new NamingViolation
                {
                    GameObject = go,
                    Message = "Uses ALL_UPPERCASE (anti-pattern)",
                    SuggestedName = ConvertToTitleCase(name),
                    OriginalName = name,
                    Severity = ViolationSeverity.Error,
                    IsFixed = false
                };
            }

            return null;
        }

        private void FixViolation(NamingViolation violation)
        {
            Undo.RecordObject(violation.GameObject, "Fix GameObject Name");
            string oldName = violation.GameObject.name;
            violation.GameObject.name = violation.SuggestedName;
            EditorUtility.SetDirty(violation.GameObject);
            
            violation.IsFixed = true;
            
            // Clear focus to prevent GUI state issues with text fields
            GUI.FocusControl(null);
            Repaint();
            
            Debug.Log($"✅ Renamed '{oldName}' to '{violation.SuggestedName}'");
        }

        private void FixAllViolations(List<NamingViolation> violationsToFix)
        {
            int fixedCount = 0;
            foreach (var violation in violationsToFix.Where(v => !v.IsFixed))
            {
                Undo.RecordObject(violation.GameObject, "Fix GameObject Names");
                violation.GameObject.name = violation.SuggestedName;
                EditorUtility.SetDirty(violation.GameObject);
                violation.IsFixed = true;
                fixedCount++;
            }

            // Clear focus to prevent GUI state issues with text fields
            GUI.FocusControl(null);
            
            Debug.Log($"✅ Fixed {fixedCount} naming violations!");
            Repaint();
        }

        private static bool IsPartOfImportedModel(GameObject go)
        {
            // Check if this GameObject or any parent is from an imported model
            Transform current = go.transform;
            
            while (current != null)
            {
                // Check if it's a prefab instance from a model asset
                var prefabAssetType = PrefabUtility.GetPrefabAssetType(current.gameObject);
                if (prefabAssetType == PrefabAssetType.Model)
                {
                    return true;
                }

                // Check if parent has model-specific components (common in model hierarchies)
                if (current.GetComponent<MeshFilter>() != null || 
                    current.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    // If this has mesh components and is part of a prefab, likely a model
                    if (PrefabUtility.IsPartOfAnyPrefab(current.gameObject))
                    {
                        var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(current.gameObject);
                        if (!string.IsNullOrEmpty(assetPath) && 
                            (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".obj") || 
                             assetPath.EndsWith(".blend") || assetPath.EndsWith(".dae") ||
                             assetPath.EndsWith(".3ds") || assetPath.EndsWith(".dxf")))
                        {
                            return true;
                        }
                    }
                }

                current = current.parent;
            }

            return false;
        }

        private static string ConvertSnakeCaseToPascalWithSpaces(string snakeCase)
        {
            var parts = snakeCase.Split('_');
            var result = new System.Text.StringBuilder();
            
            foreach (var part in parts)
            {
                if (result.Length > 0)
                    result.Append(' ');
                
                result.Append(char.ToUpper(part[0]));
                result.Append(part.Substring(1).ToLower());
            }
            
            return result.ToString();
        }

        private static string ConvertToTitleCase(string upperCase)
        {
            string lower = upperCase.ToLower();
            var parts = lower.Split('_', '-', ' ');
            var result = new System.Text.StringBuilder();
            
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;
                    
                if (result.Length > 0)
                    result.Append(' ');
                
                result.Append(char.ToUpper(part[0]));
                result.Append(part.Substring(1));
            }
            
            return result.ToString();
        }

        private static string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }

        private class NamingViolation
        {
            public GameObject GameObject;
            public string Message;
            public string SuggestedName;
            public string OriginalName;
            public ViolationSeverity Severity;
            public bool IsFixed;
        }

        private enum ViolationSeverity
        {
            Warning,
            Error
        }
    }
}

