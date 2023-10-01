using System;
using System.Collections.Generic;

using UnityEngine;

public class OpenAiInputBuilder
{
    public static MessageListBuilder CreateMessageList()
    {
        return new MessageListBuilder();
    }

    public class RequestBuilder
    {
        private string model;
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
    }

    [System.Serializable]
    public class Request
    {
        public string model;
        public List<MessageListBuilder.RequestMessage> messages;

        public float temperature;
    }

    //TODO: Explain? why ResponseChoice ...
    [System.Serializable]
    public struct Response
    {
        public string id;
        public ResponseChoice[] choices;
    }

    [System.Serializable]
    public struct ResponseChoice
    {
        public int index;
        public ResponseMessage message;

        [System.Serializable]
        public struct ResponseMessage
        {
            public string role;
            public string content;
        }
    }
}
