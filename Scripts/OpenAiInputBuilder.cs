// for List<>
using System.Collections.Generic;
using UnityEngine;

/// <Title> Newtonsoft.JSON GitHub Repository </Title>
/// <Author> James Newton-King (JamesNK) </Author>
/// <Release Date> 08.03.2023 </Release Date>
/// <Access Date> 10.09.2023 </Access Date>
/// <Code version> 13.0.3 </Code version>
/// <Availability> https://github.com/JamesNK/Newtonsoft.Json </Availability>
/// <Usecase> JSON Serialization </Usecase>
/// <License> Open-Source MIT License https://opensource.org/licenses/MIT </License>
/// <Description>
///Newtonsoft.JSON is a popular .NET library for working with JSON data.
///It provides powerful JSON serialization and deserialization capabilities.
/// </Description>
using Newtonsoft.Json;

public class OpenAiInputBuilder
{
    public class RequestBuilder
    {
        private string model;
        private List<RequestMessage> messages = new();

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
                temperature = temperature ?? 1f,
            };

            return JsonConvert.SerializeObject(req, Formatting.Indented);
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
