using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary> The prompt manager is used to manage custom prompts and display pre-defined prompts. </summary>
public class PromptManager : SingleExtensionApplication
{
    /// <summary> The display name of the prompt manager. </summary>
    public override string DisplayName => "Prompt Manager";

    /// <summary> The scroll position of the prompt manager. </summary>
    private Vector2 scrollPosition;

    /// <summary> The boolean that indicates if a new prompt should be added. </summary>
    private bool addField = false;

    /// <summary> The index of the selected prompt. </summary>
    private int selectedIndex = -1;

    /// <summary> The content of the new prompt. </summary>
    string newPromptContent,
        /// <summary> The title of the new prompt. </summary>
        newPromptTitle;

    /// <summary> The name of the prompt list file. </summary>
    public static readonly string promptListFileName = "promptList.json";

    /// <summary> The current prompt texts. </summary>
    private List<string> currentPromptTexts = new();

    /// <summary> The GUIStyle for displaying strings as code. </summary>
    GUIStyle codeStyle;

    /// <summary> The boolean that indicates if the prompt manager has been initialized. </summary>
    private bool HasInit { get; set; } = false;

    /// <summary> The list of custom prompts. </summary>

    private static List<(string Title, string Content)> customPromptList = new();

    /// <summary> The getter and setter for the custom prompt list. </summary>
    public static List<(string Title, string Content)> CustomPromptList
    {
        get => customPromptList;
        set => customPromptList = value;
    }

    //These prompt get loaded if no custom prompts are found

    /// <summary>The default custom prompts that get loaded when no prompt is loaded. </summary>
    private static readonly List<(string Title, string Content)> defaultCustomPrompts =
        new()
        {
            OpenAiStandardPrompts.ImproveScriptPrompt,
            OpenAiStandardPrompts.WriteCommentsPrompt,
            OpenAiStandardPrompts.RemoveVariablesPrompt,
            OpenAiStandardPrompts.RemoveDebugLogsPrompt,
            OpenAiStandardPrompts.AutoGenerateSerializationPrompt,
            OpenAiStandardPrompts.GenerateRotationScriptPrompt,
            OpenAiStandardPrompts.GenerateParticleSystemPrompt,
            OpenAiStandardPrompts.GenerateLightsPrompt,
        };

    /// <summary> The default pre-defined prompt list. </summary>
    private readonly List<(string Title, string Content)> defaultPromptList =
        new()
        {
            OpenAiStandardPrompts.ScriptEndNote,
            OpenAiStandardPrompts.CreateNewScriptWithPrompt,
            OpenAiStandardPrompts.ObjectGenerationPrompt,
            OpenAiStandardPrompts.UpdateExistingScriptWithPrompt,
            OpenAiStandardPrompts.ColorImageGenerationPrompt,
        };

    /// <summary> The method that renders the GUI of the prompt manager. </summary>
    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            if (!HasInit)
            {
                CustomPromptList = LoadPromptListFromJson();
                ResetCurrentPromptTexts();
                codeStyle = CreateCodeStyle();
                HasInit = true;
            }
            RenderPromptLists();
            AddDefaultSpace();
            RenderNewPromptField();
            RenderHelpBox();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary> Renders the custom and default prompt lists. </summary>
    private void RenderPromptLists()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        RenderCustomPromptList();
        AddDefaultSpace();
        AddDefaultSpace();
        RenderDefaultPromptList();

