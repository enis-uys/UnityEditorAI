using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

using System.Linq;

public class ExtensionTabs : EditorWindow
{
    private static SingleExtensionApplication[] applications;
    private static string[] displayNames => applications.Select(a => a.DisplayName).ToArray();
    private int currentApplication;
    private bool hasInit = false;

    [MenuItem("Window/AI Tabs")]
    public static void ShowWindow()
    {
        GetWindow<ExtensionTabs>("AI Tabs");
    }

    private void OnEnable()
    {
        if (!hasInit)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (applications != null)
            return;

        applications = new SingleExtensionApplication[]
        {
            ScriptableObject.CreateInstance<AIChat>(),
            ScriptableObject.CreateInstance<AIScript>(),
            ScriptableObject.CreateInstance<AIObjectGenerator>(),
            ScriptableObject.CreateInstance<AISettings>()
        };
        foreach (var app in applications)
        {
            app.Initialize(this);
        }
    }

    private void OnGUI()
    {
        Initialize();
        EditorGUILayout.LabelField("Choose a tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        var prevApplication = currentApplication;

        currentApplication = GUILayout.Toolbar(currentApplication, displayNames);
        if (currentApplication != prevApplication)
        {
            GUIUtility.keyboardControl = 0;
            applications[currentApplication].Reload();
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            applications[currentApplication].DisplayName,
            EditorStyles.boldLabel
        );
        applications[currentApplication].OnGUI();
    }
}
