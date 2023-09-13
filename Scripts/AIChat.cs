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
    private GUIStyle richTextStyle;

    private bool shouldLoadEditorPrefs = true;

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
        public List<string> messageListConversation = new List<string>();
    }

    private List<string> messageHistoryList = new List<string>();

    public enum EditorPrefKey
    {
        InputText,
        MessageHistoryCount
    }

    private Dictionary<EditorPrefKey, string> editorPrefKeys = new Dictionary<EditorPrefKey, string>
    {
        { EditorPrefKey.InputText, "InputText" },
        { EditorPrefKey.MessageHistoryCount, "MessageHistoryCount" }
    };

    /// <summary>
    /// GUI callback for rendering the AI Chat extension.
    /// </summary>
    public override void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginVertical("Box");
            if (shouldLoadEditorPrefs)
            {
                LoadEditorPrefs();
                shouldLoadEditorPrefs = false;
            }
            InitializeRichTextStyle();
            RenderInputField();
            GUILayout.Space(defaultSpace);
            RenderOutputField();
            GUILayout.Space(defaultSpace);
            EditorGUILayout.HelpBox(helpBox.HBMessage, helpBox.HBMessageType);
            helpBox.RenderProgressBar();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Initializes the rich text style for the output field.
    /// </summary>
    private void InitializeRichTextStyle()
    {
        if (richTextStyle == null)
        {
            richTextStyle = new GUIStyle(GUI.skin.textArea) { richText = true };
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
                GUIUtility.keyboardControl = 0;
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
                    helpBox.UpdateMessageAndType(
                        "An error occurred during AI processing: " + ex.Message,
                        MessageType.Error
                    );
                    helpBox.FinishProgressBarWithDelay(helpBox.ProgressBarDelayInMilliseconds);
                    Debug.LogError("An error occurred during AI processing: " + ex.Message);
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

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextArea(
            MessageHistoryListToFormatedString(messageHistoryList),
            richTextStyle,
            GUILayout.ExpandHeight(true)
        );
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Lorem Ipsum Test"))
        {
            AddLongMessageForTest();
        }
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
        GUIUtility.keyboardControl = 0;
        inputText = "";
        helpBox.SetProgressBarProgress(0.2f);

        var gptResponse = await OpenAiManager.ChatToGpt(input);

        AddRoleMessageToMessageList("User", input);
        AddRoleMessageToMessageList("System", gptResponse);
        helpBox.FinishProgressBarWithDelay(helpBox.ProgressBarDelayInMilliseconds);
    }

    private void AddRoleMessageToMessageList(string role, string message)
    {
        string roleMessage = $"{role}: {message}";
        messageHistoryList.Add(roleMessage);
    }

    private string MessageHistoryListToFormatedString(List<string> messageList)
    {
        List<string> formattedMessageList = new List<string>();
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
            helpBox.UpdateMessageAndType(
                "No right message history file selected.",
                MessageType.Warning
            );
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

    /// <summary>
    /// Adds a long test message for testing purposes.
    /// </summary>
    /// TODO: Remove this method when not needed
    private void AddLongMessageForTest()
    {
        var lorem = "Lorem Ipsum doremi fas soll la to di \n";
        messageHistoryOutputField += "\nLorem: " + lorem + lorem + lorem + lorem;
    }

    private void LoadEditorPrefs()
    {
        string loadedInputText = "";
        List<string> loadedMessageHistoryList = new List<string>();

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
