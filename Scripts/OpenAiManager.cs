using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;

public class OpenAiManager
{
    const string endpoint = "https://api.openai.com/v1/chat/completions";

    //later include the api Key here and read it from here
    private static string settingsOpenAiApiKey =
        "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";

    private static string settingsOpenAiModel = "gpt-3.5-turbo";
    private static float settingsOpenAiTemperature = 1f;

    public static string TestConnection(
        string apiKey,
        string gptModel = null,
        float temperature = 1f
    )
    {
        return SendMessageToGpt(
            settingsOpenAiApiKey,
            "Hello World!",
            settingsOpenAiModel,
            settingsOpenAiTemperature
        );
        // do some settings helpBox logic here later
    }

    public static string InputToGptCreateScript(string inputPrompt)
    {
        string gptInput = OpenAiStandardPrompts.CreateNewBaseScriptPrompt(inputPrompt);
        return SendMessageToGpt(
            settingsOpenAiApiKey,
            gptInput,
            settingsOpenAiModel,
            settingsOpenAiTemperature
        );
    }

    public static string ChatToGpt(string requestMessage)
    {
        return SendMessageToGpt(
            settingsOpenAiApiKey,
            requestMessage,
            settingsOpenAiModel,
            settingsOpenAiTemperature
        );
    }

    //parts of this method are from github AICommand
    //used an optional parameter for the model
    //TODO: add more parameters (max_tokens, top_p, frequency_penalty, presence_penalty,
    //stop, n, logprobs, echo, stream, best_of, logit_bias, return_prompt, return_metadata, return_sequences, expand, **kwargs)
    public static string SendMessageToGpt(
        string apiKey,
        string requestMessage,
        string gptModel,
        float temperature // default value
    )
    {
        try
        {
            // gptModel = GetModelFromFile();
            gptModel = CheckGptModel(gptModel);

            var requestBody = BuildOpenApiRequest(gptModel, requestMessage, temperature);

            var jsonResponse = SendOpenApiRequest(apiKey, requestBody);

            string responseResult = ParseOpenApiResponse(jsonResponse);

            return responseResult;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private string GetAPIKeyFromFile()
    {
        //later implementation
        return settingsOpenAiApiKey;
    }

    private string GetModelFromFile()
    {
        //later implementation
        return settingsOpenAiModel;
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
        float temperature
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

    private static string SendOpenApiRequest(string apiKey, string requestBody)
    {
        //using calls the dispose method after the code block is done

        using (var post = UnityWebRequest.Post(endpoint, requestBody, "application/json"))
        {
            post.timeout = 20;
            post.SetRequestHeader("Authorization", "Bearer " + apiKey);
            var req = post.SendWebRequest();
            // Maybe add a progress bar here also change the while loop (not good practice)

            while (!req.isDone)
            {
                System.Threading.Thread.Sleep(100);
            }
            var jsonResponse = post.downloadHandler.text;
            return jsonResponse;
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
