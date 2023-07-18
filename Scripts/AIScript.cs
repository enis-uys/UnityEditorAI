using UnityEditor;
using UnityEngine;
using System.IO;

using Unity;

public class AIScript : SingleExtensionApplication
{
    public override string DisplayName => "AI Script";
    private GameObject csPrefab;

    private MonoScript inputScript;
    private string inputField = "";
    private Vector2 inputScrollPosition;
    private bool shouldUpdateExistingScript = false;
    private const int standardSpace = 20;

    public override void OnGUI()
    {
        RenderInputField();
        GUILayout.Space(standardSpace);
        //Delete later
        GUILayout.Label(
            "Those are placeholders. Later you can put in files that need to be changed.",
            EditorStyles.boldLabel
        );
        csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);
        GUILayout.Space(standardSpace);
        inputScript = (MonoScript)
            EditorGUILayout.ObjectField(inputScript, typeof(MonoScript), true);
    }

    private void RenderInputField()
    {
        GUILayout.Label(
            new GUIContent(
                "Describe your prompt.",
                "Describe what need to be changed / added to the script or explain what the new script should do."
            ),
            EditorStyles.boldLabel
        );
        inputScrollPosition = EditorGUILayout.BeginScrollView(
            inputScrollPosition,
            GUILayout.MinHeight(150)
        );
        inputField = EditorGUILayout.TextArea(inputField, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
        // not needed right now. I just will create new script inside the folder for now
        // shouldUpdateExistingScript = GUILayout.Toggle(
        //     shouldUpdateExistingScript,
        //     "Update the existing script instead of creating a new one in the same folder."
        // );
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear"))
        {
            inputScript = null;
            csPrefab = null;
            GUIUtility.keyboardControl = 0;
            inputField = "";
        }

        if (GUILayout.Button("Send", GUILayout.ExpandWidth(true)))
        {
            if (!string.IsNullOrEmpty(inputField))
            {
                try
                {
                    //Will lead to Create New Script Based On Input
                    CreateScriptAndCheckTheConditions(inputField, inputScript);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("An error occurred during AI processing: " + ex.Message);
                }
            }
            else
            {
                //later helpbox
                Debug.Log("Empty input");
            }
        }

        GUILayout.EndHorizontal();
    }

    private void CreateScriptAndCheckTheConditions(string inputPrompt, MonoScript inputScript)
    {
        //need to create if inside because of GUILayout.Button
        if (shouldUpdateExistingScript && inputScript != null)
        {
            // not implemented yet --> need to create a system to preview and apply before updating
            // UpdateExistingScript("Update script");
            Debug.Log("UpdateExistingScript (not implemented yet)");
        }
        // always true so this is the only call for now --> empty string already checked
        else if (true || (!shouldUpdateExistingScript && inputScript == null))
        {
            // no script selected
            Debug.Log("Create a complete new script.");
            CreateNewScriptBasedOnInput(inputPrompt);
        }
        else if (!shouldUpdateExistingScript && inputScript != null)
        {
            //  script selected
            Debug.Log("Create new version of the selected script.");
            CreateNewScriptVersion(inputPrompt, inputScript);
        }
        else
        {
            Debug.Log("Something went wrong.");
        }
    }

    private void CreateNewScriptBasedOnInput(string inputPrompt)
    {
        var gptScriptResponse = OpenAiManager.InputToGptCreateScript(inputPrompt);
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

    private void CreateNewScriptVersion(string inputPrompt, MonoScript inputScript)
    {
        //not implemented yet. Do later --> need a new prompt system for it
        Debug.Log(inputScript.name);
        // Read the content of the MonoScript asset
        string scriptContent = inputScript.ToString();
        //Writes scriptContent to newScriptVersion.json in standard Path
        FileManager<string>.SaveStringToFileInDefaultPath(
            scriptContent,
            inputScript.name + "NewVersion.cs"
        );
    }

    private void UpdateExistingScript(string inputPrompt) { }

    public override void OnEnable()
    {
        //api Key from file save or other first time things
    }
}
