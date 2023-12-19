using System;
using UnityEngine;
using UnityEngine.Networking;

/// <Title> UniTask GitHub Repository </Title>
/// <Author> Yoshifumi Kawai / Cysharp, Inc. (Cysharp) </Author>
/// <Release Date> 01.11.2022 </Release Date>
/// <Access Date> 10.09.2023 </Access Date>
/// <Code version> 2.3.3 </Code version>
/// <Availability> https://github.com/Cysharp/UniTask </Availability>
/// <Usecase> Asynchronous programming </Usecase>
/// <License> Open-Source MIT License https://opensource.org/licenses/MIT </License>
/// <Description>
/// Every Task is wrapped in a UniTask. UniTask is a wrapper for Task and Task<T>.
/// It is a alternative to C# async/await for Unity. It is faster than async/await and allocates less memory.
/// It is also compatible with the Unity SynchronizationContext, so you can use it for Unity's main thread and coroutine.
/// </Description>
using Cysharp.Threading.Tasks;

using UnityEditor;

/// <summary> The response class that contains the id and choices. </summary>

public class OpenAiApiManager
{
    /// <summary> The endpoint for the chat model. </summary>

    const string chatEndpoint = "https://api.openai.com/v1/chat/completions";

    /// <summary> The endpoint for the completion model. (Davinci and Gpt Instruct) </summary>
    const string completionsEndpoint = "https://api.openai.com/v1/completions";

    /// <summary> The singleton instance of the settings file manager that contains the settings for the AI. </summary>
    private static readonly AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();

    /// <summary> Sends a single message to the GPT model and returns the response. </summary>
    /// <param name="requestMessage"> The message that should be sent to the GPT model. </param>
    /// <returns> The response of the GPT model. </returns>

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

    /// <summary> Sends a list of messages to the GPT model and returns the response. </summary>
    /// <param name="messageListBuilder">The message list builder that contains the messages that should be sent to the GPT model. </param>
    /// <returns>The response of the GPT model. </returns>
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

    /// <summary> Sends a list of messages to the GPT model and returns the response. </summary>
    /// <param name="apiKey">The API key that should be used to send the request. </param>
    /// <param name="messageListBuilder"> The message list builder that contains the messages that should be sent to the GPT model. </param>
    /// <param name="gptModel"> The GPT model that should be used to send the request. </param>
    /// <param name="temperature"> The temperature that should be used to send the request. </param>
    /// <param name="timeoutInSeconds"> The timeout in seconds that should be used to send the request. </param>
    /// <returns> The response of the GPT model. </returns>
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
            HelpBox.GetInstance().UpdateIntendedProgress(0.4f);
            var requestBody = BuildOpenApiRequest(gptModel, messageListBuilder, temperature);
            string jsonResponse = await SendGptApiRequestAsync(
                apiKey,
                GetEndPoint(gptModel),
                requestBody,
                timeoutInSeconds
            );
            HelpBox.GetInstance().UpdateIntendedProgress(0.5f);

            string responseResult = ParseOpenApiResponse(
                jsonResponse,
                gptModel.Contains("davinci") || gptModel.Contains("instruct")
            );
            return responseResult;
        }
        catch (Exception e)
        {
            ErrorMessage(e);
            return null;
        }
    }

    /// <summary> Builds the request body for the OpenAI API </summary>
    /// <param name="gptModel"> The GPT model that should be used to send the request. </param>
    /// <param name="messageListBuilder"> The message list builder that contains the messages that should be sent to the GPT model. </param>
    /// <param name="temperature"></param>
    /// <returns> The request body as a string for the OpenAI API. </returns>
    private static string BuildOpenApiRequest(
        string gptModel,
        MessageListBuilder messageListBuilder,
        float? temperature
    )
    {
        if (gptModel.Contains("davinci") || gptModel.Contains("instruct"))
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

    /// <summary> Sends a request to the OpenAI API and returns the response. </summary>
    /// <param name="apiKey"> The API key that should be used to send the request. </param>
    /// <param name="endpoint"> The endpoint that should be used to send the request. </param>
    private static async UniTask<string> SendGptApiRequestAsync(
        string apiKey,
        string endpoint,
        string requestBody,
        int timeoutInSeconds = 20
    )
    {
        try
        {
            using var post = UnityWebRequest.Post(endpoint, requestBody, "application/json");
            post.timeout = timeoutInSeconds;
            post.SetRequestHeader("Authorization", "Bearer " + apiKey);
            var req = post.SendWebRequest();
            await req;
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
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Updates the help box with an error message. Recommends a higher timeout.
    /// </summary>
    /// <param name="e"> The exception that should be used to update the help box. </param>
    private static void ErrorMessage(Exception e)
    {
        string helpBoxMessage = e.Message + "\nTry to set a higher timeout in the settings.";
        HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
    }

    /// <summary> Parses the response of the OpenAI API and returns the response as a string.</summary>
    /// <param name="jsonResponse"> The response of the OpenAI API as a string. </param>
    /// <param name="isCompletion"> Whether the response is a completion response or not. </param>
    /// <returns> The response of the OpenAI API as a string. </returns>
    private static string ParseOpenApiResponse(string jsonResponse, bool isCompletion = false)
    {
        if (isCompletion)
        {
            var data = JsonUtility.FromJson<GptCompletionResponse>(jsonResponse);
            return data.choices[0].text;
        }
        else
        {
            var data = JsonUtility.FromJson<GptResponse>(jsonResponse);
            string responseResult = data.choices[0].message.content.Trim();
            return responseResult;
        }
    }

    /// <summary>
    /// Returns the endpoint for the GPT model.
    /// </summary>
    /// <param name="gptModel"> The GPT model that should be used to send the request. </param>
    /// <returns> The endpoint for the GPT model. </returns>
    private static string GetEndPoint(string gptModel)
    {
        if (gptModel.Contains("davinci") || gptModel.Contains("instruct"))
        {
            return completionsEndpoint;
        }
        else
        {
            return chatEndpoint;
        }
    }
}
