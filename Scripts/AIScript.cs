using UnityEditor;
using UnityEngine;

// for the dictionary
using System.Collections.Generic;
// for ToArray()
using System.Linq;

//TODO: reference to the dll
//https://github.com/Cysharp/UniTask
using Cysharp.Threading.Tasks;

public class AIScript : SingleExtensionApplication
{
    public override string DisplayName => "AI Script";
    private MonoScript inputScript;
    private string inputText = "";
    private Vector2 inputScrollPosition;
    private GameObject csPrefab;

    HelpBox helpBox = HelpBox.GetInstance();

    //TODO: Implement system that updates existing script and asks for confirmation
    //private bool shouldUpdateExistingScript = false;
    private const int standardSpace = 10;

    private Dictionary<int, string> prompts = new Dictionary<int, string>
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

    private Dictionary<EditorPrefKey, string> editorPrefKeys = new Dictionary<EditorPrefKey, string>
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

            LoadEditorPrefs();
            inputScript = (MonoScript)
                EditorGUILayout.ObjectField(
                    inputScript,
                    typeof(MonoScript),
                    true,
                    GUILayout.Width(300)
                );
            GUILayout.Space(standardSpace);

            RenderPopupField();
            GUILayout.Space(standardSpace);

            RenderInputField();
            GUILayout.Space(standardSpace);

            GUILayout.Label(
                "Those are placeholders. Later you can put in files that need to be changed.",
                EditorStyles.boldLabel
            );
            csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);
            GUILayout.Space(standardSpace);
            EditorGUILayout.HelpBox(helpBox.HelpBoxMessage, helpBox.HelpBoxMessageType);
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
            GUIUtility.keyboardControl = 0;
            inputText = "";
        }

        if (GUILayout.Button("Send Prompt", GUILayout.ExpandWidth(true)))
        {
            if (string.IsNullOrEmpty(inputText))
            {
                helpBox.UpdateHelpBoxMessageAndType(
                    "Please enter a prompt in the input field. It will be used to create a new script or to update an existing one.",
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
                    helpBox.UpdateHelpBoxMessageAndType(
                        "An error occurred while processing the input.",
                        MessageType.Error
                    );
                }
            }
        }

        GUILayout.EndHorizontal();
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

    private async void CreateNewScriptBasedOnInput(string inputPrompt)
    {
        inputText = "";
        GUIUtility.keyboardControl = 0;
        string gptScriptResponse = await OpenAiManager.InputToGptCreateScript(inputPrompt);
        // maybe convert this to a readable view Debug.Log(gptScriptResponse);
        //if no response is given, do nothing
        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            return;
        }
        string gptScriptClassName = FileManager<string>.ExtractClassNameFromScript(
            gptScriptResponse
        );
        FileManager<string>.SaveStringToFileInGeneratedPath(
            gptScriptResponse,
            gptScriptClassName + ".cs"
        );

        // UpdateListToGui(input, gptResponse);
    }

    private async void CreateNewScriptVersion(string inputPrompt)
    {
        inputText = "";
        GUIUtility.keyboardControl = 0;
        if (inputScript == null)
        {
            //TODO: Remove null check and insted check if valid script --> you need to write this method anyway in the FileManager
            helpBox.UpdateHelpBoxMessageAndType("Please select a valid script.", MessageType.Error);
            return;
        }
        // Read the content of the MonoScript asset
        string scriptContent = inputScript.ToString();
        helpBox.UpdateHelpBoxMessageAndType(
            inputScript.name + " got read and sent to GPT.",
            MessageType.Info
        );
        string gptScriptResponse = await OpenAiManager.InputScriptToGptCreateScript(
            inputPrompt,
            scriptContent
        );

        // maybe convert this to a readable view  Debug.Log(gptScriptResponse);
        //if no response is given, do nothing
        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            return;
        }
        string gptScriptClassName = FileManager<string>.ExtractClassNameFromScript(
            gptScriptResponse
        );
        FileManager<string>.SaveStringToFileInGeneratedPath(
            gptScriptResponse,
            gptScriptClassName + ".cs"
        );
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
