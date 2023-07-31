using UnityEngine;
using UnityEditor;
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
    private string outputSettingsFileName = "AISettings.txt";
    private float temperature = 1f;
    private GptModels selectedGptModel = GptModels.Gpt35Turbo;

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
    public GptModels SelectedGptModel
    {
        get => selectedGptModel;
        set => selectedGptModel = value;
    }

    // Serializable AISettings class
    [System.Serializable]
    public class AISettingsSerializable
    {
        public string apiKey;
        public string outputFolderPath;
        public string outputSettingsFileName;
        public float temperature;
        public string selectedGptModel;
    }

    public void SaveAPIKey()
    {
        string outputFilePath = OutputFolderPath + outputSettingsFileName;
        File.WriteAllText(outputFilePath, ApiKey);
    }

    public void LoadAPIKeyFromFile()
    {
        string outputFilePath = OutputFolderPath + outputSettingsFileName;
        apiKey = File.Exists(outputFilePath) ? File.ReadAllText(outputFilePath) : string.Empty;
    }

    public void LoadSettingsFromJson()
    {
        string outputFilePath = OutputFolderPath + "settings.json";
        string json = File.Exists(outputFilePath) ? File.ReadAllText(outputFilePath) : string.Empty;
        AISettingsSerializable settings = JsonUtility.FromJson<AISettingsSerializable>(json);
        apiKey = settings.apiKey;
        temperature = settings.temperature;
        selectedGptModel = (GptModels)
            System.Enum.Parse(typeof(GptModels), settings.selectedGptModel);

        AISettings.UpdateHelpMessage("Settings successfully loaded!", MessageType.Info);
    }

    public void SaveSettingsInJson()
    {
        AISettingsSerializable settings = new AISettingsSerializable();
        settings.apiKey = this.apiKey;
        settings.outputFolderPath = this.outputFolderPath;
        settings.outputSettingsFileName = this.outputSettingsFileName;
        settings.temperature = this.temperature;
        settings.selectedGptModel = this.selectedGptModel.ToString();
        string json = JsonUtility.ToJson(settings);
        string outputFilePath = OutputFolderPath + "settings.json";
        File.WriteAllText(outputFilePath, json);
        AISettings.UpdateHelpMessage("Settings successfully saved!", MessageType.Info);
    }
}
