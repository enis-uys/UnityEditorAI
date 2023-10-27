using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using System.IO;
using System.Linq;

public class AIObjectGenerator : SingleExtensionApplication
{
    public override string DisplayName => "AI Object Generator";

    private string inputText = "";
    private Vector2 inputScrollPosition;
    private GameObject csPrefab;
    private const string DoTaskTemp = "DoTaskTemp";
    private string doTaskScriptContent;
    private static string generatePath;
    private bool HasInit { get; set; } = false;
    private static List<(string Title, string Content)> loadedPromptList = new();
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

    private void RenderPrefabField() =>
        csPrefab = (GameObject)EditorGUILayout.ObjectField(csPrefab, typeof(GameObject), true);

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
            FinishProgressBarWithDelay();
            // Saves the response from the OpenAI API into doTaskScriptContent
            doTaskScriptContent = gptScriptResponse;
            Repaint();
        }
    }

    //This part is adapted from Kenjiro AICommand (AICommandWindow.cs)
    // <Availability> https://github.com/keijiro/AICommand/ </Availability>
    // View LICENSE.md to see the license and information.
    private void WriteDoTaskScriptInFile()
    {
        generatePath = FileManager<string>.settingsFM.GeneratedFilesFolderPath + DoTaskTemp + ".cs";
        string cleanedDoTaskScriptContent = ScriptUtil.CleanScript(doTaskScriptContent);
        FileManager<string>.CreateScriptAssetWithReflection(
            generatePath,
            cleanedDoTaskScriptContent
        );
        doTaskScriptContent = "";
        AssetDatabase.Refresh();
    }

    // End adapted part from Kenjiro AICommand

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

    public static void ReloadPromptList()
    {
        loadedPromptList = PromptManager.LoadPromptListFromJson();
    }

    public enum EditorPrefKey
    {
        InputText,
        DoTaskScriptContent,
        GeneratePath,
        SelectedPrompt
    }

    private static readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputText, "InputTextKey" },
            { EditorPrefKey.DoTaskScriptContent, "DoTaskScriptContentKey" },
            { EditorPrefKey.GeneratePath, "GeneratePathKey" },
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
                    case EditorPrefKey.InputText:
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

    private void SetEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.InputText:
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
