using System;
using System.Collections.Generic;
using LeastSquares;
using UnityEngine;

public class OpenAiInputBuilder
{
    public static MessageListBuilder CreateMessageList()
    {
        return new MessageListBuilder();
    }

    public class RequestBuilder
    {
        private string model,
            prompt;
        private MessageListBuilder messageListBuilder = new();
        private float? temperature;

        public RequestBuilder WithModel(string model)
        {
            this.model = model;
            return this;
        }

        public RequestBuilder WithTemperature(float? temperature)
        {
            this.temperature = temperature;
            return this;
        }

        public RequestBuilder WithMessageListBuilder(MessageListBuilder messageListBuilder)
        {
            this.messageListBuilder = messageListBuilder;
            return this;
        }

        public RequestBuilder WithPrompt(string prompt)
        {
            this.prompt = prompt;
            return this;
        }

        public string Build()
        {
            var req = new Request
            {
                model = model,
                messages = messageListBuilder.Build(),
                temperature = temperature.Value,
            };
            return FileManager<Request>.SerializeDataToJson(req);
        }

        public string BuildCompletionRequest()
        {
            var request = new CompletionRequest()
            {
                model = model,
                prompt = prompt,
                temperature = temperature ?? 0.0f
            };
            return JsonUtility.ToJson(request);
        }
    }

    [Serializable]
    public class Request
    {
        public string model;
        public List<MessageListBuilder.RequestMessage> messages;

        public float temperature;
    }

    [Serializable]
    public class CompletionRequest
    {
        public string model;
        public string prompt;
        public float temperature;
    }

    [Serializable]
    public struct Response
    {
        public string id;
        public ResponseChoice[] choices;

        [Serializable]
        public struct ResponseChoice
        {
            public int index;
            public ResponseMessage message;

            [Serializable]
            public struct ResponseMessage
            {
                public string role;
                public string content;
            }
        }
    }

    [Serializable]
    public struct CompletionResponse
    {
        public string id;
        public CompletionResponseChoice[] choices;

        [Serializable]
        public struct CompletionResponseChoice
        {
            public int index;
            public string text;
        }
    }
}
