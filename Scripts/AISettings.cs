using UnityEditor;
using UnityEngine;

public class AISettings : SingleExtensionApplication
{
    public override string DisplayName => "AI Settings";
    private AISettingsFileManager settingsFM = new AISettingsFileManager();

    private bool hasInit = false;

    public override void OnEnable()
    {
        if (!hasInit)
        {
            settingsFM.LoadCustomSettings();
            hasInit = true;
        }
    }

    public override void OnGUI()
    {
        RenderSettingsGUI();
    }

    private void RenderSettingsGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        try
        {
            settingsFM.ApiKey = EditorGUILayout.TextField("OpenAI API Key", settingsFM.ApiKey);

            EditorGUILayout.Space(defaultSpace);
            GUILayout.Label("Model", EditorStyles.boldLabel);
            // Get the corres   nding string value from the dictionary//

            settingsFM.SelectedGptModel = (AISettingsFileManager.GptModels)
                EditorGUILayout.EnumPopup("Select an option:", settingsFM.SelectedGptModel);

            EditorGUILayout.Space(defaultSpace);
            //we create a slider for the temperature now
            GUILayout.Label(
                new GUIContent(
                    "Temperature",
                    "The higher the temperature (0f-1f) the more random the output will be. The standard temperature is 1f."
                ),
                EditorStyles.boldLabel
            );

            settingsFM.Temperature = EditorGUILayout.Slider(settingsFM.Temperature, 0.0f, 1.0f);
            EditorGUILayout.Space(defaultSpace);

            GUILayout.Label(
                new GUIContent(
                    "Max Tokens",
                    "The maximum number of tokens to generate. Requests can use up to 4096 tokens shared between prompt and completion. (One token is around 4 characters for normal English text.)"
                ),
                EditorStyles.boldLabel
            );
            settingsFM.MaxTokens = EditorGUILayout.IntField(settingsFM.MaxTokens);
            // Clamp value between 0 and 4096
            settingsFM.MaxTokens = EditorGUILayout.IntSlider(settingsFM.MaxTokens, 0, 4096);
            EditorGUILayout.Space(defaultSpace);
            GUILayout.Label(
                new GUIContent(
                    "Timeout in Seconds",
                    "The maximum number of seconds to wait for a response from the server. The default is 20 seconds."
                ),
                EditorStyles.boldLabel
            );
            settingsFM.TimeoutInSeconds = EditorGUILayout.IntSlider(
                settingsFM.TimeoutInSeconds,
                5,
                60
            );
            EditorGUILayout.Space(defaultSpace);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test API"))
            {
                TestAPI();
            }

            if (GUILayout.Button("Reset To Default Settings"))
            {
                //     settingsFM.LoadAndCreateDefaultSettings();
            }
            if (GUILayout.Button("Load Settings From File"))
            {
                settingsFM.LoadSettingsFromFile();
            }
            if (GUILayout.Button(new GUIContent("Save Settings", "Saves API Key to a json file")))
            {
                settingsFM.SaveSettingsInJson();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(defaultSpace);
            EditorGUILayout.HelpBox(helpBox.HBMessage, helpBox.HBMessageType);
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void TestAPI()
    {
        bool isGpt4OrDavinci =
            settingsFM.SelectedGptModel == AISettingsFileManager.GptModels.Gpt4
            || settingsFM.SelectedGptModel == AISettingsFileManager.GptModels.TextDavinci003;
        bool isValidApiKey =
            OpenAiManager.TestConnection(
                settingsFM.ApiKey,
                settingsFM.gptModelDictionary[settingsFM.SelectedGptModel],
                settingsFM.Temperature
            ) != null;

        if (isValidApiKey && !isGpt4OrDavinci)
        {
            helpBox.UpdateMessageAndType("API Key is valid!", MessageType.Info);
        }
        else
        {
            helpBox.UpdateMessageAndType(
                "API Key is not valid! Or you have used gpt-4 or davinci. Those are work in progress!",
                MessageType.Error
            );
        }
    }
}
