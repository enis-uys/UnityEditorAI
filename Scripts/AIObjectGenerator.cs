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

    private Dictionary<EditorPrefKey, string> editorPrefKeys = new Dictionary<EditorPrefKey, string>
    {
        { EditorPrefKey.InputText, "InputText" }
    };

    public override void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginVertical("Box");
            RenderInputField();
            GUILayout.Space(defaultSpace);
            csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);
            GUILayout.Space(defaultSpace);
            EditorGUILayout.HelpBox(helpBox.HBMessage, helpBox.HBMessageType);
            helpBox.RenderProgressBar();
            GUILayout.Space(defaultSpace);
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
            GUIUtility.keyboardControl = 0;
            inputText = "";
        }
        if (GUILayout.Button("Send Text"))
        {
            if (string.IsNullOrEmpty(inputText))
            {
                helpBox.UpdateMessageAndType(
                    "Please enter a prompt in the input field.",
                    MessageType.Error
                );
            }
            else
            {
                try
                {
                    ProcessInputPrompt(inputText);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("An error occurred: " + ex.Message);
                    helpBox.UpdateMessageAndType(
                        "An error occurred while processing the input.",
                        MessageType.Error
                    );
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
        helpBox.UpdateMessageAndType("Processing...", MessageType.Info);
        string gptScriptResponse = await OpenAiManager.InputToGptCreateScript(inputPrompt);
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
