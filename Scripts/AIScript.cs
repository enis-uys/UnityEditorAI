using UnityEditor;
using UnityEngine;

// for the dictionary
using System.Collections.Generic;
// for ToArray()
using System.Linq;

public class AIScript : SingleExtensionApplication
{
    public override string DisplayName => "AI Script";
    private MonoScript inputScript;
    private string inputText = "";
    private string newScriptContent;
    private Vector2 inputScrollPosition;
    private GameObject csPrefab;
    private bool HasInit { get; set; } = false;

    private static List<(string Title, string Content)> loadedPromptList = new();

    //TODO: Implement system that updates existing script and asks for confirmation
    //private bool shouldUpdateExistingScript = false;
    private int selectedPromptKey = 0;

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
            // RenderGameObjectPlaceHolder();

            AddDefaultSpace();
            RenderHelpBox();
            SetEditorPrefs();
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
            csPrefab = null;
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

    //TODO: Implement gameObject manipulation / analysis
    private void RenderGameObjectPlaceHolder()
    {
        GUILayout.Label(
            "Those are placeholders. Later you can put in files that need to be changed.",
            EditorStyles.boldLabel
        );
        csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);
    }

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

    private void ClearInputAndResetKeyboardControl()
    {
        inputText = "";
        ResetKeyboardControl();
    }

    public static void ReloadPromptList()
    {
        loadedPromptList = PromptManager.LoadPromptListFromJson();
    }

    private bool IsInputScriptSelected()
    {
        return inputScript != null;
    }

    private void UpdateExistingScript(string inputPrompt)
    {
        // TODO: Implement updating the existing script
    }

    public enum EditorPrefKey
    {
        InputScriptGUID,
        InputScriptText,
        NewScriptContent,
        SelectedPrompt
    }

    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputScriptGUID, "InputScriptGUIDKey" },
            { EditorPrefKey.InputScriptText, "InputScriptTextKey" },
            { EditorPrefKey.NewScriptContent, "NewScriptContentKey" },
            { EditorPrefKey.SelectedPrompt, "SelectedPromptKey" }
        };

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
