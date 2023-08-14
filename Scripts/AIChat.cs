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

    // This can be used later to customize the chat window
    private GUIStyle richTextStyle;

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
        public List<string> messageListCon = new List<string>();
    }

    private List<string> messageHistoryList = new List<string>();

    private Vector2 inputScrollPosition;
    private Vector2 outputScrollPosition;

    HelpBox helpBox = HelpBox.GetInstance();

    private const string InputFieldKey = "InputField";
    private const string OutputFieldKey = "OutputFieldKey";

    /// <summary>
    /// GUI callback for rendering the AI Chat extension.
    /// </summary>
    public override void OnGUI()
    {
        try
        {
            LoadEditorPrefs();
            EditorGUILayout.BeginVertical("Box");
            InitializeRichTextStyle();
            RenderInputField();
            GUILayout.Space(20);
            RenderOutputField();
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
            richTextStyle = new GUIStyle(GUI.skin.textArea);
            richTextStyle.richText = true;
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
                helpBox.UpdateHelpBoxMessageAndType(
                    "An error occurred during AI processing: " + ex.Message,
                    MessageType.Error
                );
                Debug.LogError("An error occurred during AI processing: " + ex.Message);
            }
        }

        GUILayout.EndHorizontal();
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
            messageHistoryOutputField,
            richTextStyle,
            GUILayout.ExpandHeight(true)
        );
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Lorem Ipsum Test"))
        {
            try
            {
                AddLongMessageForTest();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("An error occurred during AI processing: " + ex.Message);
            }
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
        EditorGUILayout.HelpBox(helpBox.HelpBoxMessage, helpBox.HelpBoxMessageType);
    }

    /// <summary>
    /// Sends the user input to the AI model for processing.
    /// </summary>
    /// <param name="input">The user input message.</param>
    private async void GptInputSend(string input)
    {
        var gptResponse = await OpenAiManager.ChatToGpt(input);
        UpdateListToGui(input, gptResponse);
        GUIUtility.keyboardControl = 0;
        inputText = "";
    }

    /// <summary>
    /// Updates the message history and GUI output with the user input and AI response.
    /// </summary>
    /// <param name="input">The user input message.</param>
    /// <param name="output">The AI-generated response.</param>
    private void UpdateListToGui(string input, string output)
    {
        AddMessageToList("User", input, "#6DBE44");
        AddMessageToList("System", output, "#F2A900");
        messageHistoryOutputField = MessageHistoryToString();
    }

    /// <summary>
    /// Adds a message to the message history list with the specified sender and color.
    /// </summary>
    /// <param name="sender">The sender of the message.</param>
    /// <param name="message">The content of the message.</param>
    /// <param name="color">The color of the message.</param>
    public void AddMessageToList(string sender, string message, string color)
    {
        string formattedMessage = $"<color={color}>{sender}:</color> {message}";
        messageHistoryList.Add(formattedMessage);
    }

    /// <summary>
    /// Converts the message history list to a single string.
    /// </summary>
    /// <returns>The message history as a string.</returns>
    private string MessageHistoryToString()
    {
        return string.Join("\n", messageHistoryList);
    }

    /// <summary>
    /// Loads the message history from a file.
    /// </summary>
    private void LoadMessageHistoryFromFile()
    {
        messageHistoryList = FileManager<List<string>>.LoadDeserializedJsonFromDefaultPath(
            "MessageHistory"
        );
        messageHistoryOutputField = MessageHistoryToString();
    }

    /// <summary>
    /// Saves the message history to a file.
    /// </summary>
    private void SaveMessageHistoryToFile()
    {
        FileManager<List<string>>.SaveJsonToDefaultPath(messageHistoryList, "MessageHistory");
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
    private void AddLongMessageForTest()
    {
        var lorem = "Lorem Ipsum doremi fas soll la to di \n";
        messageHistoryOutputField += "Lorem: " + lorem + lorem + lorem + lorem;
    }

    private void LoadEditorPrefs()
    {
        if (EditorPrefs.HasKey(InputFieldKey))
        {
            inputText = EditorPrefs.GetString(InputFieldKey);
        }
        if (EditorPrefs.HasKey(OutputFieldKey))
        {
            messageHistoryOutputField = EditorPrefs.GetString(OutputFieldKey);
        }
    }

    private void SetEditorPrefs()
    {
        if (!string.IsNullOrEmpty(inputText))
        {
            EditorPrefs.SetString(InputFieldKey, inputText);
        }
        if (!string.IsNullOrEmpty(messageHistoryOutputField))
        {
            EditorPrefs.SetString(OutputFieldKey, messageHistoryOutputField);
        }
    }
}
