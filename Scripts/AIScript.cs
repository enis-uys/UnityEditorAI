using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Single application for the AI extension. It is used to create new scripts or update existing ones.
/// </summary>
public class AIScript : SingleExtensionApplication
{
    /// <summary>
    /// The display name of the application.
    /// </summary>
    public override string DisplayName => "AI Script";

    /// <summary>
    /// The input script that is used to update an existing script.
    /// </summary>
    private MonoScript inputScript;

    /// <summary>
    /// The input text that is used to create a new script.
    /// </summary>
    private string inputText = "";

    /// <summary>
    /// The content of the new script that is generated.
    /// </summary>
    private string newScriptContent;

    /// <summary>
    /// The scroll position of the input field.
    /// </summary>
    private Vector2 inputScrollPosition;

    /// <summary>
    ///
    /// </summary>
    private bool HasInit { get; set; } = false;

    /// <summary>
    /// The list of prompts that are loaded from the JSON file.
    /// </summary>
    private static List<(string Title, string Content)> loadedPromptList = new();
    private int selectedPromptKey = 0;

    //TODO: Implement system that updates existing script and asks for confirmation
    //private bool shouldUpdateExistingScript = false;
    /// <summary>
    /// GUI callback for rendering the AI Script extension.
    /// </summary>
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

    /// <summary>
    /// Renders the input field for the prompt.
    /// </summary>
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
        GUILayout.BeginHorizontal();
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
                    ProcessInputPrompt(inputText);
                }
                catch (System.Exception ex)
                {
                    helpBoxMessage = "An error occurred while processing the input." + ex.Message;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Renders the input script field.
    /// </summary>
    private void RenderInputScript()
    {
        inputScript = (MonoScript)
            EditorGUILayout.ObjectField(
                inputScript,
                typeof(MonoScript),
                true,
                GUILayout.Width(300)
            );
    }

    /// <summary>
    /// Renders the new script content (if there is any).
    /// </summary>
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
                    GUILayout.TextArea(newScriptContent, codeStyle, GUILayout.ExpandHeight(true));
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

    /// <summary>
    /// Saves the generated script into a file inside the GenerateFolder
    /// </summary>
    /// <returns>
    /// Returns the content of the new script.
    /// </returns>
    private string SaveScriptIntoFile()
    {
        string gptScriptClassName = ScriptUtil.ExtractNameAfterKeyWordFromScript(
            newScriptContent,
            "class"
        );
        string generatePath =
            AISettings.GetGenerateFilesFolderPathFromEditorPrefs() + gptScriptClassName + ".cs";
        FileManager<string>.CreateScriptAssetWithReflection(generatePath, newScriptContent);
        AssetDatabase.Refresh();
        newScriptContent = "";
        return newScriptContent;
    }

    /// <summary>
    /// Renders the popup field for the prompt list.
    /// </summary>
    private void RenderPopupField()
    {
        GUILayout.BeginHorizontal();
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
            if (GUILayout.Button("Execute Selected Prompt", GUILayout.ExpandWidth(true)))
            {
                if (selectedPromptKey >= 0 && selectedPromptKey < loadedPromptList.Count)
                {
                    string selectedPromptContent = loadedPromptList
                        .ElementAt(selectedPromptKey)
                        .Content;

                    string helpBoxMessage = "Executing Prompt: " + selectedPromptContent;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                    ProcessInputPrompt(selectedPromptContent);
                }
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    //TODO: Implement checkbox under the input field that allows to select if the prompt should be included

    /// <summary>
    /// Processes the input prompt and creates a new script or updates an existing one.
    /// </summary>
    /// <param name="prompt"></param>
    private void ProcessInputPrompt(string prompt)
    {
        ShowProgressBar(0.1f);
        if (IsInputScriptSelected())
        {
            CreateUpdatedScriptVersion(prompt);
        }
        else
        {
            CreateNewScript(prompt);
        }
    }

    /// <summary>
    /// Sends the input prompt to the AI API and creates a new script.
    /// </summary>
    /// <param name="inputPrompt"></param>
    private async void CreateNewScript(string inputPrompt)
    {
        ShowProgressBar(0.2f);
        ClearInputAndResetKeyboardControl();
        string helpBoxMessage;
        var messageListBuilder = new MessageListBuilder()
            .AddMessage(OpenAiStandardPrompts.CreateNewScriptWithPrompt.Content, "system")
            .AddMessage(OpenAiStandardPrompts.ScriptEndNote.Content, "system")
            .AddMessage(inputPrompt);
        ShowProgressBar(0.3f);

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
            helpBoxMessage = "Generated Script was not valid.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
            return;
        }
        string cleanedScriptContent = ScriptUtil.CleanScript(gptScriptResponse);
        newScriptContent = cleanedScriptContent;
        Repaint();
    }

    /// <summary>
    /// Sends the input prompt to the AI API and updates the selected script.
    /// </summary>
    /// <param name="inputPrompt"></param>
    private async void CreateUpdatedScriptVersion(string inputPrompt)
    {
        ShowProgressBar(0.2f);
        ClearInputAndResetKeyboardControl();
        string helpBoxMessage;
        if (inputScript == null)
        {
            helpBoxMessage = "Please select a valid script.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        // Read the content of the MonoScript asset
        //TODO:
        string scriptContent = inputScript.ToString();
        helpBoxMessage = inputScript.name + " got read and sent to GPT.";
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);

        //TODO: Test if this works
        var messageListBuilder = new MessageListBuilder()
            .AddMessage(OpenAiStandardPrompts.UpdateExistingScriptWithPrompt.Content, "system")
            .AddMessage(OpenAiStandardPrompts.ScriptEndNote.Content, "system")
            .AddMessage(inputPrompt)
            .AddMessage(scriptContent);
        ShowProgressBar(0.3f);

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
            helpBoxMessage = "Generated Script was not valid.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
            return;
        }
        string cleanedScriptContent = ScriptUtil.CleanScript(gptScriptResponse);
        newScriptContent = cleanedScriptContent;
        FinishProgressBarWithDelay();
        Repaint();
    }

    /// <summary>
    /// Clears the input field and resets the keyboard control.
    /// </summary>
    private void ClearInputAndResetKeyboardControl()
    {
        inputText = "";
        ResetKeyboardControl();
    }

    /// <summary>
    /// Reloads the prompt list from the JSON file.
    /// </summary>
    public static void ReloadPromptList()
    {
        loadedPromptList = PromptManager.LoadPromptListFromJson();
    }

    /// <summary>
    /// Checks if an input script is selected.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// The list of keys for the editor prefs.
    /// </summary>
    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputScriptGUID, "InputScriptGUIDKey" },
            { EditorPrefKey.InputScriptText, "InputScriptTextKey" },
            { EditorPrefKey.NewScriptContent, "NewScriptContentKey" },
            { EditorPrefKey.SelectedPrompt, "SelectedPromptKey" }
        };

    /// <summary>
    /// Loads the editor prefs.
    /// </summary>
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

    /// <summary>
    /// Sets the editor prefs.
    /// </summary>
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
