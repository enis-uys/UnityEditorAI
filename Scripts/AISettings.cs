using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

public class AISettings : SingleExtensionApplication
{
    public override string DisplayName => "AI Settings";
    private static readonly AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();

    private bool HasInit { get; set; } = false;
    private bool ShouldLoadEditorPrefs { get; set; } = true;

    //Important to only set this OnEnable otherwise settings might not be loaded
    public override void OnEnable()
    {
        if (!HasInit)
        {
            settingsFM.LoadCustomSettings();
            HasInit = true;
        }
    }

    public override void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            if (ShouldLoadEditorPrefs)
            {
                LoadEditorPrefs();
                ShouldLoadEditorPrefs = false;
            }
            RenderApiKeyField();
            AddDefaultSpace();

            RenderFilePathField();
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

    private void RenderApiKeyField()
    {
        settingsFM.ApiKey = EditorGUILayout.TextField("OpenAI API Key", settingsFM.ApiKey);
    }

    private void RenderFilePathField()
    {
        GUILayout.Label("Settings File Path", EditorStyles.boldLabel);
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
        GUILayout.Label("File Path for Generated Elements", EditorStyles.boldLabel);

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

    private void RenderModelSelectionField()
    {
        GUILayout.Label("Model", EditorStyles.boldLabel);
        // Get the corresponding string value from the dictionary
        int selectedGptModelInt = EditorGUILayout.Popup(
            "Select an option:",
            settingsFM.SelectedGptModelInt(),
            settingsFM.gptModelsArray
        );
        // Get the corresponding key from the dictionary
        settingsFM.SelectedGptModel = settingsFM.gptModelsArray[selectedGptModelInt];
    }

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

    private void RenderActionButtons()
    {
        GUILayout.BeginHorizontal();
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
        if (GUILayout.Button(new GUIContent("Save Settings", "Saves the settings to a json file")))
        {
            settingsFM.WriteSettingsInJson();
        }
        GUILayout.EndHorizontal();
    }

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

    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.UserFilesFolderPath, "UserFilesFolderPath" },
            { EditorPrefKey.GeneratedFilesFolderPath, "GeneratedFilesFolderPath" },
            { EditorPrefKey.SettingsFileName, "SettingsFileName" }
        };

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
