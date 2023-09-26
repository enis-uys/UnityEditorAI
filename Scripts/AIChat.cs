using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

public class AIChat : SingleExtensionApplication
{
    /// <summary>
    /// The display name of the AI Chat extension.
    /// </summary>
    public override string DisplayName => "AI Chat";

    private string inputText = "";
    private string messageHistoryOutputField = "";
    private Vector2 inputScrollPosition;
    private Vector2 outputScrollPosition;

    // This can be used later to customize the chat window

    public override bool ShouldLoadEditorPrefs { get; set; } = true;

    // Later on, we will add a list of conversations and replace the messageHistoryOutputField with a dropdown
    // This is necessary for being able to ask about old messages
    public class Conversation
    {
        /// <summary>
        /// The name of the conversation.
        /// </summary>
        public string conversationName;

        /// <summary>
        /// The list of messages in the conversation.
        /// </summary>
        public List<string> messageListConversation = new();
    }

    private List<string> messageHistoryList = new();

    /// <summary>
    /// GUI callback for rendering the AI Chat extension.
    /// </summary>
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
            InitializeRichTextStyle();
            RenderInputField();
            AddDefaultSpace();
            RenderOutputField();
            AddDefaultSpace();
            RenderHelpBox();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Renders the input field for user input.
    /// </summary>
    private void RenderInputField()
    {
        EditorGUILayout.LabelField("Input:");
        inputScrollPosition = EditorGUILayout.BeginScrollView(
            inputScrollPosition,
            GUILayout.MinHeight(150)
        );
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
        try
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear"))
            {
                ResetKeyboardControl();
                inputText = "";
            }
            if (
                GUILayout.Button("Send", GUILayout.ExpandWidth(true))
                && !string.IsNullOrEmpty(inputText)
            )
            {
                try
                {
                    GptInputSend(inputText);
                }
                catch (System.Exception ex)
                {
                    string helpBoxMessage =
                        "An error occurred while processing the input." + ex.Message;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                    helpBox.FinishProgressBarWithDelay(helpBox.ProgressBarDelayInMilliseconds);
                }
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Renders the output field for displaying the chat history.
    /// </summary>
    private void RenderOutputField()
    {
        EditorGUILayout.LabelField("Output:");
        outputScrollPosition = EditorGUILayout.BeginScrollView(
            outputScrollPosition,
            GUILayout.ExpandHeight(true)
        );

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextArea(
                MessageHistoryListToFormatedString(messageHistoryList),
                richTextStyle,
                GUILayout.ExpandHeight(true)
            );
        }

        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear"))
        {
            ClearMessageHistory();
        }
        if (GUILayout.Button("Load Conversation"))
        {
            LoadMessageHistoryFromFile();
        }
        if (GUILayout.Button("Save Conversation"))
        {
            SaveMessageHistoryToFile();
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Sends the user input to the AI model for processing.
    /// </summary>
    /// <param name="input">The user input message.</param>
    private async void GptInputSend(string input)
    {
        ResetKeyboardControl();
        inputText = "";
        helpBox.SetProgressBarProgress(0.2f);

        var gptResponse = await OpenAiApiManager.ChatToGpt(input);
        AddRoleMessageToMessageList("User", input);
        AddRoleMessageToMessageList("System", gptResponse);
        helpBox.UpdateMessage("Message sent to GPT", MessageType.Info);
        helpBox.FinishProgressBarWithDelay(helpBox.ProgressBarDelayInMilliseconds);
    }

    private void AddRoleMessageToMessageList(string role, string message)
    {
        string roleMessage = $"{role}: {message}";
        messageHistoryList.Add(roleMessage);
    }

    private string MessageHistoryListToFormatedString(List<string> messageList)
    {
        List<string> formattedMessageList = new();
        for (int i = 0; i < messageList.Count; i++)
        {
            string[] messageParts = messageList[i].Split(": ");
            if (messageParts.Length >= 2)
            {
                string sender = messageParts[0];
                string content = messageParts[1];
                string color = "#F2A900";
                if (sender == "User")
                {
                    color = "#6DBE44";
                }
                string formattedMessage = $"<color={color}>{sender}:</color> {content}";
                if (sender == "System")
                {
                    formattedMessage += "\n";
                }
                formattedMessageList.Add(formattedMessage);
            }
        }
        if (formattedMessageList.Count > 0)
        {
            return string.Join("\n", formattedMessageList);
        }
        else
        {
            return "";
        }
    }

    public static bool IsValidMessageHistory(List<string> messageList)
    {
        // Check if 'data' is a list of strings in the expected format

        foreach (string message in messageList)
        {
            // Check if each message follows the expected format
            if (!ScriptUtil.IsValidMessageFormat(message))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Loads the message history from a file.
    /// </summary>
    private void LoadMessageHistoryFromFile()
    {
        List<string> loadedHistory = FileManager<List<string>>.LoadDeserializedJsonPanel(
            "Load the message history from a file"
        );

        if (loadedHistory != null && IsValidMessageHistory(loadedHistory))
        {
            messageHistoryList = loadedHistory;
            messageHistoryOutputField = MessageHistoryListToFormatedString(messageHistoryList);
        }
        else
        {
            string helpBoxMessage = "The loaded message history is not valid.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning);
        }
    }

    /// <summary>
    /// Saves the message history to a file.
    /// </summary>
    private void SaveMessageHistoryToFile()
    {
        FileManager<List<string>>.SaveJsonToDefaultPath(messageHistoryList, "MessageHistory.json");
    }

    /// <summary>
    /// Clears the message history.
    /// </summary>
    private void ClearMessageHistory()
    {
        messageHistoryOutputField = "";
        messageHistoryList.Clear();
    }

    public enum EditorPrefKey
    {
        InputText,
        MessageHistoryCount
    }

    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputText, "InputText" },
            { EditorPrefKey.MessageHistoryCount, "MessageHistoryCount" }
        };

    private void LoadEditorPrefs()
    {
        string loadedInputText = "";
        List<string> loadedMessageHistoryList = new();

        foreach (var kvp in editorPrefKeys)
        {
            if (EditorPrefs.HasKey(kvp.Value))
            {
                switch (kvp.Key)
                {
                    case EditorPrefKey.InputText:
                        loadedInputText = EditorPrefs.GetString(kvp.Value);
                        break;
                    //in the case of the message history, the count get loaded first
                    case EditorPrefKey.MessageHistoryCount:
                        //then the messages get loaded by index
                        for (int i = 0; i < EditorPrefs.GetInt(kvp.Value); i++)
                        {
                            loadedMessageHistoryList.Add(
                                EditorPrefs.GetString("MessageHistory" + i)
                            );
                        }
                        break;
                }
            }
        }
        inputText = loadedInputText;
        messageHistoryList = loadedMessageHistoryList;
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
                case EditorPrefKey.MessageHistoryCount:
                    EditorPrefs.SetInt(kvp.Value, messageHistoryList.Count);
                    string messageHistoryKey = "MessageHistory";
                    int i = 0;
                    foreach (string message in messageHistoryList)
                    {
                        EditorPrefs.SetString(messageHistoryKey + i, message);
                        i++;
                    }
                    break;
            }
        }
    }
}
