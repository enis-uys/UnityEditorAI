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
    private Vector2 inputScrollPosition;
    private GameObject csPrefab;
    public override bool ShouldLoadEditorPrefs { get; set; } = true;

    //TODO: Implement system that updates existing script and asks for confirmation
    //private bool shouldUpdateExistingScript = false;

    private readonly Dictionary<int, string> prompts =
        new()
        {
            { 0, "Improve script" },
            { 1, "Write Comments" },
            { 2, "Remove unused variables" },
            { 3, "Remove Debug Logs" },
            { 4, "Auto-Generate Serialization" },
            { 5, "Category/Option A" },
            { 6, "Category/Option B" }
        };
    private int selectedPromptKey = 0;

    public enum EditorPrefKey
    {
        InputScript,
        InputText,
        SelectedPrompt
    }

    private Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputScript, "InputScriptKey" },
            { EditorPrefKey.InputText, "InputText" },
            { EditorPrefKey.SelectedPrompt, "SelectedPromptKey" }
        };

    public override void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginVertical("Box");
            if (ShouldLoadEditorPrefs)
            {
                LoadEditorPrefs();
                ShouldLoadEditorPrefs = false;
            }
            RenderInputScript();
            AddDefaultSpace();

            RenderPopupField();
            AddDefaultSpace();

            RenderInputField();
            AddDefaultSpace();

            GUILayout.Label(
                "Those are placeholders. Later you can put in files that need to be changed.",
                EditorStyles.boldLabel
            );
            csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);
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

    private void RenderPopupField()
    {
        GUILayout.BeginHorizontal();
        string[] promptOptions = prompts.Values.ToArray();
        selectedPromptKey = EditorGUILayout.Popup(
            "Example Prompts:",
            selectedPromptKey,
            promptOptions,
            GUILayout.Width(400)
        );

        if (GUILayout.Button("Execute Selected Prompt"))
        {
            string prompt = prompts[selectedPromptKey];
            ProcessInputPrompt(prompt);
        }

        GUILayout.EndHorizontal();
    }

    private void ProcessInputPrompt(string prompt)
    {
        if (IsInputScriptSelected())
        {
            CreateNewScriptVersion(inputText);
        }
        else
        {
            CreateNewScriptBasedOnInput(inputText);
        }
    }

    private void ClearInputAndResetKeyboardControl()
    {
        inputText = "";
        ResetKeyboardControl();
    }

    private string CleanAndSaveScriptIntoFile(string script)
    {
        string cleanedScriptResponse = ScriptUtil.CleanScript(script);
        string gptScriptClassName = ScriptUtil.ExtractClassNameFromScript(cleanedScriptResponse);
        FileManager<string>.SaveToJsonFileWithPath(
            cleanedScriptResponse,
            FileManager<string>.settingsFM.GeneratedFilesFolderPath + gptScriptClassName + ".cs"
        );

        return cleanedScriptResponse;
    }

    private async void CreateNewScriptBasedOnInput(string inputPrompt)
    {
        ClearInputAndResetKeyboardControl();

        string gptScriptResponse = await OpenAiApiManager.InputToGptCreateScript(inputPrompt);
        //TODO: maybe convert this to a readable view Debug.Log(gptScriptResponse);
        //if no response is given, do nothing
        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            return;
        }
        CleanAndSaveScriptIntoFile(gptScriptResponse);
    }

    private async void CreateNewScriptVersion(string inputPrompt)
    {
        ClearInputAndResetKeyboardControl();

        string helpBoxMessage;
        if (inputScript == null)
        {
            helpBoxMessage = "Please select a valid script.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        // Read the content of the MonoScript asset
        string scriptContent = inputScript.ToString();
        helpBoxMessage = inputScript.name + " got read and sent to GPT.";
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);

        string gptScriptResponse = await OpenAiApiManager.InputScriptToGptCreateScript(
            inputPrompt,
            scriptContent
        );

        // maybe convert this to a readable view  Debug.Log(gptScriptResponse);
        //if no response is given, do nothing

        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            helpBoxMessage = "Generated Script was empty.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        else if (!ScriptUtil.IsValidScript(gptScriptResponse))
        {
            helpBoxMessage = "Generated Script was not valid.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        CleanAndSaveScriptIntoFile(gptScriptResponse);
    }

    private bool IsInputScriptSelected()
    {
        return inputScript != null;
    }

    private void UpdateExistingScript(string inputPrompt)
    {
        // TODO: Implement updating the existing script
    }

    private void LoadEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            if (EditorPrefs.HasKey(kvp.Value))
            {
                switch (kvp.Key)
                {
                    case EditorPrefKey.InputScript:
                        string inputScriptPath = EditorPrefs.GetString(kvp.Value);
                        inputScript = AssetDatabase.LoadAssetAtPath<MonoScript>(inputScriptPath);
                        break;
                    case EditorPrefKey.InputText:
                        inputText = EditorPrefs.GetString(kvp.Value);
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
                case EditorPrefKey.InputScript:
                    if (inputScript != null)
                    {
                        string inputScriptPath = AssetDatabase.GetAssetPath(inputScript);
                        EditorPrefs.SetString(kvp.Value, inputScriptPath);
                    }
                    else
                    {
                        EditorPrefs.SetString(kvp.Value, "");
                    }
                    break;
                case EditorPrefKey.InputText:
                    EditorPrefs.SetString(kvp.Value, inputText);
                    break;
                case EditorPrefKey.SelectedPrompt:
                    EditorPrefs.SetInt(kvp.Value, selectedPromptKey);
                    break;
            }
        }
    }
}
