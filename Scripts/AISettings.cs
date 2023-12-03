using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

/// <summary> The AI Settings application </summary>
public class AISettings : SingleExtensionApplication
{
    /// <summary> The display name of a single application. </summary>
    public override string DisplayName => "AI Settings";

    /// <summary> A FileManager for the AI Settings that reads and writes the settings to a json file. </summary>
    private static readonly AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();
    private bool HasInit { get; set; } = false;

    /// <summary> Whether the EditorPrefs should be loaded. </summary>
    private bool ShouldLoadCustomSettings { get; set; } = true;

    /// TODO: Change to store a settings json in the editor prefs and load from there
    /// <summary>
    /// Loads the custom settings from the the settings file manager.
    /// Important to only set this to OnEnable otherwise settings might not be loaded before the other applications are initialized.
    /// </summary>
    public override void OnEnable()
    {
        if (ShouldLoadCustomSettings)
        {
            settingsFM.LoadCustomSettings();
            ShouldLoadCustomSettings = false;
        }
    }

    /// <summary> Renders the GUI of the application. </summary>
    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            if (!HasInit)
            {
                LoadEditorPrefs();
                HasInit = true;
            }
            RenderApiKeyField();
            AddDefaultSpace();

            RenderUserFilesPathField();
            AddDefaultSpace();

            RenderModelSelectionField();
            AddDefaultSpace();

            RenderLastMessagesSlider();
            AddDefaultSpace();

            RenderTemperatureSlider();
            AddDefaultSpace();

            RenderTimeoutInSecondsSlider();
            AddDefaultSpace();

