using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class AISettingsFileManager
{
    private static AISettingsFileManager instance;

    public static AISettingsFileManager GetInstance()
    {
        instance ??= new AISettingsFileManager();
        return instance;
    }

    private readonly string defaultSettingsFileName = "defaultSettings.json";
    private readonly string defaultUserFilesFolderPath = "Assets\\UnityEditorAI\\UserFiles\\";
    private readonly string defaultGeneratedFilesFolderPath = "Assets\\UnityEditorAI\\Generated\\";

    public enum GptModels
    {
        Gpt35Turbo,
        Gpt35Turbo16k,
        TextDavinci003,
        Gpt4,
        Default
    }

    public readonly Dictionary<GptModels, string> gptModelDictionary =
        new()
        {
            { GptModels.Gpt35Turbo, "gpt-3.5-turbo" },
            { GptModels.Gpt35Turbo16k, "gpt-3.5-turbo-16k" },
            { GptModels.TextDavinci003, "text-davinci-003" },
            { GptModels.Gpt4, "gpt-4" },
            { GptModels.Default, "gpt-3.5-turbo" }
        };

    public GptModels ParseGptModel(string modelString)
    {
        // If the input string doesn't match any dictionary value, return the default model
        if (System.Enum.TryParse(modelString, true, out GptModels model))
        {
            return model;
        }
        return GptModels.Default;
    }

    public static HelpBox helpBox = HelpBox.GetInstance();
    public string ApiKey { get; set; }
    public string UserFilesFolderPath { get; set; }
    public string SettingsFileName { get; set; }
    public string GeneratedFilesFolderPath { get; set; }
    public float Temperature { get; set; }
    public int TimeoutInSeconds { get; set; }
    public GptModels SelectedGptModel { get; set; }

    public void LoadCustomSettings(string path = null)
    {
        string helpBoxMessage;
        AISettingsSerializable settings = SettingsFileFromPath(path);

        if (settings == null)
        {
            helpBoxMessage = "No settings file found! Loading default settings.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning, false, true);
            settings = DefaultSettingsFile();
        }
        SetSettingsFromSerializable(settings);
        helpBoxMessage = "Settings loaded from file: " + UserFilesFolderPath + SettingsFileName;
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info, true);
    }

    public void SetSettingsFromSerializable(AISettingsSerializable settings, bool isDefault = false)
    {
        // Only set the API Key if the settings are not default
        if (!isDefault)
        {
            ApiKey = settings.apiKey;
        }
        string currentProjectPath = Application.dataPath;
        UserFilesFolderPath = TrimPathToAssets(settings.userFilesFolderPath, currentProjectPath);
        GeneratedFilesFolderPath = TrimPathToAssets(
            settings.generatedFilesFolderPath,
            currentProjectPath
        );
        SettingsFileName = settings.settingsFileName;
        Temperature = settings.temperature ?? 1f;
        TimeoutInSeconds = settings.timeoutInSeconds ?? 20;
        SelectedGptModel = ParseGptModel(settings.selectedGptModel);
    }

    private string TrimPathToAssets(string fullPath, string projectPath)
    {
        if (fullPath.StartsWith(projectPath))
        {
            // Remove the project path from the full path because it is not needed
            return "Assets" + fullPath[projectPath.Length..];
        }
        return fullPath;
    }

    public void LoadSettingsFromFilePanel()
    {
        string helpBoxMessage;
        if (string.IsNullOrEmpty(UserFilesFolderPath))
        {
            UserFilesFolderPath = defaultUserFilesFolderPath;
        }
        string path = EditorUtility.OpenFilePanel("Load Settings", UserFilesFolderPath, "json");
        if (!string.IsNullOrEmpty(path))
        {
            path = TrimPathToAssets(path, Application.dataPath);
            try
            {
                AISettingsSerializable panelSettings = SettingsFileFromPath(path);
                SetSettingsFromSerializable(panelSettings);
                helpBoxMessage = "Settings loaded from file: " + path;
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            }
            catch (System.Exception ex)
            {
                helpBoxMessage = "Error loading settings from file: " + ex.Message;
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            }
        }
    }

    public AISettingsSerializable DefaultSettingsFile()
    {
        AISettingsSerializable defaultSettings =
            new()
            {
                apiKey = "Please Set API Key",
                userFilesFolderPath = defaultUserFilesFolderPath,
                generatedFilesFolderPath = defaultGeneratedFilesFolderPath,
                settingsFileName = defaultSettingsFileName,
                temperature = 1f,
                timeoutInSeconds = 20,
                selectedGptModel = gptModelDictionary[GptModels.Default]
            };

        return defaultSettings;
    }

    public AISettingsSerializable SettingsFileFromPath(string path = null)
    {
        string helpBoxMessage;
        // Use the provided path or the default path if not provided
        if (string.IsNullOrEmpty(path))
        {
            //Check if the settings paths are empty. If so set default values
            if (string.IsNullOrEmpty(UserFilesFolderPath))
            {
                UserFilesFolderPath = defaultUserFilesFolderPath;
            }
            if (string.IsNullOrEmpty(SettingsFileName))
            {
                SettingsFileName = "customSettings.json";
            }
            path = UserFilesFolderPath + SettingsFileName;
        }
        else
        {
            path = TrimPathToAssets(path, Application.dataPath);
            string userDirectory = Path.GetDirectoryName(path) + "\\";
            UserFilesFolderPath = userDirectory;
            string filename = Path.GetFileName(path);
            SettingsFileName = filename;
        }

        // Check if the file exists
        if (!File.Exists(path))
        {
            helpBoxMessage = "Settings file not found at path: " + path;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return DefaultSettingsFile();
        }

        helpBoxMessage = "Loading settings from file: " + path;
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning);
        AISettingsSerializable settings =
            FileManager<AISettingsSerializable>.LoadDeserializedJsonFromPath(path);
        //check if setting is defined otherwise set default value
        settings.temperature ??= 1f;
        settings.timeoutInSeconds ??= 20;
        // Check if selectedGptModel is valid, otherwise set default value
        if (!gptModelDictionary.ContainsValue(settings.selectedGptModel))
        {
            settings.selectedGptModel = gptModelDictionary[GptModels.Default];
        }

        return settings;
    }

    public void WriteSettingsInJson(string path = null)
    {
        string helpBoxMessage;
        //  Validate API Key
        if (string.IsNullOrEmpty(ApiKey))
        {
            helpBoxMessage = "API Key cannot be empty!";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error);
            return; // Exit the function without saving if API Key is empty
        }

        // Use the provided path or the default path if not provided
        if (string.IsNullOrEmpty(path))
        {
            //Check if the settings paths are empty. If so set default values
            if (string.IsNullOrEmpty(UserFilesFolderPath))
            {
                UserFilesFolderPath = defaultUserFilesFolderPath;
            }
            if (
                string.IsNullOrEmpty(SettingsFileName)
                || SettingsFileName == defaultSettingsFileName
            )
            {
                SettingsFileName = "customSettings.json";
            }
            path = UserFilesFolderPath + SettingsFileName; //
        }

        AISettingsSerializable settings =
            new()
            {
                apiKey = ApiKey,
                userFilesFolderPath = UserFilesFolderPath,
                settingsFileName = SettingsFileName,
                generatedFilesFolderPath = GeneratedFilesFolderPath,
                temperature = Temperature,
                timeoutInSeconds = TimeoutInSeconds,
                selectedGptModel = SelectedGptModel.ToString()
            };
        FileManager<AISettingsSerializable>.SaveToJsonFileWithPath(settings, path);
        helpBoxMessage = "Settings saved to file: " + path;
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
    }
}

// Serializable AISettings class
[System.Serializable]
public class AISettingsSerializable
{
    public string apiKey;
    public string userFilesFolderPath;
    public string settingsFileName;
    public string generatedFilesFolderPath;
    public float? temperature;
    public int? timeoutInSeconds;
    public string selectedGptModel;
}
