using UnityEditor;
using UnityEngine;

// for the dictionary
using System.Collections.Generic;
// for ToArray()
using System.Linq;

public class AIScript : SingleExtensionApplication
{
    public override string DisplayName => "AI Script";
    private GameObject csPrefab;

    private MonoScript inputScript;
    private string inputField = "";
    private Vector2 inputScrollPosition;

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
                Debug.Log("Empty input");
                return;
            }
            else
            {
                ProcessInputPrompt(inputField);
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

    private void CreateNewScriptBasedOnInput(string inputPrompt)
    {
        string gptScriptResponse = OpenAiManager.InputToGptCreateScript(inputPrompt);
        Debug.Log(gptScriptResponse);
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

    private void CreateNewScriptVersion(string inputPrompt)
    {
        // Read the content of the MonoScript asset

        if (inputScript == null)
        {
            //TODO: Check if valid script --> you need to write this method anyway in the FileManager

            Debug.Log("Input script is null");
            return;
        }

        string scriptContent = inputScript.ToString();
        Debug.Log(inputScript.name + " Got read and sent to GPT");
        string gptScriptResponse = OpenAiManager.InputScriptToGptCreateScript(
            inputPrompt,
            scriptContent
        );
        Debug.Log(gptScriptResponse);
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
