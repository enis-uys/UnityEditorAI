using UnityEditor;
using UnityEngine;

using System.Linq;

/// <summary> The main window of the AI extension </summary>
public class ExtensionTabs : EditorWindow
{
    /// <summary> The list of applications to display in the toolbar. </summary>
    private static SingleExtensionApplication[] applications;

    /// <summary> The list of display names for the applications. </summary>
    private static string[] DisplayNames => applications.Select(a => a.DisplayName).ToArray();

    /// <summary> The index of the current application. </summary>
    private int currentApplication;

    /// <summary> Whether the window has been initialized. </summary>
    private bool HasInit { get; set; } = false;

    /// <summary> The scroll position of the whole extension window. </summary>
    private Vector2 scrollPosition;

    /// <summary> Opens the window. </summary>
    [MenuItem("Window/AI Tabs")]
    public static void ShowWindow()
    {
        GetWindow<ExtensionTabs>("AI Tabs");
    }

    /// <summary> Initializes the window. </summary>
    private void OnEnable()
    {
        if (!HasInit)
        {
            Initialize();
            HasInit = true;
        }
    }

    /// <summary>
    /// Initializes the applications.
    /// </summary>
    private void Initialize()
    {
        if (applications != null)
            return;

        applications = new SingleExtensionApplication[]
        {
            CreateInstance<AIAssistant>(),
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

    /// <summary>
    /// Draws the window.
    /// </summary>
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        try
        {
            Initialize();
            EditorStyles.textField.wordWrap = true;
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
