using UnityEngine;

/// <summary> The class that builds the request. </summary>
public class OpenAiInputBuilder
{
    /// <summary> Function that creates a messageList builder.</summary>
    /// <returns> The messageList builder.</returns>
    public static MessageListBuilder CreateMessageList()
    {
        return new MessageListBuilder();
    }

    /// <summary> The message list builder class that contains the message list. </summary>
    public class RequestBuilder
    {
        /// <summary> The model of the request.</summary>
        private string model,
            /// <summary> The prompt of the request.</summary>
            prompt;

        /// <summary> The message list builder of the request. </summary>
        private MessageListBuilder messageListBuilder = new();

        /// <summary> The temperature of the request. </summary>
        private float? temperature;

        /// <summary> The model of the request. </summary>
        /// <param name="model"> The model of the request. </param>
        /// <returns> The request builder. </returns>
        public RequestBuilder WithModel(string model)
        {
            this.model = model;
            return this;
        }

        /// <summary> The temperature of the request. </summary>
        /// <param name="temperature"> The temperature of the request. </param>
        /// <returns> The request builder. </returns>
        public RequestBuilder WithTemperature(float? temperature)
        {
            this.temperature = temperature;
            return this;
        }

        /// <summary> The message list builder of the request. </summary>
        /// <param name="messageListBuilder"> The message list builder of the request. </param>
        /// <returns> The request builder. </returns>
        public RequestBuilder WithMessageListBuilder(MessageListBuilder messageListBuilder)
        {
            this.messageListBuilder = messageListBuilder;
            return this;
        }

        /// <summary> The prompt of the request. Used for completion requests. </summary>
        /// <param name="prompt"> The prompt of the request
        /// <returns> The request builder. </returns>
        public RequestBuilder WithPrompt(string prompt)
        {
            this.prompt = prompt;
            return this;
        }

        /// <summary> The function that builds the request. </summary>
        /// <returns> The request. </returns>
        public string Build()
        {
            var req = new GptRequest
            {
                model = model,
                messages = messageListBuilder.Build(),
                temperature = temperature.Value,
            };
            return FileManager<GptRequest>.SerializeDataToJson(req);
        }

        /// <summary> The function that builds the completion request. </summary>
        /// <returns> The request. </returns>
        public string BuildCompletionRequest()
        {
            var request = new GptCompletionRequest()
            {
                model = model,
                prompt = prompt,
                temperature = temperature ?? 0.0f
            };
            return JsonUtility.ToJson(request);
        }
    }
}
