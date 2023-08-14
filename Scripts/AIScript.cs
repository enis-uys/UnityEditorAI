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
    private GameObject csPrefab;

    private MonoScript inputScript;
    private string inputField = "";
    private Vector2 inputScrollPosition;
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

    public override void OnGUI()
    {
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
        inputField = EditorGUILayout.TextArea(inputField, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            inputScript = null;
            csPrefab = null;
            GUIUtility.keyboardControl = 0;
            inputField = "";
        }

        if (GUILayout.Button("Send Text", GUILayout.ExpandWidth(true)))
        {
            if (string.IsNullOrEmpty(inputField))
            {
                helpBox.UpdateHelpBoxMessageAndType(
                    "Please enter a prompt in the input field.",
                    MessageType.Error
                );
            }
            else
            {
                try
                {
                    ProcessInputPrompt(inputField);
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
            CreateNewScriptVersion(inputField);
        }
        else
        {
            CreateNewScriptBasedOnInput(inputField);
        }
    }

    private async void CreateNewScriptBasedOnInput(string inputPrompt)
    {
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
        //clears the input field --> not needed now
        // inputField = "";
        // GUIUtility.keyboardControl = 0;
    }

    private async void CreateNewScriptVersion(string inputPrompt)
    {
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

    public override void OnEnable()
    {
        // TODO: Initialize or set API Key from file or other sources here
    }
}
