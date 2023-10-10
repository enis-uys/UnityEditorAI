using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class PromptManager : SingleExtensionApplication
{
    public override string DisplayName => "Prompt Manager";
    private Vector2 scrollPosition;

    private bool addField = false,
        hasInit = false;
    private int selectedIndex = -1;
    string newPromptContent,
        newPromptTitle;
    public static readonly string promptListFileName = "promptList.json";
    private List<string> currentPromptTexts = new();

    public override bool ShouldLoadEditorPrefs { get; set; } = false;

    private List<(string Title, string Content)> customPromptList = new();
    public List<(string Title, string Content)> CustomPromptList
    {
        get => customPromptList;
        set => customPromptList = value;
    }

    private readonly List<(string Title, string Content)> defaultPromptList =
        new()
        {
            ("Script End Note", OpenAiStandardPrompts.ScriptEndNote),
            ("Create New Script With Prompt", OpenAiStandardPrompts.CreateNewScriptWithPrompt),
            ("Create New Object With Prompt", OpenAiStandardPrompts.ObjectGenerationPrompt),
            (
                "Update Existing Script With Prompt",
                OpenAiStandardPrompts.UpdateExistingScriptWithPrompt
            )
        };

    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            if (!hasInit)
            {
                CustomPromptList = LoadPromptListFromJson();
                ResetCurrentPromptTexts();
                hasInit = true;
            }
            InitializeGuiStyles();
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
        RenderDefaultPromptList();

        EditorGUILayout.EndScrollView();
    }

    private void RenderCustomPromptList()
    {
        GUILayout.Label(
            new GUIContent(
                "Custom Prompts",
                "Custom Prompts are saved inside the extension's userfiles folder."
            ),
            EditorStyles.boldLabel
        );
        if (CustomPromptList == null || CustomPromptList.Count == 0)
        {
            helpBox.UpdateMessage("No custom prompts found.", MessageType.Warning, false, false);
        }
        else
        {
            int itemToRemove = -1; // Track the item to remove from the list
            for (int i = 0; i < CustomPromptList.Count; i++)
            {
                GUILayout.BeginVertical();

                try
                {
                    GUILayout.Label(CustomPromptList[i].Title, EditorStyles.boldLabel);
                    using (new EditorGUI.DisabledScope(selectedIndex != i))
                    {
                        currentPromptTexts[i] = EditorGUILayout.TextArea(
                            currentPromptTexts[i],
                            richTextStyle
                        );

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Delete", GUILayout.ExpandWidth(true)))
                        {
                            selectedIndex = -1;
                            ResetKeyboardControl();
                            itemToRemove = i;
                            ResetCurrentPromptTexts();
                        }
                        if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
                        {
                            selectedIndex = -1;
                            ResetKeyboardControl();
                            var updatedItem = (CustomPromptList[i].Title, currentPromptTexts[i]);
                            CustomPromptList[i] = updatedItem;
                            SavePromptListInJson();
                            ResetCurrentPromptTexts();
                        }
                    }
                    if (selectedIndex != i && GUILayout.Button("Edit", GUILayout.ExpandWidth(true)))
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
                    GUILayout.EndHorizontal();
                }
                finally
                {
                    GUILayout.EndVertical();
                }
            }
            if (itemToRemove >= 0)
            {
                CustomPromptList.RemoveAt(itemToRemove);
            }
        }
    }

    private void RenderDefaultPromptList()
    {
        GUILayout.Label(
            new GUIContent("Default Prompts", "Default Prompts are not editable."),
            EditorStyles.boldLabel
        );
        for (int i = 0; i < defaultPromptList.Count; i++)
        {
            GUILayout.BeginVertical();
            try
            {
                GUILayout.Label(defaultPromptList[i].Title, EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextArea(defaultPromptList[i].Content, richTextStyle);
                }
            }
            finally
            {
                GUILayout.EndVertical();
            }
        }
    }

    private List<(string Title, string Content)> LoadPromptListFromJson()
    {
        List<(string Title, string Content)> loadedPromptList = new();
        try
        {
            loadedPromptList = FileManager<
                List<(string Title, string Content)>
            >.LoadDeserializedJsonFromPath(
                AISettingsFileManager.GetInstance().UserFilesFolderPath,
                promptListFileName
            );
        }
        catch (Newtonsoft.Json.JsonException jsonEx)
        {
            string helpBoxMessage =
                "JSON data does not match expected type." + "\n" + jsonEx.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            helpBox.FinishProgressBarWithDelay(helpBox.ProgressBarDelayInMilliseconds);
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
        FileManager<List<(string, string)>>.SaveFileToDefaultPath(
            CustomPromptList,
            promptListFileName
        );
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
                newPromptContent = EditorGUILayout.TextField("New Prompt Title", newPromptContent);
                newPromptTitle = EditorGUILayout.TextField("New Prompt Content", newPromptTitle);
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
