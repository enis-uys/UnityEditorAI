using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AIObjectGenerator : SingleExtensionApplication
{
    public override string DisplayName => "AI Object Generator";

    private string inputText = "";
    private Vector2 inputScrollPosition;
    private GameObject csPrefab;

    public enum EditorPrefKey
    {
        InputText
    }

    public override bool ShouldLoadEditorPrefs { get; set; } = false;

    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new() { { EditorPrefKey.InputText, "InputText" } };

    public override void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginVertical("Box");
            RenderInputField();
            AddDefaultSpace();
            csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);
            AddDefaultSpace();
            RenderHelpBox();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void RenderInputField()
    {
        GUILayout.Label(
            new GUIContent(
                "Describe your prompt.",
                "Describe what needs to be changed/added to the Object or explain what the Object should do."
            ),
            EditorStyles.boldLabel
        );

        inputScrollPosition = EditorGUILayout.BeginScrollView(
            inputScrollPosition,
            GUILayout.MinHeight(150)
        );
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            csPrefab = null;
            ResetKeyboardControl();
            inputText = "";
        }
        if (GUILayout.Button("Send Text"))
        {
            if (string.IsNullOrEmpty(inputText))
            {
                string helpBoxMessage = "Please enter a prompt in the input field.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            }
            else
            {
                try
                {
                    ProcessInputPrompt(inputText);
                }
                catch (System.Exception ex)
                {
                    string helpBoxMessage =
                        "An error occurred while processing the input." + ex.Message;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                }
            }
        }
        if (GUILayout.Button("Progress Test"))
        {
            ShowProgressBar(0.5f);
        }
        GUILayout.EndHorizontal();
    }

    private async void ProcessInputPrompt(string inputPrompt)
    {
        string helpBoxMessage = "Processing...";
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
        string gptScriptResponse = await OpenAiApiManager.InputToGptCreateScript(inputPrompt);
        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            return;
        }
    }

    private void LoadEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            if (EditorPrefs.HasKey(kvp.Value))
            {
                switch (kvp.Key)
                {
                    case EditorPrefKey.InputText:
                        inputText = EditorPrefs.GetString(kvp.Value);
                        break;
                }
            }
        }
    }

    private void SetEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.InputText:
                    EditorPrefs.SetString(kvp.Value, inputText);
                    break;
            }
        }
    }
}
