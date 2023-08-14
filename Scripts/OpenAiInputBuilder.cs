// for List<>
using System.Collections.Generic;
using UnityEngine;

public class OpenAiInputBuilder
{
    public class RequestBuilder
    {
        private string model;
        private List<RequestMessage> messages = new List<RequestMessage>();

        private float temperature;
        public int maxTokens;

        public RequestBuilder WithModel(string model)
        {
            this.model = model;
            return this;
        }

        public RequestBuilder WithTemperature(float temperature)
        {
            this.temperature = temperature;
            return this;
        }

        public RequestBuilder WithMaxTokens(int maxTokens)
        {
            this.maxTokens = maxTokens;
            return this;
        }

        public RequestBuilder AddMessage(string role, string content)
        {
            messages.Add(new RequestMessage { role = role, content = content });
            return this;
        }

        public string Build()
        {
            var req = new Request
            {
                model = model,
                messages = messages.ToArray(),
                temperature = temperature,
                max_tokens = maxTokens
            };
            return JsonUtility.ToJson(req);
        }
    }

    [System.Serializable]
    public struct Response
    {
        public string id;
        public ResponseChoice[] choices;
    }

    [System.Serializable]
    public class RequestMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Request
    {
        public string model;
        public RequestMessage[] messages;

        public float temperature;
        public int max_tokens;
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
