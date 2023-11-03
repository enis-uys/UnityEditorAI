using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using System.IO;
using System.Linq;

/// <summary>
/// Single application for the AI extension. It is used to generate new GameObjects inside a Unity Scene.
/// </summary>
public class AIObjectGenerator : SingleExtensionApplication
{
    /// <summary>
    /// The display name of the application.
    /// </summary>
    public override string DisplayName => "AI Object Generator";

    /// <summary>
    /// The input text for the prompt.
    /// </summary>
    private string inputText = "";

    /// <summary>
    /// The scroll position of the input text.
    /// </summary>
    private Vector2 inputScrollPosition;

    /// <summary>
    /// The prefab that is used to generate the new GameObject. It is not used yet.
    /// TODO: Implement the usage of the prefab.
    /// </summary>
    private GameObject csPrefab;

    /// <summary>
    /// The name of the temporary script that is used to generate the new GameObject.
    /// </summary>
    private const string DoTaskTemp = "DoTaskTemp";

    /// <summary>
    /// The content of the temporary script that is used to generate the new GameObject.
    /// </summary>
    private string doTaskScriptContent;

    /// <summary>
    /// The path of the temporary script that is used to generate the new GameObject.
    /// </summary>
    private static string generatePath;
    private bool HasInit { get; set; } = false;

    /// <summary>
    /// The list of prompts that are loaded from the JSON file.
    /// </summary>
    private static List<(string Title, string Content)> loadedPromptList = new();

    /// <summary>
    /// The index of the selected prompt.
    /// </summary>
    private int selectedPromptKey = 0;

