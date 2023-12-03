using System;
using System.Collections.Generic;

[Serializable]
/// <summary> A single request message class that contains the content and role of a message. Is used inside an ApiRequest. </summary>
public class RequestMessage
{
    /// <summary> The content of the message. </summary>
    public string content;

    /// <summary> The role of the message. </summary>
    public string role;

    /// <summary> The constructor for the request message. </summary>
    /// <param name="content"> The content of the message. </param>
    /// <param name="role"> The role of the message. </param>
    public RequestMessage(string content, string role = "user")
    {
        this.content = content;
        this.role = role;
    }
}

/// <summary> The request class that contains the model, messages, and temperature.  </summary>
[Serializable]
public class GptRequest
{
    /// <summary> The model of the request. </summary>
    public string model;

    /// <summary> The messages of the request. </summary>
    public List<RequestMessage> messages;

    /// <summary> The temperature of the request. It represents the randomness of the response. </summary>

    public float temperature;
}

/// <summary> The response class that contains the id and choices. </summary>
[Serializable]
public class GptResponse
{
    /// <summary> The id of the response. </summary>
    public string id;

    /// <summary> The choices of the response. Gpt might give more than one response. </summary>
    public ResponseChoice[] choices;

    /// <summary> The response choice class that contains the index and message. </summary>
    [Serializable]
    public struct ResponseChoice
    {
        /// <summary> The index of the response choice. </summary>
        public int index;

        /// <summary> The message of the response choice. </summary>
        public ResponseMessage message;

        /// <summary> The response message class that contains the role and content. </summary>
        [Serializable]
        public struct ResponseMessage
        {
            public string role;
            public string content;
        }
    }
}

/// <summary> The completion request class that contains the model, prompt, and temperature. </summary>
[Serializable]
public class GptCompletionRequest
{
    /// <summary> The model of the completion request. </summary>
    public string model;

    /// <summary> The prompt of the completion request. </summary>
    public string prompt;

    /// <summary> The temperature of the completion request. </summary>
    public float temperature;
}

/// <summary> The completion response class that contains the id and response choices. </summary>
[Serializable]
public struct GptCompletionResponse
{
    /// <summary> The id of the response. </summary>
    public string id;

    // The choices of the response. Gpt might give more than one response.
    public CompletionResponseChoice[] choices;

    /// <summary> The completion response choice class that contains the index and text. </summary>
    [Serializable]
    public struct CompletionResponseChoice
    {
        /// <summary> The index of the response choice. </summary>
        public int index;

        /// <summary> The text of the response choice. </summary>
        public string text;
    }
}
