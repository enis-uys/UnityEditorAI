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
    const string endpoint = "https://api.openai.com/v1/chat/completions";

    private static readonly AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();

    private static readonly string settingsDefaultOpenAiModel = settingsFM.gptModelDictionary[
        AISettingsFileManager.GptModels.Default
    ];

    public static async UniTask<string> TestConnection()
    {
        try
        {
            return await SendMessageToGpt(
                settingsFM.ApiKey,
                "Hello World!",
                settingsFM.gptModelDictionary[AISettingsFileManager.GptModels.Default],
                settingsFM.Temperature,
                settingsFM.TimeoutInSeconds
            );
        }
        catch (Exception e)
        {
            ErrorMessage(e);
            Debug.LogError("Error while testing connection: " + e.Message);

            return null;
        }
        //TODO: some settings helpBox logic here later
    }

    public static async UniTask<string> InputToGptCreateScript(string inputPrompt)
    {
        try
        {
            string gptInput = OpenAiStandardPrompts.CreateNewScriptWithPrompt(inputPrompt);

            return await SendMessageToGpt(
                settingsFM.ApiKey,
                gptInput,
                settingsFM.gptModelDictionary[AISettingsFileManager.GptModels.Default],
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

    public static async UniTask<string> InputScriptToGptCreateScript(
        string inputPrompt,
        string inputScriptString
    )
    {
        try
        {
            string gptInput = OpenAiStandardPrompts.UpdateExistingScriptWithPrompt(
                inputPrompt,
                inputScriptString
            );
            return await SendMessageToGpt(
                settingsFM.ApiKey,
                gptInput,
                settingsFM.gptModelDictionary[AISettingsFileManager.GptModels.Default],
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

    public static async UniTask<string> ChatToGpt(string requestMessage)
    {
        try
        {
            return await SendMessageToGpt(
                settingsFM.ApiKey,
                requestMessage,
                settingsFM.gptModelDictionary[AISettingsFileManager.GptModels.Default],
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

    //parts of this method are from github AICommand
    //https://github.com/keijiro/AICommand
    //used an optional parameter for the model
    //TODO: add more parameters (top_p, frequency_penalty, presence_penalty,
    //stop, n, logprobs, echo, stream, best_of, logit_bias, return_prompt, return_metadata, return_sequences, expand, **kwargs)
    public static async UniTask<string> SendMessageToGpt(
        string apiKey,
        string requestMessage,
        string gptModel,
        float? temperature,
        int timeoutInSeconds
    )
    {
        try
        {
            gptModel = CheckGptModel(gptModel);

            var requestBody = BuildOpenApiRequest(gptModel, requestMessage, temperature);

            string jsonResponse = await SendGptApiRequestAsync(
                apiKey,
                requestBody,
                timeoutInSeconds
            );
            string responseResult = ParseOpenApiResponse(jsonResponse);

            return responseResult;
        }
        // catch errors that occur while preparing the request
        catch (Exception e)
        {
            ErrorMessage(e);
            return null;
        }
    }

    private static async UniTask<string> SendGptApiRequestAsync(
        string apiKey,
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
            // Maybe add a progress bar here also change the while loop (not good practice)

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

    private static string BuildOpenApiRequest(
        string gptModel,
        string requestMessage,
        float? temperature
    )
    {
        //you have to change the endpoints depending on the model used
        // remove the options like temperature if not set
        var requestBody = new OpenAiInputBuilder.RequestBuilder()
            .WithModel(gptModel)
            .AddMessage("user", requestMessage)
            .WithTemperature(temperature)
            .Build();
        return requestBody;
    }

    private static string CheckGptModel(string gptModel)
    {
        if (
            gptModel != "gpt-3.5-turbo"
            && gptModel != "gpt35turbo16k"
            && gptModel != "text-davinci-003"
            && gptModel != "gpt-4"
        )
        {
            return settingsDefaultOpenAiModel;
        }
        else
        {
            return gptModel;
        }
    }

    private static string ParseOpenApiResponse(string jsonResponse)
    {
        var data = JsonUtility.FromJson<OpenAiInputBuilder.Response>(jsonResponse);
        string responseResult = data.choices[0].message.content;

        return responseResult;
    }
}

#endif
