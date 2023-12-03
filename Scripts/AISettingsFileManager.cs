using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary> The file manager for the AI settings </summary>
public class AISettingsFileManager
{
    /// <summary> The singleton instance of the file manager. </summary>
    private static AISettingsFileManager instance;

    /// <summary> Gets the singleton instance of the file manager. </summary>
    /// <returns> Returns the instance of the file manager. </returns>
    public static AISettingsFileManager GetInstance()
    {
        instance ??= new AISettingsFileManager();
        return instance;
    }

    /// <summary> The default settings file name. </summary>
    private readonly string defaultSettingsFileName = "defaultSettings.json";

    /// <summary> The default user files folder path. </summary>
    private readonly string defaultUserFilesFolderPath = "Assets\\UnityEditorAI\\UserFiles\\";

    /// <summary> The default generated files folder path. </summary>
    private readonly string defaultGeneratedFilesFolderPath = "Assets\\UnityEditorAI\\Generated\\";

    /// <summary> The ai models available. </summary>
    public static readonly string Gpt35Turbo = "gpt-3.5-turbo",
        Gpt35Turbo16k = "gpt-3.5-turbo-16k",
        TextDavinci003 = "text-davinci-003",
        Gpt4 = "gpt-4",
        GptDefault = Gpt35Turbo;

    /// <summary> The list of ai models available. </summary>

    public static readonly List<string> gptModels =
        new() { Gpt35Turbo, Gpt35Turbo16k, TextDavinci003, Gpt4 };

    /// <summary> The list of ai models available as an array (for the dropdown menu) </summary>
    public string[] gptModelsArray = gptModels.ToArray();

    /// <summary> The index of the selected ai model. </summary>
    /// <returns> Returns the index of the selected ai model.</returns>
    public int SelectedGptModelInt()
    {
        for (int i = 0; i < gptModels.Count; i++)
        {
            if (gptModels[i] == SelectedGptModel)
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary> Access to the help box. </summary>
    public static HelpBox helpBox = HelpBox.GetInstance();

    /// <summary> The API Key to use for the AI. </summary>
    public string ApiKey { get; set; }

    /// <summary> The path to the user files folder. </summary>
    public string UserFilesFolderPath { get; set; }

    /// <summary> The name of the settings file. </summary>
    public string SettingsFileName { get; set; }

    /// <summary> The path to the generated files folder. </summary>
    public string GeneratedFilesFolderPath { get; set; }

    /// <summary> The number of messages to send to the AI. </summary>
    public int LastMessagesToSend { get; set; }

    /// <summary> Temperature of the AI </summary>
    public float Temperature { get; set; }

    /// <summary> Timeout in seconds for the AI after which it will stop the request </summary>
    public int TimeoutInSeconds { get; set; }

    /// <summary> The selected GPT model </summary>
    public string SelectedGptModel { get; set; }

    /// <summary> Loads the settings from a file. </summary>
    /// <param name="path"> The path to the settings file. If null, the default path is used. </param>
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
    }

    /// <summary> Sets the settings from a serializable object. </summary>
    /// <param name="settings"> The serializable settings object to set the settings from. </param>
    /// <param name="isDefault"> Whether the settings are default settings. If true, the API Key is not set, so the api key is not overwritten. </param>
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
        LastMessagesToSend = settings.lastMessagesToSend ?? 2;
        Temperature = settings.temperature ?? 1f;
        TimeoutInSeconds = settings.timeoutInSeconds ?? 20;
        SelectedGptModel = settings.selectedGptModel;
    }

    /// <summary> Trims the path to the assets folder, so there is no long path shown in the inspector. </summary>
    /// <param name="fullPath"> The full path to trim. </param>
    /// <param name="projectPath"> The path to the project. </param>
    /// <returns> returns the trimmed path that starts with "Assets". </returns>
    private string TrimPathToAssets(string fullPath, string projectPath)
    {
        if (fullPath.StartsWith(projectPath))
        {
            // Remove the project path from the full path because it is not needed
            return "Assets" + fullPath[projectPath.Length..];
        }
        return fullPath;
    }

