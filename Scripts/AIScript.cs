using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary> Single application for the AI extension. It is used to create new scripts or update existing ones </summary>
public class AIScript : SingleExtensionApplication
{
    /// <summary> The display name of the application. </summary>
    public override string DisplayName => "AI Script";

    /// <summary> The input script that is used to update an existing script. </summary>
    private MonoScript inputScript;

    /// <summary> The input text that is used to create a new script. </summary>
    private string inputText = "";

    /// <summary> The content of the new script that is generated. </summary>
    private string newScriptContent;

    /// <summary> The scroll position of the input field. </summary>
    private Vector2 inputScrollPosition;

    /// <summary> Whether the application has been initialized. </summary>
    private bool HasInit { get; set; } = false;

    /// <summary> The list of prompts that are loaded from the JSON file. </summary>
    private static List<(string Title, string Content)> loadedPromptList = new();
    private int selectedPromptKey = 0;

    //TODO: Implement system that updates existing script and asks for confirmation
    // private bool shouldUpdateExistingScript = false;

    /// <summary> GUI callback for rendering the AI Script extension. </summary>
    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            if (!HasInit)
            {
                LoadEditorPrefs();
                ReloadPromptList();
                HasInit = true;
            }
            RenderInputScript();
            AddDefaultSpace();

            RenderPopupField();
            AddDefaultSpace();

            RenderInputField();
            AddDefaultSpace();
            RenderNewScriptContent();

            AddDefaultSpace();
            RenderHelpBox();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary> Renders the input field for the prompt. </summary>
    private void RenderInputField()
    {
        GUILayout.Label(
            new GUIContent(
                "Describe your prompt.",
                "Describe what needs to be changed/added to the script or explain what the new script should do."
            ),
            EditorStyles.boldLabel
        );

        inputScrollPosition = EditorGUILayout.BeginScrollView(
            inputScrollPosition,
            GUILayout.MinHeight(150)
        );
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            inputScript = null;
            ResetKeyboardControl();
            inputText = "";
        }