        EditorGUILayout.EndScrollView();
    }

    /// <summary> Renders the custom prompt list. </summary>
    private void RenderCustomPromptList()
    {
        GUILayout.Label(
            new GUIContent(
                "Custom Prompts:",
                "Custom Prompts are saved inside the extension's userfiles folder."
            ),
            EditorStyles.boldLabel
        );
        if (CustomPromptList == null || CustomPromptList.Count == 0)
        {
            helpBox.UpdateMessage("No custom prompts found.", MessageType.Warning, false, false);
            GUILayout.Label("NO CUSTOM PROMPTS FOUND.", EditorStyles.boldLabel);
        }
        else
        {
            int itemToRemove = -1; // Track the item to remove from the list
            for (int i = 0; i < CustomPromptList.Count; i++)
            {
                EditorGUILayout.BeginVertical();

                try
                {
                    AddDefaultSpace();
                    GUILayout.Label(CustomPromptList[i].Title, EditorStyles.boldLabel);
                    using (new EditorGUI.DisabledScope(selectedIndex != i))
                    {
                        currentPromptTexts[i] = EditorGUILayout.TextArea(
                            currentPromptTexts[i],
                            codeStyle
                        );
                    }

                    EditorGUILayout.BeginHorizontal();
                    try
                    {
                        using (new EditorGUI.DisabledScope(selectedIndex != i))
                        {
                            if (GUILayout.Button("Delete", GUILayout.ExpandWidth(true)))
                            {
                                itemToRemove = i;
                                selectedIndex = -1;
                                ResetKeyboardControl();
                            }
                            if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
                            {
                                selectedIndex = -1;
                                ResetKeyboardControl();
                                var updatedItem = (
                                    CustomPromptList[i].Title,
                                    currentPromptTexts[i]
                                );
                                CustomPromptList[i] = updatedItem;
                                SavePromptListInJson();
                            }
                        }
                        if (GUILayout.Button("Copy", GUILayout.ExpandWidth(true)))
                        {
                            EditorGUIUtility.systemCopyBuffer = currentPromptTexts[i];
                            string helpBoxMessage = "Copied prompt to clipboard.";
                            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                        }
                        if (
                            selectedIndex != i
                            && GUILayout.Button("Edit", GUILayout.ExpandWidth(true))
                        )
                        {
                            selectedIndex = i;
                        }
                        else if (
                            selectedIndex == i
                            && GUILayout.Button("Cancel", GUILayout.ExpandWidth(true))
                        )
                        {
                            selectedIndex = -1;
                            ResetCurrentPromptTexts();
                            ResetKeyboardControl();
                        }
                    }
                    finally
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }
                finally
                {
                    EditorGUILayout.EndVertical();
                }
            }
            // Remove marked items from the list in reverse order to avoid index issues

            if (itemToRemove >= 0)
            {
                CustomPromptList.RemoveAt(itemToRemove);
                SavePromptListInJson();
            }
        }
    }

    ///  Renders the default  </summary>
    private void RenderDefaultPromptList()
    {
        GUILayout.Label(
            new GUIContent("Default Prompts:", "Default Prompts are not editable."),
            EditorStyles.boldLabel
        );
        for (int i = 0; i < defaultPromptList.Count; i++)
        {
            EditorGUILayout.BeginVertical();
            try
            {
                AddDefaultSpace();
                GUILayout.Label(defaultPromptList[i].Title, EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextArea(defaultPromptList[i].Content, codeStyle);
                }
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Copy", GUILayout.ExpandWidth(true)))
            {
                EditorGUIUtility.systemCopyBuffer = defaultPromptList[i].Content;
                string helpBoxMessage = "Copied prompt to clipboard.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            }
        }
    }

    /// <summary> Loads the prompt list from the json file. </summary>
    /// <returns> The loaded prompt list. </returns>
    public static List<(string Title, string Content)> LoadPromptListFromJson()
    {
        List<(string Title, string Content)> loadedPromptList = new();
        string helpBoxMessage;
        try
        {
            string filePath =
                AISettingsFileManager.GetInstance().UserFilesFolderPath + promptListFileName;
            // Create the file if it doesn't exist and saves it
            if (!FileManager<string>.CreateFileIfNotExisting(filePath))
            {
                helpBoxMessage = "No custom prompt list found. Creating default prompt list.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                loadedPromptList = defaultCustomPrompts;
                FileManager<List<(string, string)>>.SaveToJsonFileWithPath(
                    loadedPromptList,
                    filePath
                );
                return loadedPromptList;
            }
            else {
            loadedPromptList = FileManager<
                List<(string Title, string Content)>
            >.LoadDeserializedJsonFromPath(filePath);
            }

        }
        catch (Newtonsoft.Json.JsonException jsonEx)
        {
            helpBoxMessage = "JSON data does not match expected type." + "\n" + jsonEx.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
        }
        catch (SystemException e)
        {
            helpBox.UpdateMessage(
                "Error loading promptList from JSON: " + e.Message,
                MessageType.Error,
                false,
                true
            );
        }
        return loadedPromptList;
    }

    /// <summary> Saves the prompt list in the json file. </summary>
    private void SavePromptListInJson()
    {
        FileManager<List<(string, string)>>.SaveJsonFileToDefaultPath(
            CustomPromptList,
            promptListFileName
        );
        ResetCurrentPromptTexts();
        AIScript.ReloadPromptList();
        AIObjectGenerator.ReloadPromptList();
    }

    /// <summary> Renders the the new prompt field. </summary>
    private void RenderNewPromptField()
    {
        EditorGUILayout.BeginHorizontal();

        if (!addField && GUILayout.Button("Add New Prompt"))
        {
            addField = true;
        }
        else if (addField && GUILayout.Button("Cancel"))
        {
            addField = false;
            ResetKeyboardControl();
        }
        EditorGUILayout.EndHorizontal();

        if (addField)
        {
            using (new EditorGUI.DisabledScope(!addField))
            {
                newPromptTitle = EditorGUILayout.TextField("New Prompt Title", newPromptTitle);
                newPromptContent = EditorGUILayout.TextField(
                    "New Prompt Content",
                    newPromptContent
                );
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Add"))
                {
                    if (
                        string.IsNullOrEmpty(newPromptTitle)
                        || string.IsNullOrEmpty(newPromptContent)
                    )
                    {
                        string helpBoxMessage = "Prompt fields are empty.";
                        helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning, false, false);
                    }
                    else
                    {
                        var newPromptItem = (newPromptTitle, newPromptContent);
                        CustomPromptList.Add(newPromptItem);
                        SavePromptListInJson();
                        newPromptContent = "";
                        newPromptTitle = "";
                        selectedIndex = -1;
                        addField = false;
                        Repaint();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    /// <summary> Resets the current prompt texts to reload the custom prompt list. </summary>
    private void ResetCurrentPromptTexts()
    {
        if (CustomPromptList != null && CustomPromptList.Count > 0)
        {
            currentPromptTexts = CustomPromptList.ConvertAll(item => item.Content);
        }
    }
}
