using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AISettings : SingleExtensionApplication
{
    public override string DisplayName => "AI Settings";

    private string apiKey;

    // Serializable fields
    [SerializeField]
    private string outputFolderPath = "Assets/UnityEditorAI/UserFiles/";

    public enum GptModels
    {
        Gpt35Turbo,
        Gpt35Turbo16k,
        TextDavinci003,
        Gpt4
    }

    [SerializeField]
    private GptModels selectedGptModel = GptModels.Gpt35Turbo;

    [SerializeField, Range(0f, 1f)]
    private float temperature = 1f;

    [SerializeField]
    private MessageType helpBoxMessageType = MessageType.Warning;

    [SerializeField]
    private string helpBoxMessage =
        "You will see if it worked here! Please do not use gpt-4 or davinci for now.";

    private readonly Dictionary<GptModels, string> gptModelDictionary = new Dictionary<
        GptModels,
        string
    >
    {
        { GptModels.Gpt35Turbo, "gpt-3.5-turbo" },
        { GptModels.Gpt35Turbo16k, "gpt-3.5-turbo-16k" },
        { GptModels.TextDavinci003, "text-davinci-003" },
        { GptModels.Gpt4, "gpt-4" }
    };

    private string outputFilePath;

    public override void OnEnable()
    {
        apiKey = GetAPIKeyFromFile();
    }

    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            apiKey = EditorGUILayout.TextField("OpenAI API Key", apiKey);
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Save API Key", "Saves API Key to a file")))
            {
                SaveAPIKey();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Model", EditorStyles.boldLabel);
            // Get the corresponding string value from the dictionary

            selectedGptModel = (GptModels)
                EditorGUILayout.EnumPopup("Select an option:", selectedGptModel);

            EditorGUILayout.Space();
            //we create a slider for the temperature now
            GUILayout.Label(
                new GUIContent(
                    "Temperature",
                    "The higher the temperature (0f-1f) the more random the output will be. The standard temperature is 1f."
                ),
                EditorStyles.boldLabel
            );
            temperature = EditorGUILayout.Slider(temperature, 0.0f, 1.0f);

            if (GUILayout.Button("Test API"))
            {
                TestAPI();
            }

            EditorGUILayout.HelpBox(helpBoxMessage, helpBoxMessageType);
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void SaveAPIKey()
    {
        outputFilePath = outputFolderPath + "AISettings.txt";
        File.WriteAllText(outputFilePath, apiKey);
    }

    private string GetAPIKeyFromFile()
    {
        outputFilePath = outputFolderPath + "AISettings.txt";
        return File.Exists(outputFilePath) ? File.ReadAllText(outputFilePath) : string.Empty;
    }

    private void TestAPI()
    {
        bool isGpt4OrDavinci =
            selectedGptModel == GptModels.Gpt4 || selectedGptModel == GptModels.TextDavinci003;
        bool isValidApiKey =
            OpenAiManager.TestConnection(apiKey, gptModelDictionary[selectedGptModel], temperature)
            != null;

        if (isValidApiKey && !isGpt4OrDavinci)
        {
            helpBoxMessage = "API Key is valid!";
            helpBoxMessageType = MessageType.Info;
        }
        else
        {
            helpBoxMessage =
                "API Key is not valid! Or you have used gpt-4 or davinci. Those are work in progress!";
            helpBoxMessageType = MessageType.Error;
        }
    }
}