        if (GUILayout.Button("Send Prompt", GUILayout.ExpandWidth(true)))
        {
            string helpBoxMessage;
            if (string.IsNullOrEmpty(inputText))
            {
                helpBoxMessage =
                    "Please enter a prompt in the input field. It will be used to create a new script or to update an existing one.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning);
            }
            else
            {
                try
                {
                    // ProcessInputPrompt(inputText);
                    ShowProgressBar(0.1f);
                    helpBoxMessage = "Executing Prompt: " + inputText;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                    GenerateScript(inputText, IsInputScriptSelected());
                }
                catch (System.Exception ex)
                {
                    helpBoxMessage = "An error occurred while processing the input." + ex.Message;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary> Renders the input script field. </summary>
    private void RenderInputScript()
    {
        GUILayout.Label(
            new GUIContent(
                "Select a script to update.",
                "Select a script that should be updated with the generated script. If no script is selected, a new script will be created without reference."
            ),
            EditorStyles.boldLabel
        );
        inputScript = (MonoScript)
            EditorGUILayout.ObjectField(
                inputScript,
                typeof(MonoScript),
                true,
                GUILayout.Width(300)
            );
    }

    /// <summary> Renders the new script content (if there is any). </summary>
    private void RenderNewScriptContent()
    {
        bool scriptContentEmpty = string.IsNullOrEmpty(newScriptContent);
        if (!scriptContentEmpty)
        {
            AddDefaultSpace();
            EditorGUILayout.BeginVertical();
            try
            {
                GUIStyle codeStyle = CreateCodeStyle();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextArea(
                        newScriptContent,
                        codeStyle,
                        GUILayout.ExpandHeight(true)
                    );
                }
                using (new EditorGUI.DisabledScope(scriptContentEmpty))
                {
                    GUIStyle customButtonStyle = CreateHighlightButtonStyle();
                    EditorGUILayout.BeginHorizontal();
                    try
                    {
                        if (GUILayout.Button("Delete Content"))
                        {
                            newScriptContent = "";
                        }
                        if (GUILayout.Button("Copy Content"))
                        {
                            EditorGUIUtility.systemCopyBuffer = newScriptContent;
                            string helpBoxMessage = "Copied script to clipboard.";
                            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                        }
                        if (GUILayout.Button("Create Script File", customButtonStyle))
                        {
                            SaveScriptIntoFile();
                        }
                    }
                    finally
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }
    }

    /// <summary> Saves the generated script into a file inside the GenerateFolder </summary>
    private void SaveScriptIntoFile()
    {
        string gptScriptClassName = ScriptUtil.ExtractNameAfterKeyWordFromScript(
            newScriptContent,
            "class"
        );
        if (string.IsNullOrEmpty(gptScriptClassName))
        {
            string helpBoxMessage = "Could not extract class name from script.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        string generatePath =
            AISettings.GetGenerateFilesFolderPathFromEditorPrefs() + gptScriptClassName + ".cs";
        ReflectiveMethods.CreateScriptAssetWithReflection(generatePath, newScriptContent);
        AssetDatabase.Refresh();
        newScriptContent = "";
    }

    /// <summary> Renders the popup field for the prompt list. </summary>
    private void RenderPopupField()
    {
        GUILayout.Label(
            new GUIContent(
                "Select a prompt from PromptManager.",
                "Select a prompt from the list that should be used to create a new script or to update an existing one."
            ),
            EditorStyles.boldLabel
        );
        EditorGUILayout.BeginHorizontal();
        try
        {
            string[] loadedPromptListArray = loadedPromptList
                .Select(tuple => tuple.Title)
                .ToArray();
            selectedPromptKey = EditorGUILayout.Popup(
                selectedPromptKey,
                loadedPromptListArray,
                GUILayout.MaxWidth(300)
            );
            //TODO: Implement checkbox under the input field that allows to select if the prompt should be included or only the popup prompt
            if (GUILayout.Button("Execute Selected Prompt", GUILayout.ExpandWidth(true)))
            {
                if (selectedPromptKey >= 0 && selectedPromptKey < loadedPromptList.Count)
                {
                    string selectedPromptContent = loadedPromptList
                        .ElementAt(selectedPromptKey)
                        .Content;
                    string helpBoxMessage = "Executing Prompt: " + selectedPromptContent;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                    ShowProgressBar(0.1f);
                    GenerateScript(selectedPromptContent, IsInputScriptSelected());
                    ///ProcessInputPrompt(selectedPromptContent);
                }
            }
        }
        finally
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary> Processes the input prompt and creates a new script or updates an existing one./ </summary>
    /// <param name="inputScript"> The input script that is used to generate the Script </param>
    /// <param name="isUpdate"> Whether a script should be used to update an existing script or not. </param>
    private async void GenerateScript(string inputPrompt, bool isUpdate)
    {
        ShowProgressBar(0.2f);
        ClearInputAndResetKeyboardControl();
        string helpBoxMessage;
        var messageListBuilder = new MessageListBuilder();
        if (isUpdate && inputScript == null)
        {
            helpBoxMessage = "Please select a valid script.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        else if (isUpdate && inputScript != null)
        {
            // Read the content of the MonoScript asset
            string scriptContent = inputScript.ToString();
            helpBoxMessage = inputScript.name + " got read and sent to GPT.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            messageListBuilder
                .AddMessage(OpenAiStandardPrompts.UpdateExistingScriptWithPrompt.Content, "system")
                .AddMessage(OpenAiStandardPrompts.ScriptEndNote.Content, "system")
                .AddMessage(inputPrompt)
                .AddMessage(scriptContent);
            ShowProgressBar(0.3f);
        }
        else if (!isUpdate)
        {
            messageListBuilder
                .AddMessage(OpenAiStandardPrompts.CreateNewScriptWithPrompt.Content, "system")
                .AddMessage(OpenAiStandardPrompts.ScriptEndNote.Content, "system")
                .AddMessage(inputPrompt);
            ShowProgressBar(0.3f);
        }
        inputScript = null;
        string gptScriptResponse = await OpenAiApiManager.RequestToGpt(messageListBuilder);
        ShowProgressBar(0.8f);
        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            helpBoxMessage = "Generated Script was empty.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
            return;
        }
        else if (!ScriptUtil.IsValidScript(gptScriptResponse))
        {
            helpBoxMessage = "Generated Script was not valid.\n" + gptScriptResponse;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
            return;
        }
        string cleanedScriptContent = ScriptUtil.CleanScript(gptScriptResponse);
        newScriptContent = cleanedScriptContent;
        FinishProgressBarWithDelay();
        Repaint();
    }

    /// <summary> Clears the input field and resets the keyboard control. </summary>
    private void ClearInputAndResetKeyboardControl()
    {
        inputText = "";
        ResetKeyboardControl();
    }

    /// <summary> Reloads the prompt list from the JSON file. </summary>
    public static void ReloadPromptList()
    {
        loadedPromptList = PromptManager.LoadPromptListFromJson();
    }

    /// <summary> Checks if an input script is selected. </summary>
    /// <returns> Returns true if an input script is selected. </returns>
    private bool IsInputScriptSelected()
    {
        return inputScript != null;
    }

    public enum EditorPrefKey
    {
        InputScriptGUID,
        InputScriptText,
        NewScriptContent,
        SelectedPrompt
    }

    /// <summary> The list of keys for the editor prefs. </summary>
    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputScriptGUID, "InputScriptGUIDKey" },
            { EditorPrefKey.InputScriptText, "InputScriptTextKey" },
            { EditorPrefKey.NewScriptContent, "NewScriptContentKey" },
            { EditorPrefKey.SelectedPrompt, "SelectedPromptKey" }
        };

    /// <summary> Loads the editor prefs. </summary>
    private void LoadEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            if (EditorPrefs.HasKey(kvp.Value))
            {
                switch (kvp.Key)
                {
                    case EditorPrefKey.InputScriptGUID:
                        // Updated to use GUID because it is more reliable than path
                        string inputScriptPath = AssetDatabase.GUIDToAssetPath(
                            EditorPrefs.GetString(kvp.Value)
                        );
                        inputScript = AssetDatabase.LoadAssetAtPath<MonoScript>(inputScriptPath);
                        break;
                    case EditorPrefKey.InputScriptText:
                        inputText = EditorPrefs.GetString(kvp.Value);
                        break;
                    case EditorPrefKey.NewScriptContent:
                        newScriptContent = EditorPrefs.GetString(kvp.Value);
                        break;
                    case EditorPrefKey.SelectedPrompt:
                        selectedPromptKey = EditorPrefs.GetInt(kvp.Value);
                        break;
                }
            }
        }
    }

    /// <summary> Sets the editor prefs. </summary>
    private void SetEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.InputScriptGUID:
                    if (inputScript != null)
                    {
                        // Updated to use GUID because it is more reliable than path
                        string inputScriptGUID = AssetDatabase.AssetPathToGUID(
                            AssetDatabase.GetAssetPath(inputScript)
                        );
                        EditorPrefs.SetString(kvp.Value, inputScriptGUID);
                    }
                    else
                    {
                        EditorPrefs.SetString(kvp.Value, "");
                    }
                    break;
                case EditorPrefKey.InputScriptText:
                    EditorPrefs.SetString(kvp.Value, inputText);
                    break;
                case EditorPrefKey.NewScriptContent:
                    EditorPrefs.SetString(kvp.Value, newScriptContent);
                    break;
                case EditorPrefKey.SelectedPrompt:
                    EditorPrefs.SetInt(kvp.Value, selectedPromptKey);
                    break;
            }
        }
    }
}
