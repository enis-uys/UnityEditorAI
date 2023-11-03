using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System;

public class AiAssistant : SingleExtensionApplication
{
    /// <summary>
    /// The display name of the AI Assistant extension.
    /// </summary>
    public override string DisplayName => "AI Assistant";

    private string inputText = "";
    GUIStyle richTextStyle;
    private const string messageHistoryFileName = "messageHistory.json";
    private string messageHistoryOutputField = "";
    private Vector2 inputScrollPosition;
    private Vector2 outputScrollPosition;

    private readonly MessageListBuilder messageHistoryListBuilder = new();
    public bool HasInit { get; set; } = false;

    // Later on, we will add a list of conversations and replace the messageHistoryOutputField with a dropdown
    // This is necessary for being able to ask about old messages
    //TODO: Implement
    public class Conversation
    {
        /// <summary>
        /// The name of the conversation.
        /// </summary>
        public string conversationName;

        /// <summary>
        /// The list of messages in the conversation.
        /// </summary>
        public MessageListBuilder messageListBuilder;
    }

    /// <summary>
    /// GUI callback for rendering the AI Chat extension.
    /// </summary>
    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            if (!HasInit)
            {
                LoadEditorPrefs();
                HasInit = true;
            }
            richTextStyle = CreateRichTextStyle();
            RenderInputField();
            AddDefaultSpace();
            RenderConversationField();
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
            GUILayout.ExpandHeight(true)
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
                    ReadInputAndSendToGPT(inputText);
                }
                catch (Exception ex)
                {
                    string helpBoxMessage =
                        "An error occurred while processing the input." + ex.Message;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                    FinishProgressBarWithDelay();
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
    private void RenderConversationField()
    {
        EditorGUILayout.LabelField("Conversation:");
        outputScrollPosition = EditorGUILayout.BeginScrollView(
            outputScrollPosition,
            GUILayout.ExpandHeight(true)
        );
        try
        {
            using (new EditorGUI.DisabledScope(true))
            {
                messageHistoryOutputField = EditorGUILayout.TextArea(
                    MessageHistoryListToFormatedString(messageHistoryListBuilder),
                    richTextStyle,
                    GUILayout.ExpandHeight(true)
                );
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
        GUILayout.BeginHorizontal();
        try
        {
            if (GUILayout.Button("Clear"))
            {
                ClearMessageHistory();
            }
            if (GUILayout.Button("Copy Conversation"))
            {
                EditorGUIUtility.systemCopyBuffer = messageHistoryOutputField;
            }
            if (GUILayout.Button("Load Conversation"))
            {
                LoadMessageHistoryFromFile();
            }
            if (GUILayout.Button("Save Conversation"))
            {
                SaveMessageHistoryToFile();
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Sends the user input to the AI model for processing.
    /// </summary>
    /// <param name="input">The user input message.</param>
    private async void ReadInputAndSendToGPT(string input)
    {
        ResetKeyboardControl();
        inputText = "";
        ShowProgressBar(0.1f);
        AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();
        MessageListBuilder tempMessageListBuilder = new();
        int lastMessagesCount = settingsFM.LastMessagesToSend;
        if (lastMessagesCount > 0 && messageHistoryListBuilder.GetMessageCount() > 0)
        {
            int messageCount = messageHistoryListBuilder.GetMessageCount();
            for (int i = messageCount - lastMessagesCount; i < messageCount; i++)
            {
                tempMessageListBuilder.AddMessage(messageHistoryListBuilder.GetMessageAt(i));
            }
        }
        tempMessageListBuilder.AddMessage(input, "user");

        var gptResponse = await OpenAiApiManager.RequestToGpt(tempMessageListBuilder);
        Debug.Log("GPT response: " + gptResponse);
        if (gptResponse != null)
        {
            messageHistoryListBuilder.AddMessage(input, "user");
            messageHistoryListBuilder.AddMessage(gptResponse, "assistant");
            helpBox.UpdateMessage("Message received from GPT", MessageType.Info);
        }
        else
        {
            string helpBoxMessage = "GPT did not answer.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
        }
        FinishProgressBarWithDelay();
    }

    private string MessageHistoryListToFormatedString(MessageListBuilder messageListBuilder)
    {
        List<string> formattedMessageList = new();

        foreach (var requestMessage in messageListBuilder.Build())
        {
            string role = requestMessage.role;
            string content = requestMessage.content;
            string color = "#F2A900";
            if (role == "user")
            {
                color = "#6DBE44";
            }
            string formattedMessage = $"<color={color}>{role}:</color> {content}";
            // Add a new line for better readability if the sender is "assistant"
            if (role == "assistant")
            {
                formattedMessage += "\n";
            }
            formattedMessageList.Add(formattedMessage);
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

    /// <summary>
    /// Loads the message history from a file.
    /// </summary>
    private void LoadMessageHistoryFromFile()
    {
        List<MessageListBuilder.RequestMessage> loadedMessageHistory = new();
        try
        {
            var loadedData = FileManager<
                List<MessageListBuilder.RequestMessage>
            >.LoadDeserializedJsonPanel("Load the message history from a file");
            if (loadedData != null)
            {
                loadedMessageHistory = loadedData;
            }
            else
            {
                string helpBoxMessage = "No message history selected.";
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error);
            }
        }
        catch (Newtonsoft.Json.JsonException jsonEx)
        {
            string helpBoxMessage =
                "JSON data does not match expected type." + "\n" + jsonEx.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
        }
        catch (Exception ex)
        {
            string helpBoxMessage =
                "An error occurred while loading the message history." + ex.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
        }
        messageHistoryListBuilder.ClearMessages().AddMessages(loadedMessageHistory);
    }

    /// <summary>
    /// Saves the message history to a file.
    /// </summary>
    private void SaveMessageHistoryToFile()
    {
        FileManager<List<MessageListBuilder.RequestMessage>>.SaveJsonFileToDefaultPath(
            messageHistoryListBuilder.Build(),
            messageHistoryFileName
        );
    }

    /// <summary>
    /// Clears the message history.
    /// </summary>
    private void ClearMessageHistory()
    {
        messageHistoryOutputField = "";
        messageHistoryListBuilder.ClearMessages();
    }

    public enum EditorPrefKey
    {
        InputText,
        MessageHistoryListJson
    }

    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.InputText, "InputTextKey" },
            { EditorPrefKey.MessageHistoryListJson, "MessageHistoryListJsonKey" }
        };

    private void SetEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.InputText:
                    EditorPrefs.SetString(kvp.Value, inputText);
                    break;
                case EditorPrefKey.MessageHistoryListJson:
                    // Serialize the messageHistoryList to JSON
                    string messageHistoryListJson = FileManager<
                        List<MessageListBuilder.RequestMessage>
                    >.SerializeDataToJson(messageHistoryListBuilder.Build());
                    EditorPrefs.SetString(kvp.Value, messageHistoryListJson);
                    break;
            }
        }
    }

    private void LoadEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.InputText:
                    inputText = EditorPrefs.GetString(kvp.Value);
                    break;
                case EditorPrefKey.MessageHistoryListJson:
                    // Retrieve the serialized JSON string from EditorPrefs
                    string messageHistoryListJson = EditorPrefs.GetString(kvp.Value);
                    if (!string.IsNullOrEmpty(messageHistoryListJson))
                    {
                        List<MessageListBuilder.RequestMessage> messageHistoryList = FileManager<
                            List<MessageListBuilder.RequestMessage>
                        >.DeserializeJsonString(messageHistoryListJson);
                        messageHistoryListBuilder.ClearMessages();
                        if (messageHistoryList != null)
                        {
                            messageHistoryListBuilder.AddMessages(messageHistoryList);
                        }
                        else
                        {
                            string helpBoxMessage = "Deserialized message history list is null.";
                            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                        }
                        // Deserialize the JSON string back to a list of RequestMessages
                    }
                    break;
            }
        }
    }
}
