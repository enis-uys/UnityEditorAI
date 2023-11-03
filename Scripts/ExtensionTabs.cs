using UnityEditor;
using UnityEngine;

using System.Linq;

public class ExtensionTabs : EditorWindow
{
    private static SingleExtensionApplication[] applications;
    private static string[] DisplayNames => applications.Select(a => a.DisplayName).ToArray();
    private int currentApplication;
    private bool HasInit { get; set; } = false;
    private Vector2 scrollPosition;

    [MenuItem("Window/AI Tabs")]
    public static void ShowWindow()
    {
        GetWindow<ExtensionTabs>("AI Tabs");
    }

    private void OnEnable()
    {
        if (!HasInit)
        {
            Initialize();
            HasInit = true;
        }
    }

    private void Initialize()
    {
        if (applications != null)
            return;

        applications = new SingleExtensionApplication[]
        {
            CreateInstance<AiAssistant>(),
            CreateInstance<AIScript>(),
            CreateInstance<AIObjectGenerator>(),
            CreateInstance<PromptManager>(),
            CreateInstance<ColorExtruder>(),
            CreateInstance<AISettings>()
        };
        foreach (var app in applications)
        {
            app.Initialize(this);
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        try
        {
            Initialize();
            EditorGUILayout.LabelField("Choose a tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            var prevApplication = currentApplication;
            currentApplication = GUILayout.Toolbar(currentApplication, DisplayNames);
            if (currentApplication != prevApplication)
            {
                GUIUtility.keyboardControl = 0;
                applications[currentApplication].OnEnable();
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                applications[currentApplication].DisplayName,
                EditorStyles.boldLabel
            );
            applications[currentApplication].OnGUI();
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }
}