    /// <summary> Saves the settings from file panel. </summary>
    public void LoadSettingsFromFilePanel()
    {
        string helpBoxMessage;
        if (string.IsNullOrEmpty(UserFilesFolderPath))
        {
            UserFilesFolderPath = defaultUserFilesFolderPath;
        }
        string panelPath = EditorUtility.OpenFilePanel(
            "Load Settings",
            UserFilesFolderPath,
            "json"
        );
        if (!string.IsNullOrEmpty(panelPath))
        {
            panelPath = TrimPathToAssets(panelPath, Application.dataPath);
            try
            {
                AISettingsSerializable panelSettings = SettingsFileFromPath(panelPath);
                SetSettingsFromSerializable(panelSettings);
            }
            catch (Newtonsoft.Json.JsonException jsonEx)
            {
                helpBoxMessage = "JSON data does not match expected type." + "\n" + jsonEx.Message;
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            }
            catch (System.Exception ex)
            {
                helpBoxMessage = "Error loading settings from file: " + ex.Message;
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            }
        }
    }

    /// <summary> Saves the settings to file panel. </summary>
    /// <returns> Returns the path to the settings file. </returns>
    public AISettingsSerializable DefaultSettingsFile()
    {
        AISettingsSerializable defaultSettings =
            new()
            {
                apiKey = "Please Set API Key",
                userFilesFolderPath = defaultUserFilesFolderPath,
                generatedFilesFolderPath = defaultGeneratedFilesFolderPath,
                settingsFileName = defaultSettingsFileName,
                lastMessagesToSend = 2,
                temperature = 1f,
                timeoutInSeconds = 20,
                selectedGptModel = GptDefault
            };

        return defaultSettings;
    }

    /// <summary> Loads the settings from a file. </summary>
    /// <param name="settingsPath"> The path to the settings file. If null, the default path is used. </param>
    /// <returns> Returns the settings from the file.</returns>
    public AISettingsSerializable SettingsFileFromPath(string settingsPath = null)
    {
        string helpBoxMessage;
        // Use the provided path or the default path if not provided
        if (string.IsNullOrEmpty(settingsPath))
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
            settingsPath = UserFilesFolderPath + SettingsFileName;
        }
        else
        {
            settingsPath = TrimPathToAssets(settingsPath, Application.dataPath);
        }

        // Check if the file exists
        if (!File.Exists(settingsPath))
        {
            helpBoxMessage = "Settings file not found at path: " + settingsPath;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, false);
            return DefaultSettingsFile();
        }

        helpBoxMessage = "Loading settings from file: " + settingsPath;
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning);
        AISettingsSerializable settings =
            FileManager<AISettingsSerializable>.LoadDeserializedJsonFromPath(settingsPath);
        //check if setting is defined otherwise set default value
        settings.lastMessagesToSend ??= 2;
        settings.temperature ??= 1f;
        settings.timeoutInSeconds ??= 20;
        // Check if selectedGptModel is valid, otherwise set default value

        settings.selectedGptModel = gptModels.Contains(settings.selectedGptModel)
            ? settings.selectedGptModel
            : GptDefault;
        //After loading the settings file, set the user files folder path and settings file name
        string userDirectory = Path.GetDirectoryName(settingsPath) + "\\";
        UserFilesFolderPath = userDirectory;
        string filename = Path.GetFileName(settingsPath);
        SettingsFileName = filename;
        return settings;
    }

    /// <summary> Writes the settings to a json file. </summary>
    /// <param name="path"> The path to the settings file. If null, the default path is used. </param>
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
                lastMessagesToSend = LastMessagesToSend,
                temperature = Temperature,
                timeoutInSeconds = TimeoutInSeconds,
                selectedGptModel = SelectedGptModel
            };
        FileManager<AISettingsSerializable>.SaveToJsonFileWithPath(settings, path);
        helpBoxMessage = "Settings saved to file: " + path;
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
    }
}

/// <summary> Serializable class for the AI settings. </summary>
[System.Serializable]
public class AISettingsSerializable
{
    public string apiKey;
    public string userFilesFolderPath;
    public string settingsFileName;
    public string generatedFilesFolderPath;
    public int? lastMessagesToSend;
    public float? temperature;
    public int? timeoutInSeconds;
    public string selectedGptModel;
}