    /// <summary>
    /// GUI callback for rendering the AI Object Generator extension.
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
            RenderInputField();
            RenderPopupField();
            AddDefaultSpace();
            RenderPrefabField();
            AddDefaultSpace();
            RenderTempDoTaskContent();
            RenderHelpBox();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Renders the prefab field. (not used yet)
    /// </summary>
    private void RenderPrefabField() =>
        csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);

    /// <summary>
    /// Renders the input field.
    /// </summary>
    private void RenderInputField()
    {
        GUILayout.Label(
            new GUIContent(
                "Describe your prompt to create a new Object. Afterwards you have to trigger the new Object Generation Script.",
                "Describe what needs to be changed/added to the Object or explain what the Object should do."
            )
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

    /// <summary>
    /// Renders the prompt popup field.
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
                    try
                    {
                        ProcessInputPromptForGenerate(selectedPromptContent);
                    }
                    catch (System.Exception ex)
                    {
                        helpBoxMessage =
                            "An error occurred while processing the input." + ex.Message;
                        helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                    }
                }
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Renders the temporary script content (if it exists)
    /// </summary>
    private void RenderTempDoTaskContent()
    {
        bool scriptContentEmpty = string.IsNullOrEmpty(doTaskScriptContent);
        if (!scriptContentEmpty)
        {
            AddDefaultSpace();
            EditorGUILayout.BeginVertical();
            try
            {
                // GUIStyle codeStyle = CreateCodeStyle();
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.TextArea(
                        doTaskScriptContent,
                        // codeStyle,
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
                            doTaskScriptContent = "";
                        }
                        if (GUILayout.Button("Copy Content"))
                        {
                            EditorGUIUtility.systemCopyBuffer = doTaskScriptContent;
                            string helpBoxMessage = "Copied script to clipboard.";
                            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                        }
                        if (GUILayout.Button("Trigger Object Generation Script", customButtonStyle))
                        {
                            WriteDoTaskScriptInFile();
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
    /// Processes the input prompt and sends it to the OpenAI API to generate a new GameObject.
    /// </summary>
    /// <param name="inputPrompt"></param>
    private async void ProcessInputPromptForGenerate(string inputPrompt)
    {
        ResetKeyboardControl();
        inputText = "";
        ShowProgressBar(0.1f);
        var messageListBuilder = new MessageListBuilder()
            .AddMessage(OpenAiStandardPrompts.ObjectGenerationPrompt.Content, "system")
            .AddMessage(inputPrompt);
        ShowProgressBar(0.3f);

        string gptScriptResponse = await OpenAiApiManager.RequestToGpt(messageListBuilder);
        ShowProgressBar(0.8f);

        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            string helpBoxMessage = "No response from OpenAI API.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
            return;
        }
        else
        {
            string helpBoxMessage = "Successfully received response from OpenAI API.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            // Saves the response from the OpenAI API into doTaskScriptContent
            string cleanedDoTaskScriptContent = ScriptUtil.CleanScript(gptScriptResponse);
            doTaskScriptContent = cleanedDoTaskScriptContent;
            FinishProgressBarWithDelay();
            Repaint();
        }
    }

    //This part is adapted from Kenjiro AICommand (AICommandWindow.cs)
    // <Availability> https://github.com/keijiro/AICommand/ </Availability>
    ///View LICENSE.md to see the license and information.
    /// <summary>
    /// Creates a script with reflection inside a temporary file and refreshes the asset database.
    /// </summary>
    private void WriteDoTaskScriptInFile()
    {
        generatePath = AISettings.GetGenerateFilesFolderPathFromEditorPrefs() + DoTaskTemp + ".cs";
        FileManager<string>.CreateScriptAssetWithReflection(generatePath, doTaskScriptContent);
        doTaskScriptContent = "";
        AssetDatabase.Refresh();
    }

    // End adapted part from Kenjiro AICommand


    /// <summary>
    ///   Executes the temporary script and deletes it afterwards. It is called after the asset database is refreshed.
    /// </summary>
    [InitializeOnLoadMethod]
    private static void ExecuteAndDeleteAfterReload()
    {
        generatePath = EditorPrefs.GetString(editorPrefKeys[EditorPrefKey.GeneratePath]);
        bool doTaskExists = File.Exists(generatePath) && !string.IsNullOrEmpty(generatePath);
        if (!doTaskExists)
        {
            return;
        }
        string scriptString = File.ReadAllText(generatePath);
        string className = ScriptUtil.ExtractNameAfterKeyWordFromScript(scriptString, "class");
        string methodName = ScriptUtil.ExtractNameAfterKeyWordFromScript(
            scriptString,
            "static void"
        );
        if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName))
        {
            AssetDatabase.DeleteAsset(generatePath);
            Debug.Log("Class or method name is null or empty.");
            return;
        }
        FileManager<string>.InvokeFunction(className, methodName);
        AssetDatabase.DeleteAsset(generatePath);
    }

    /// <summary>
    /// Reloads the prompt list from the JSON file.
    /// </summary>
    public static void ReloadPromptList()
    {
        loadedPromptList = PromptManager.LoadPromptListFromJson();
    }

    public enum EditorPrefKey
    {
        InputObjectText,
        DoTaskScriptContent,
        GeneratePath,
        SelectedPrompt
    }

    /// <summary>
    /// The dictionary that contains the keys for the editor prefs.
    /// </summary>
    private static readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputObjectText, "InputObjectTextKey" },
            { EditorPrefKey.DoTaskScriptContent, "DoTaskScriptContentKey" },
            { EditorPrefKey.GeneratePath, "GeneratePathKey" },
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
                    case EditorPrefKey.InputObjectText:
                        inputText = EditorPrefs.GetString(kvp.Value);
                        break;
                    case EditorPrefKey.DoTaskScriptContent:
                        doTaskScriptContent = EditorPrefs.GetString(kvp.Value);
                        break;
                    case EditorPrefKey.GeneratePath:
                        generatePath = EditorPrefs.GetString(kvp.Value);
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
                case EditorPrefKey.InputObjectText:
                    EditorPrefs.SetString(kvp.Value, inputText);
                    break;
                case EditorPrefKey.DoTaskScriptContent:
                    EditorPrefs.SetString(kvp.Value, doTaskScriptContent);
                    break;
                case EditorPrefKey.GeneratePath:
                    EditorPrefs.SetString(kvp.Value, generatePath);
                    break;
                case EditorPrefKey.SelectedPrompt:
                    EditorPrefs.SetInt(kvp.Value, selectedPromptKey);
                    break;
            }
        }
    }
}
