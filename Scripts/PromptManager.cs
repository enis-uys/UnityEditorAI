using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class PromptManager : SingleExtensionApplication
{
    public override string DisplayName => "Prompt Manager";
    private Vector2 scrollPosition;

    private bool addField = false;
    private int selectedIndex = -1;
    string newPromptContent,
        newPromptTitle;
    public static readonly string promptListFileName = "promptList.json";
    private List<string> currentPromptTexts = new();
    GUIStyle codeStyle;
    private bool HasInit { get; set; } = false;

    private static List<(string Title, string Content)> customPromptList = new();
    public static List<(string Title, string Content)> CustomPromptList
    {
        get => customPromptList;
        set => customPromptList = value;
    }

    //These prompt get loaded if no custom prompts are found
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
    private readonly List<(string Title, string Content)> defaultPromptList =
        new()
        {
            OpenAiStandardPrompts.ScriptEndNote,
            OpenAiStandardPrompts.CreateNewScriptWithPrompt,
            OpenAiStandardPrompts.ObjectGenerationPrompt,
            OpenAiStandardPrompts.UpdateExistingScriptWithPrompt,
        };

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

    private void RenderPromptLists()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        RenderCustomPromptList();
        AddDefaultSpace();
        AddDefaultSpace();
        RenderDefaultPromptList();

        EditorGUILayout.EndScrollView();
    }

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
                GUILayout.BeginVertical();

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

                    GUILayout.BeginHorizontal();
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
                        GUILayout.EndHorizontal();
                    }
                }
                finally
                {
                    GUILayout.EndVertical();
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

    private void RenderDefaultPromptList()
    {
        GUILayout.Label(
            new GUIContent("Default Prompts:", "Default Prompts are not editable."),
            EditorStyles.boldLabel
        );
        for (int i = 0; i < defaultPromptList.Count; i++)
        {
            GUILayout.BeginVertical();
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
                GUILayout.EndVertical();
            }
            if (GUILayout.Button("Copy", GUILayout.ExpandWidth(true)))
            {
                EditorGUIUtility.systemCopyBuffer = defaultPromptList[i].Content;
                string helpBoxMessage = "Copied prompt to clipboard.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            }
        }
    }

    public static List<(string Title, string Content)> LoadPromptListFromJson()
    {
        List<(string Title, string Content)> loadedPromptList = new();
        try
        {
            string filePath =
                AISettingsFileManager.GetInstance().UserFilesFolderPath + promptListFileName;

            // Create the file if it doesn't exist and saves it
            if (
                !FileManager<List<(string Title, string Content)>>.CreateFileIfNotExisting(filePath)
            )
            {
                loadedPromptList = defaultCustomPrompts;
                FileManager<List<(string, string)>>.SaveToJsonFileWithPath(
                    loadedPromptList,
                    filePath
                );
                return loadedPromptList;
            }

            loadedPromptList = FileManager<
                List<(string Title, string Content)>
            >.LoadDeserializedJsonFromPath(filePath);
        }
        catch (Newtonsoft.Json.JsonException jsonEx)
        {
            string helpBoxMessage =
                "JSON data does not match expected type." + "\n" + jsonEx.Message;
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
                GUILayout.BeginHorizontal();

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
                GUILayout.EndHorizontal();
            }
        }
    }

    private void ResetCurrentPromptTexts()
    {
        if (CustomPromptList != null && CustomPromptList.Count > 0)
        {
            currentPromptTexts = CustomPromptList.ConvertAll(item => item.Content);
        }
    }
}
