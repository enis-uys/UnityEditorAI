using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class AISettingsFileManager
{
    public AISettingsFileManager() { }

    public enum GptModels
    {
        Gpt35Turbo,
        Gpt35Turbo16k,
        TextDavinci003,
        Gpt4
    }

    public readonly Dictionary<GptModels, string> gptModelDictionary = new Dictionary<
        GptModels,
        string
    >
    {
        { GptModels.Gpt35Turbo, "gpt-3.5-turbo" },
        { GptModels.Gpt35Turbo16k, "gpt-3.5-turbo-16k" },
        { GptModels.TextDavinci003, "text-davinci-003" },
        { GptModels.Gpt4, "gpt-4" }
    };

    private string apiKey;
    private string outputFolderPath = "Assets/UnityEditorAI/UserFiles/";
    private string outputSettingsFileName = "settings";
    private float temperature;
    private int maxTokens;
    private GptModels selectedGptModel = GptModels.Gpt35Turbo;
    private int timeoutInSeconds;
    HelpBox helpBox = HelpBox.GetInstance();

    public string ApiKey
    {
        get => apiKey;
        set => apiKey = value;
    }
    public string OutputFolderPath
    {
        get => outputFolderPath;
        set => outputFolderPath = value;
    }
    public string OutputSettingsFileName
    {
        get => outputSettingsFileName;
        set => outputSettingsFileName = value;
    }
    public float Temperature
    {
        get => temperature;
        set => temperature = value;
    }
    public int MaxTokens
    {
        get => maxTokens;
        set => maxTokens = value;
    }
    public int TimeoutInSeconds
    {
        get => timeoutInSeconds;
        set => timeoutInSeconds = value;
    }
    public GptModels SelectedGptModel
    {
        get => selectedGptModel;
        set => selectedGptModel = value;
    }

    public void SaveSettingsInJson()
    {
        AISettingsSerializable settings = new AISettingsSerializable();
        settings.apiKey = this.apiKey;
        settings.outputFolderPath = this.outputFolderPath;
        settings.outputSettingsFileName = this.outputSettingsFileName;
        settings.temperature = this.temperature;
        settings.maxTokens = this.maxTokens;
        settings.timeoutInSeconds = this.timeoutInSeconds;
        settings.selectedGptModel = this.selectedGptModel.ToString();
        FileManager<AISettingsSerializable>.SaveToJsonFileWithPath(
            settings,
            outputFolderPath,
            outputSettingsFileName
        );
        helpBox.UpdateHelpBoxMessageAndType("Settings successfully saved!", MessageType.Info);
    }

    public void LoadCustomSettings()
    {
        AISettingsSerializable settings = LoadAndConvertSettingsFromFile();
        if (settings == null)
        {
            helpBox.UpdateHelpBoxMessageAndType("No settings file found!", MessageType.Error);
            return;
        }
        apiKey = settings.apiKey;
        temperature = settings.temperature.Value;
        maxTokens = settings.maxTokens.Value;
        timeoutInSeconds = settings.timeoutInSeconds.Value;
        selectedGptModel = (GptModels)
            System.Enum.Parse(typeof(GptModels), settings.selectedGptModel);
        helpBox.UpdateHelpBoxMessageAndType("Settings successfully loaded!", MessageType.Info);
    }

    public void LoadSettingsFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Load Settings", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                Debug.Log(json);
                //LoadAndConvertSettingsFromFile(json);
            }
            catch (Exception ex)
            {
                helpBox.UpdateHelpBoxMessageAndType(
                    "Error loading settings from file: " + ex.Message,
                    MessageType.Error
                );
                Debug.LogError("Error loading settings from file: " + ex.Message);
            }
        }
        helpBox.UpdateHelpBoxMessageAndType("Function!", MessageType.Info);
    }

    public AISettingsSerializable LoadAndConvertSettingsFromFile()
    {
        //uses FileManager to load settings from json file
        AISettingsSerializable settings =
            FileManager<AISettingsSerializable>.LoadDeserializedJsonFromPath<AISettingsSerializable>(
                outputFolderPath,
                outputSettingsFileName
            );
        //check if setting is defined otherwise set default value
        if (!settings.temperature.HasValue)
        {
            settings.temperature = 1f;
        }
        if (!settings.maxTokens.HasValue)
        {
            settings.maxTokens = 4096;
        }
        if (!settings.timeoutInSeconds.HasValue)
        {
            settings.timeoutInSeconds = 20;
        }
        return settings;
    }

    public string LoadAPIKeyFromFile()
    {
        AISettingsSerializable settings = LoadAndConvertSettingsFromFile();
        return settings.apiKey;
    }

    public float LoadTemperatureFromFile()
    {
        AISettingsSerializable settings = LoadAndConvertSettingsFromFile();
        return settings.temperature.Value;
    }

    public int LoadMaxTokensFromFile()
    {
        AISettingsSerializable settings = LoadAndConvertSettingsFromFile();
        return settings.maxTokens.Value;
    }

    public int LoadTimeoutInSecondsFromFile()
    {
        AISettingsSerializable settings = LoadAndConvertSettingsFromFile();
        return settings.timeoutInSeconds.Value;
    }

    public string LoadSelectedGptModelAsStringFromFile()
    {
        AISettingsSerializable settings = LoadAndConvertSettingsFromFile();
        return settings.selectedGptModel;
    }

    public GptModels LoadSelectedGptModelAsEnumFromFile()
    {
        string gptModel = LoadSelectedGptModelAsStringFromFile();
        return (GptModels)System.Enum.Parse(typeof(GptModels), gptModel);
    }
}

// Serializable AISettings class
[System.Serializable]
public class AISettingsSerializable
{
    public string apiKey;
    public string outputFolderPath;
    public string outputSettingsFileName;

    [SerializeField]
    public float? temperature;

    [SerializeField]
    public int? maxTokens;
    public int? timeoutInSeconds;
    public string selectedGptModel;
}
