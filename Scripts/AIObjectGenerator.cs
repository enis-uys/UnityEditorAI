using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

//TODO: For Reflection of Flags
using System.Reflection;
using System.IO;

public class AIObjectGenerator : SingleExtensionApplication
{
    public override string DisplayName => "AI Object Generator";

    private string inputText = "";
    private Vector2 inputScrollPosition;
    private GameObject csPrefab;
    private const string DoTaskTemp = "DoTaskTemp";

    public enum EditorPrefKey
    {
        InputText
    }

    public override bool ShouldLoadEditorPrefs { get; set; } = false;

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
                "Describe your prompt to create a new Object. Afterwards you have to trigger the new Object Generation Script.",
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
        if (GUILayout.Button("Progress Test"))
        {
            ShowProgressBar(0.5f);
        }
        string generatePath =
            FileManager<string>.settingsFM.GeneratedFilesFolderPath + DoTaskTemp + ".cs";
        bool TempFileExists = File.Exists(generatePath);

        using (new EditorGUI.DisabledScope(!TempFileExists))
        {
            if (GUILayout.Button("Trigger Object Generation Script"))
            {
                //This part is adapted from Kenjiro AICommand (AICommandWindow.cs)
                // <Availability> https://github.com/keijiro/AICommand/ </Availability>
                // View LICENSE.md to see the license and information.

                if (!TempFileExists)
                {
                    helpBox.UpdateMessage(
                        "DoTaskTemp file does not exist.",
                        MessageType.Error,
                        false,
                        false
                    );
                    return;
                }
                EditorApplication.ExecuteMenuItem("Edit/Do Task");
                AssetDatabase.DeleteAsset(generatePath);
                AssetDatabase.Refresh();
                //End of adapted part from Kenjiro AICommand.
            }
        }
        if (GUILayout.Button("Send Input"))
        {
            if (string.IsNullOrEmpty(inputText))
            {
                string helpBoxMessage = "Please enter a prompt in the input field.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, false);
            }
            else
            {
                try
                {
                    ProcessInputPromptForGenerate(inputText);
                }
                catch (System.Exception ex)
                {
                    string helpBoxMessage =
                        "An error occurred while processing the input." + ex.Message;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    // This part is partly adapted from Kenjiro AICommand (AICommandWindow.cs)
    // <Availability> https://github.com/keijiro/AICommand/ </Availability>
    // View LICENSE.md to see the license and information.

    private async void ProcessInputPromptForGenerate(string inputPrompt)
    {
        var messageListBuilder = new MessageListBuilder()
            .AddMessage(OpenAiStandardPrompts.ObjectGenerationPrompt, "system")
            .AddMessage(inputPrompt);

        string gptScriptResponse = await OpenAiApiManager.RequestToGpt(messageListBuilder);

        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            helpBox.UpdateMessage("No response from OpenAI API.", MessageType.Error, false, true);
            return;
        }
        // Define the path where the generated script will be saved.
        string generatePath =
            FileManager<string>.settingsFM.GeneratedFilesFolderPath + DoTaskTemp + ".cs";
        FileManager<string>.CreateScriptAssetWithReflection(generatePath, gptScriptResponse);
        // Refresh the AssetDatabase to reflect the newly created asset.
        AssetDatabase.Refresh();
    }

    // End of adapted part from Kenjiro AICommand.


    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new() { { EditorPrefKey.InputText, "InputText" } };

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
