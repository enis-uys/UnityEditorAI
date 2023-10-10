using System;
using UnityEngine;
using UnityEngine.Networking;

/// <Title> UniTask GitHub Repository </Title>
/// <Author> Yoshifumi Kawai / Cysharp, Inc. (Cysharp   ) </Author>
/// <Release Date> 01.11.2022 </Release Date>
/// <Access Date> 10.09.2023 </Access Date>
/// <Code version> 2.3.3 </Code version>
/// <Availability> https://github.com/Cysharp/UniTask </Availability>
/// <Usecase> Asynchronous programming </Usecase>
/// <License> Open-Source MIT License https://opensource.org/licenses/MIT </License>
/// <Description>
/// Every Task is wrapped in a UniTask. UniTask is a wrapper for Task and Task<T>.
/// It is a lightweight alternative to C# async/await for Unity. It is faster than async/await and allocates less memory.
/// It is also compatible with the Unity SynchronizationContext, so you can use it for Unity's main thread and coroutine.
/// </Description>
using Cysharp.Threading.Tasks;

//TODO: Why? if UNITY_EDITOR is not defined, the code is not executed
#if UNITY_EDITOR
using UnityEditor;

public class OpenAiApiManager
{
    const string chatEndpoint = "https://api.openai.com/v1/chat/completions";

    //for davinci model
    const string completionsEndpoint = "https://api.openai.com/v1/completions";

    private static readonly AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();

    public static async UniTask<string> RequestToGpt(string requestMessage)
    {
        try
        {
            MessageListBuilder messageListBuilder = new();
            messageListBuilder.AddMessage(requestMessage, "user");
            return await SendMessagesToGpt(
                settingsFM.ApiKey,
                messageListBuilder,
                settingsFM.SelectedGptModel,
                settingsFM.Temperature,
                settingsFM.TimeoutInSeconds
            );
        }
        catch (Exception e)
        {
            ErrorMessage(e);
            return null;
        }
    }

    public static async UniTask<string> RequestToGpt(MessageListBuilder messageListBuilder)
    {
        try
        {
            return await SendMessagesToGpt(
                settingsFM.ApiKey,
                messageListBuilder,
                settingsFM.SelectedGptModel,
                settingsFM.Temperature,
                settingsFM.TimeoutInSeconds
            );
        }
        catch (Exception e)
        {
            ErrorMessage(e);
            return null;
        }
    }

    //Parts of this method are inspired from AICommand
    ///<Title> AICommand GitHub Repository </Title>
    /// <Author> Keijiro Takahashi (keijiro) </Author>
    /// <Release Date> 20.03.2023 </Release Date>
    /// <Access Date> 10.09.2023 </Access Date>
    /// <Code version> N/A </Code version>
    /// <Availability> https://github.com/keijiro/AICommand/ </Availability>
    /// <Usecase> Proof-of-Concept Integration of ChatGPT into Unity Editor </Usecase>
    /// <License> Unlicense (Public Domain) </License>
    /// <Description>
    /// AICommand is a proof-of-concept integration of ChatGPT into Unity Editor. It allows controlling the Editor using natural language prompts.
    /// </Description>

    //TODO: explain why not more parameters in the bachelor add more parameters (top_p, frequency_penalty, presence_penalty,
    //stop, n, logprobs, echo, stream, best_of, logit_bias, return_prompt, return_metadata, return_sequences, expand, **kwargs)

    public static async UniTask<string> SendMessagesToGpt(
        string apiKey,
        MessageListBuilder messageListBuilder,
        string gptModel,
        float? temperature,
        int timeoutInSeconds
    )
    {
        try
        {
            var requestBody = BuildOpenApiRequest(gptModel, messageListBuilder, temperature);
            string jsonResponse = await SendGptApiRequestAsync(
                apiKey,
                GetEndPoint(gptModel),
                requestBody,
                timeoutInSeconds
            );
            string responseResult = ParseOpenApiResponse(
                jsonResponse,
                gptModel.Contains("davinci")
            );

            return responseResult;
        }
        // catch errors that occur while preparing the request
        catch (Exception e)
        {
            ErrorMessage(e);
            return null;
        }
    }

    private static string BuildOpenApiRequest(
        string gptModel,
        MessageListBuilder messageListBuilder,
        float? temperature
    )
    {
        if (gptModel.Contains("davinci"))
        {
            int messageListCount = messageListBuilder.GetMessageCount();
            var requestBody = new OpenAiInputBuilder.RequestBuilder()
                .WithModel(gptModel)
                .WithPrompt(messageListBuilder.GetMessageAt(messageListCount - 1).content)
                .WithTemperature(temperature)
                .BuildCompletionRequest();
            return requestBody;
        }
        else
        {
            var requestBody = new OpenAiInputBuilder.RequestBuilder()
                .WithModel(gptModel)
                .WithMessageListBuilder(messageListBuilder)
                .WithTemperature(temperature)
                .Build();
            return requestBody;
        }
    }

    private static async UniTask<string> SendGptApiRequestAsync(
        string apiKey,
        string endpoint,
        string requestBody,
        int timeoutInSeconds = 20
    )
    {
        //using calls the dispose method after the code block is done
        try
        {
            using var post = UnityWebRequest.Post(endpoint, requestBody, "application/json");
            // Set the timeout to the value specified in the settings

            post.timeout = timeoutInSeconds;

            post.SetRequestHeader("Authorization", "Bearer " + apiKey);

            var req = post.SendWebRequest();

            // The await keyword will yield control back to the caller while the request is being processed.
            await req;
            //TODO: Maybe add a progress bar here

            if (
                post.result == UnityWebRequest.Result.ConnectionError
                || post.result == UnityWebRequest.Result.ProtocolError
            )
            {
                string helpBoxMessage = "Error while sending async message to GPT " + post.error;
                HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
                return null;
            }

            var jsonResponse = post.downloadHandler.text;
            return jsonResponse;
        }
        // catch errors that occur while getting a response and throws them
        catch (Exception)
        {
            throw;
        }
    }

    private static void ErrorMessage(Exception e)
    {
        string helpBoxMessage = e.Message + "\nTry to set a higher timeout in the settings.";
        HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
    }

    private static string ParseOpenApiResponse(string jsonResponse, bool isCompletion = false)
    {
        if (isCompletion)
        {
            var data = JsonUtility.FromJson<OpenAiInputBuilder.CompletionResponse>(jsonResponse);
            return data.choices[0].text;
        }
        else
        {
            var data = JsonUtility.FromJson<OpenAiInputBuilder.Response>(jsonResponse);

            string responseResult = data.choices[0].message.content.Trim();
            return responseResult;
        }
    }

    private static string GetEndPoint(string gptModel)
    {
        if (gptModel.Contains("davinci"))
        {
            return completionsEndpoint;
        }
        else
        {
            return chatEndpoint;
        }
    }
}

#endif
