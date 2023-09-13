using System;

using System.Threading.Tasks;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;

// Adapted from UniTask library by Cysharp
// GitHub Repository: https://github.com/Cysharp/UniTask
// Used for asynchronous programming.
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;

public class OpenAiManager
{
    const string endpoint = "https://api.openai.com/v1/chat/completions";

    private static AISettingsFileManager settingsFM = new AISettingsFileManager();

    private static string settingsOpenAiModel = "gpt-3.5-turbo";

    public static async Task<string> TestConnection(
        string apiKey,
        string gptModel = null,
        float? temperature = 1f,
        int? maxTokens = 4,
        int? timeoutInSeconds = 15
    )
    {
        AISettingsSerializable settings = settingsFM.LoadAndConvertSettingsFromFile();
        return await SendMessageToGpt(
            settings.apiKey,
            "Hello World!",
            settings.selectedGptModel,
            settings.temperature.Value,
            settings.maxTokens.Value,
            settings.timeoutInSeconds.Value
        );
        // do some settings helpBox logic here later
    }

    public static async Task<string> InputToGptCreateScript(string inputPrompt)
    {
        //TODO: make sure the output is a runnable script by removing ```

        string gptInput = OpenAiStandardPrompts.CreateNewScriptWithPrompt(inputPrompt);
        AISettingsSerializable settings = settingsFM.LoadAndConvertSettingsFromFile();

        return await SendMessageToGpt(
            settings.apiKey,
            gptInput,
            settings.selectedGptModel,
            settings.temperature.Value,
            settings.maxTokens.Value,
            settings.timeoutInSeconds.Value
        );
    }

    public static async Task<string> InputScriptToGptCreateScript(
        string inputPrompt,
        string inputScriptString
    )
    {
        //TODO: make sure the output is a runnable script by removing ```

        string gptInput = OpenAiStandardPrompts.UpdateExistingScriptWithPrompt(
            inputPrompt,
            inputScriptString
        );
        AISettingsSerializable settings = settingsFM.LoadAndConvertSettingsFromFile();
        return await SendMessageToGpt(
            settings.apiKey,
            gptInput,
            settings.selectedGptModel,
            settings.temperature.Value,
            settings.maxTokens.Value,
            settings.timeoutInSeconds.Value
        );
    }

    public static async Task<string> ChatToGpt(string requestMessage)
    {
        AISettingsSerializable settings = settingsFM.LoadAndConvertSettingsFromFile();
        return await SendMessageToGpt(
            settings.apiKey,
            requestMessage,
            settings.selectedGptModel,
            settings.temperature.Value,
            settings.maxTokens.Value,
            settings.timeoutInSeconds.Value
        );
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
            return settingsOpenAiModel;
        }
        else
        {
            return gptModel;
        }
    }

    private static string BuildOpenApiRequest(
        string gptModel,
        string requestMessage,
        float temperature,
        int maxTokens
    )
    {
        //you have to change the endpoints depending on the model used
        // remove the options like temperature if not set
        var requestBody = new OpenAiInputBuilder.RequestBuilder()
            .WithModel(gptModel)
            .AddMessage("user", requestMessage)
            .WithTemperature(temperature)
            .WithMaxTokens(maxTokens)
            .Build();

        return requestBody;
    }

    //parts of this method are from github AICommand
    //https://github.com/keijiro/AICommand
    //used an optional parameter for the model
    //TODO: add more parameters (max_tokens, top_p, frequency_penalty, presence_penalty,
    //stop, n, logprobs, echo, stream, best_of, logit_bias, return_prompt, return_metadata, return_sequences, expand, **kwargs)
    public static async Task<string> SendMessageToGpt(
        string apiKey,
        string requestMessage,
        string gptModel,
        float temperature,
        int maxTokens,
        int timeoutInSeconds
    )
    {
        try
        {
            // gptModel = GetModelFromFile();
            gptModel = CheckGptModel(gptModel);

            var requestBody = BuildOpenApiRequest(gptModel, requestMessage, temperature, maxTokens);

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
            HelpBox
                .GetInstance()
                .UpdateMessageAndType(
                    "Error while sending message to GPT: "
                        + e.Message
                        + "\nSet a higher timeout in the settings.",
                    MessageType.Error
                );
            return null;
        }
    }

    private static async Task<string> SendGptApiRequestAsync(
        string apiKey,
        string requestBody,
        int timeoutInSeconds = 20
    )
    {
        //using calls the dispose method after the code block is done
        try
        {
            using (var post = UnityWebRequest.Post(endpoint, requestBody, "application/json"))
            {
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
                    HelpBox
                        .GetInstance()
                        .UpdateMessageAndType("Error: " + post.error, MessageType.Error);
                    return null;
                }

                var jsonResponse = post.downloadHandler.text;
                return jsonResponse;
            }
        }
        // catch errors that occur while getting a response
        catch (Exception e)
        {
            HelpBox
                .GetInstance()
                .UpdateMessageAndType(
                    "Error while sending message to GPT: " + e.Message,
                    MessageType.Error
                );
            return null;
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
