using UnityEngine;
using UnityEditor;
using System.IO;
using static CandyCore.EditorGUICandy;

public class ModelToPrefabConverterWindow : EditorWindow
{
    private GameObject _selectedModel;
    private bool _createScripts = true;
    private TextAsset _configScriptTemplate;
    private TextAsset _controllerScriptTemplate;
    private TextAsset _stateScriptTemplate;
    private TextAsset _viewScriptTemplate;

    [MenuItem("Tools/Candy/Model To Prefab Converter")]
    public static void ShowWindow()
    {
        GetWindow<ModelToPrefabConverterWindow>("Model To Prefab Converter");
    }

    private void OnEnable()
    {
        LoadDefaultTemplates();
    }

    private void LoadDefaultTemplates()
    {
        _configScriptTemplate = LoadTemplateAsset("ConfigScriptTemplate");
        _controllerScriptTemplate = LoadTemplateAsset("ControllerScriptTemplate");
        _stateScriptTemplate = LoadTemplateAsset("StateScriptTemplate");
        _viewScriptTemplate = LoadTemplateAsset("ViewScriptTemplate");
    }

    private TextAsset LoadTemplateAsset(string templateName)
    {
        string[] guids = AssetDatabase.FindAssets($"{templateName} t:TextAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        return null;
    }

    private void OnGUI()
    {
        BeginSection("Configuration");
        _configScriptTemplate = EditorGUILayout.ObjectField("Config Script Template", _configScriptTemplate, typeof(TextAsset), false) as TextAsset;
        _controllerScriptTemplate = EditorGUILayout.ObjectField("Controller Script Template", _controllerScriptTemplate, typeof(TextAsset), false) as TextAsset;
        _stateScriptTemplate = EditorGUILayout.ObjectField("State Script Template", _stateScriptTemplate, typeof(TextAsset), false) as TextAsset;
        _viewScriptTemplate = EditorGUILayout.ObjectField("View Script Template", _viewScriptTemplate, typeof(TextAsset), false) as TextAsset;
        EndSection();

        BeginSection("Current");
        _selectedModel = EditorGUILayout.ObjectField("Select Model", _selectedModel, typeof(GameObject), false) as GameObject;
        _createScripts = EditorGUILayout.Toggle("Create Scripts", _createScripts);
        if (Button("Convert"))
        {
            if (_selectedModel != null)
            {
                ConvertModelToPrefab();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a model before converting.", "OK");
            }
        }
        EndSection();
    }

    private void ConvertModelToPrefab()
    {
        string modelName = _selectedModel.name;
        string modelPath = AssetDatabase.GetAssetPath(_selectedModel);
        string modelDirectory = Path.GetDirectoryName(modelPath);
        string prefabPath = Path.Combine(modelDirectory, $"{modelName}.prefab");

        // Create the prefab structure
        GameObject prefabRoot = new GameObject(modelName);
        GameObject viewObject = new GameObject("View");
        GameObject modelObject = PrefabUtility.InstantiatePrefab(_selectedModel) as GameObject;

        viewObject.transform.SetParent(prefabRoot.transform);
        modelObject.transform.SetParent(viewObject.transform);

        modelObject.name = "Model";

        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

        if (_createScripts)
        {
            CreateScript(modelName, $"{modelName}Config", null, modelDirectory, _configScriptTemplate);
            CreateScript(modelName, $"{modelName}Controller", prefabRoot, modelDirectory, _controllerScriptTemplate);
            CreateScript(modelName, $"{modelName}State", null, modelDirectory, _stateScriptTemplate);
            CreateScript(modelName, $"{modelName}View", viewObject, modelDirectory, _viewScriptTemplate);
        }

        DestroyImmediate(prefabRoot);

        Debug.Log($"Prefab created at {prefabPath}");
    }

    private void CreateScript(string entityName, string scriptName, GameObject targetObject, string directory, TextAsset scriptTemplate)
    {
        string scriptPath = Path.Combine(directory, $"{scriptName}.cs");

        if (!File.Exists(scriptPath))
        {
            if (scriptTemplate != null)
            {
                string scriptContent = scriptTemplate.text;
                scriptContent = scriptContent.Replace("{{ScriptName}}", scriptName);
                scriptContent = scriptContent.Replace("{{EntityName}}", entityName);
                File.WriteAllText(scriptPath, scriptContent);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Script template is missing for {scriptName}. Script creation skipped.");
                return;
            }
        }

        if (targetObject != null)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            targetObject.AddComponent(script.GetClass());
        }
    }
}