            RenderActionButtons();
            AddDefaultSpace();
            RenderHelpBox();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary> Renders the ApiKey field. </summary>
    private void RenderApiKeyField()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            new GUIContent(
                "API Key:",
                "The API Key for the OpenAI API. You can get an API Key from the OpenAI website."
            ),
            EditorStyles.boldLabel,
            GUILayout.ExpandWidth(false)
        );
        settingsFM.ApiKey = EditorGUILayout.TextField(
            settingsFM.ApiKey,
            GUILayout.ExpandWidth(true)
        );
        EditorGUILayout.EndHorizontal();
    }

    /// <summary> Renders the User Files Path field. </summary>
    private void RenderUserFilesPathField()
    {
        GUILayout.Label(
            new GUIContent(
                "User Settings File Path",
                "The path where the user settings will be saved. The default path is Assets/UnityEditorAI/UserFiles/"
            ),
            EditorStyles.boldLabel
        );
        if (GUILayout.Button(settingsFM.UserFilesFolderPath))
        {
            string selectedSettingsFolderPath = EditorUtility.OpenFolderPanel(
                "Select Settings Folder Path",
                settingsFM.UserFilesFolderPath,
                ""
            );
            if (!string.IsNullOrEmpty(selectedSettingsFolderPath))
            {
                UpdateFolderPath(selectedSettingsFolderPath);
            }
        }
        AddDefaultSpace();
        GUILayout.Label(
            new GUIContent(
                "File Path for Generated Elements",
                "The path where the generated elements will be saved. The default path is Assets/UnityEditorAI/Generated/"
            ),
            EditorStyles.boldLabel
        );

        if (GUILayout.Button(settingsFM.GeneratedFilesFolderPath))
        {
            string selectedGenerateFolderPath = EditorUtility.OpenFolderPanel(
                "Select Generate Folder Path",
                settingsFM.GeneratedFilesFolderPath,
                ""
            );
            if (!string.IsNullOrEmpty(selectedGenerateFolderPath))
            {
                UpdateFolderPath(selectedGenerateFolderPath, true);
            }
        }
    }

    /// <summary> Updates the folder path inside the settings file manager. </summary>
    /// <param name="selectedFolderPath"> The selected folder path. </param>
    /// <param name="isGenerateFolderPath">
    /// The selected folder path is for the generated files folder.
    /// Has an default value of false. If true then instead the generated files folder path will be updated.
    /// </param>
    private void UpdateFolderPath(string selectedFolderPath, bool isGenerateFolderPath = false)
    {
        int assetsIndex = selectedFolderPath.IndexOf("Assets");

        if (assetsIndex != -1)
        {
            if (isGenerateFolderPath)
            {
                settingsFM.GeneratedFilesFolderPath = selectedFolderPath[assetsIndex..] + "\\";
            }
            else
            {
                settingsFM.UserFilesFolderPath = selectedFolderPath[assetsIndex..] + "\\";
            }
        }
        else
        {
            if (isGenerateFolderPath)
            {
                settingsFM.GeneratedFilesFolderPath = selectedFolderPath + "\\";
            }
            else
            {
                settingsFM.UserFilesFolderPath = selectedFolderPath + "\\";
            }
            string helpBoxMessage =
                "The selected folder is not inside the project's Assets Folder. It is recommended to choose a path inside the project's Assets Folder.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning, false, true);
        }
    }

    /// <summary> Renders the Model Selection field. </summary>
    private void RenderModelSelectionField()
    {
        GUILayout.Label(
            new GUIContent("Model:", "Select a model to use for the AI."),
            EditorStyles.boldLabel
        );
        // Get the corresponding string value from the dictionary
        int selectedGptModelInt = EditorGUILayout.Popup(
            "Select an option:",
            settingsFM.SelectedGptModelInt(),
            settingsFM.gptModelsArray
        );
        // Get the corresponding key from the dictionary
        settingsFM.SelectedGptModel = settingsFM.gptModelsArray[selectedGptModelInt];
    }

    /// <summary> Renders the Last Messages Slider. </summary>
    private void RenderLastMessagesSlider()
    {
        GUILayout.Label(
            new GUIContent(
                "Last Messages To Send",
                "Set how many last messages should be send with a message in the chat. The default is 2."
            ),
            EditorStyles.boldLabel
        );

        settingsFM.LastMessagesToSend = EditorGUILayout.IntSlider(
            settingsFM.LastMessagesToSend,
            0,
            5
        );
    }

    /// <summary> Renders the Temperature Slider. </summary>
    private void RenderTemperatureSlider()
    {
        GUILayout.Label(
            new GUIContent(
                "Temperature",
                "The higher the temperature (0f-1f) the more random the output will be. The standard temperature is 1f."
            ),
            EditorStyles.boldLabel
        );

        settingsFM.Temperature = EditorGUILayout.Slider(settingsFM.Temperature, 0.0f, 1.0f);
    }

    /// <summary> Renders the Timeout in Seconds Slider. </summary>
    private void RenderTimeoutInSecondsSlider()
    {
        GUILayout.Label(
            new GUIContent(
                "Timeout in Seconds",
                "The maximum number of seconds to wait for a response from the server. The default is 20 seconds."
            ),
            EditorStyles.boldLabel
        );
        settingsFM.TimeoutInSeconds = EditorGUILayout.IntSlider(settingsFM.TimeoutInSeconds, 5, 60);
    }

    /// <summary> Renders the Action Buttons. </summary>
    private void RenderActionButtons()
    {
        GUILayout.BeginHorizontal();
        try
        {
            if (GUILayout.Button("Test API"))
            {
                TestAPI();
            }
            if (GUILayout.Button("Reset To Default Settings"))
            {
                settingsFM.SetSettingsFromSerializable(settingsFM.DefaultSettingsFile(), true);
            }
            if (GUILayout.Button("Load Settings From File"))
            {
                settingsFM.LoadSettingsFromFilePanel();
            }
            if (
                GUILayout.Button(
                    new GUIContent("Save Settings", "Saves the settings to a json file")
                )
            )
            {
                settingsFM.WriteSettingsInJson();
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    /// <summary> Tests the API Key by sending a request with Hello World! as the prompt. </summary>
    private async void TestAPI()
    {
        string helpBoxMessage;

        bool isValidApiKey = await OpenAiApiManager.RequestToGpt("Hello World!") != null;
        if (isValidApiKey)
        {
            helpBoxMessage = "API Key is valid!";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
        }
        else
        {
            helpBoxMessage = "API Key is not valid!";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error);
        }
    }

    public enum EditorPrefKey
    {
        UserFilesFolderPath,
        GeneratedFilesFolderPath,
        SettingsFileName
    }

    /// <summary> The list of keys for the EditorPrefs. </summary>
    private static readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.UserFilesFolderPath, "UserFilesFolderPath" },
            { EditorPrefKey.GeneratedFilesFolderPath, "GeneratedFilesFolderPath" },
            { EditorPrefKey.SettingsFileName, "SettingsFileName" }
        };

    /// <summary> Gets the generated files folder path from the EditorPrefs. </summary>
    /// <returns> Returns the stored generated files folder path. If no path is stored then returns the default path. </returns>
    public static string GetGenerateFilesFolderPathFromEditorPrefs()
    {
        string generatedPath = EditorPrefs.GetString(
            editorPrefKeys[EditorPrefKey.GeneratedFilesFolderPath]
        );
        if (string.IsNullOrEmpty(generatedPath))
        {
            //Default path
            return "Assets/UnityEditorAI/Generated/";
        }
        return generatedPath;
    }

    /// <summary> Gets the user files folder path from the EditorPrefs. </summary>
    /// <returns> Returns the stored user files folder path. If no path is stored then returns the default path. </returns>
    public static string GetUserFilesFolderPathFromEditorPrefs()
    {
        string userFilesPath = EditorPrefs.GetString(
            editorPrefKeys[EditorPrefKey.UserFilesFolderPath]
        );
        if (string.IsNullOrEmpty(userFilesPath))
        {
            //Default path
            return "Assets/UnityEditorAI/UserFiles/";
        }
        return userFilesPath;
    }

    /// <summary> Loads the EditorPrefs. </summary>
    private void LoadEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            if (EditorPrefs.HasKey(kvp.Value))
            {
                switch (kvp.Key)
                {
                    case EditorPrefKey.UserFilesFolderPath:
                        settingsFM.UserFilesFolderPath = EditorPrefs.GetString(kvp.Value);
                        break;
                    case EditorPrefKey.GeneratedFilesFolderPath:
                        settingsFM.GeneratedFilesFolderPath = EditorPrefs.GetString(kvp.Value);
                        break;
                    case EditorPrefKey.SettingsFileName:
                        settingsFM.SettingsFileName = EditorPrefs.GetString(kvp.Value);
                        break;
                }
            }
        }
    }

    /// <summary> Sets the EditorPrefs. </summary>
    private void SetEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.UserFilesFolderPath:
                    EditorPrefs.SetString(kvp.Value, settingsFM.UserFilesFolderPath);
                    break;
                case EditorPrefKey.GeneratedFilesFolderPath:
                    EditorPrefs.SetString(kvp.Value, settingsFM.GeneratedFilesFolderPath);
                    break;
                case EditorPrefKey.SettingsFileName:
                    EditorPrefs.SetString(kvp.Value, settingsFM.SettingsFileName);
                    break;
            }
        }
    }
}
