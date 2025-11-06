using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Candy.Unity.Editor
{
    /// <summary>
    /// Validates GameObject naming conventions according to project standards.
    /// Enforces the use of spaces between words for better readability.
    /// </summary>
    [InitializeOnLoad]
    public class GameObjectNamingValidator
    {
        private static readonly Regex SnakeCasePattern = new(@"^[a-z]+(_[a-z]+)+$");
        private static readonly Regex AllUppercasePattern = new(@"^[A-Z_\-]+$");
        private const string AutoValidateKey = "GameObjectNamingValidator.AutoValidate";

        static GameObjectNamingValidator()
        {
            if (GetAutoValidateEnabled())
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
            }
        }

        public static void UpdateAutoValidation(bool enabled)
        {
            if (enabled)
            {
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
            }
            else
            {
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            }
        }

        private static void OnHierarchyChanged()
        {
            // Only validate in edit mode to avoid performance issues during play mode
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
                return;

            // Validate only selected GameObjects for performance
            if (Selection.gameObjects.Length > 0)
            {
                foreach (var go in Selection.gameObjects)
                {
                    var violation = ValidateGameObjectName(go);
                    if (violation != null)
                    {
                        Debug.LogWarning(
                            $"[Naming Convention] {violation.Message}\n" +
                            $"Suggestion: Rename to '{violation.SuggestedName}'",
                            go
                        );
                    }
                }
            }
        }

        private static NamingViolation ValidateGameObjectName(GameObject go)
        {
            string name = go.name;

            // Skip children of imported 3D models
            if (IsPartOfImportedModel(go))
                return null;

            // Check for PascalCase concatenation (multiple words without spaces)
            if (Regex.IsMatch(name, @"^[A-Z][a-z]+[A-Z]") && !name.Contains(" "))
            {
                string withSpaces = Regex.Replace(name, @"([a-z])([A-Z])", "$1 $2");
                return new NamingViolation
                {
                    GameObject = go,
                    Message = $"GameObject '{name}' uses PascalCase (should use spaces)",
                    SuggestedName = withSpaces,
                    Severity = ViolationSeverity.Warning
                };
            }

            // Check for snake_case (anti-pattern)
            if (SnakeCasePattern.IsMatch(name))
            {
                string fixedName = ConvertSnakeCaseToPascalWithSpaces(name);
                return new NamingViolation
                {
                    GameObject = go,
                    Message = $"GameObject '{name}' uses snake_case (should use spaces)",
                    SuggestedName = fixedName,
                    Severity = ViolationSeverity.Error
                };
            }

            // Check for ALL_UPPERCASE (anti-pattern)
            if (AllUppercasePattern.IsMatch(name) && name.Length > 3)
            {
                return new NamingViolation
                {
                    GameObject = go,
                    Message = $"GameObject '{name}' uses ALL_UPPERCASE (should use PascalCase with spaces)",
                    SuggestedName = ConvertToTitleCase(name),
                    Severity = ViolationSeverity.Error
                };
            }

            return null;
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

        private static bool GetAutoValidateEnabled()
        {
            return EditorPrefs.GetBool(AutoValidateKey, true);
        }

        private class NamingViolation
        {
            public GameObject GameObject;
            public string Message;
            public string SuggestedName;
            public ViolationSeverity Severity;
        }

        private enum ViolationSeverity
        {
            Warning,
            Error
        }
    }
}

