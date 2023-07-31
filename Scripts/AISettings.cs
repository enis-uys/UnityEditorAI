using UnityEditor;
using UnityEngine;

public class AISettings : SingleExtensionApplication
{
    public override string DisplayName => "AI Settings";
    private AISettingsFileManager settingsFM = new AISettingsFileManager();

    private static MessageType helpBoxMessageType;

    private static string helpBoxMessage;

    public override void OnEnable()
    {
        settingsFM.LoadSettingsFromJson();
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

            EditorGUILayout.Space();
            GUILayout.Label("Model", EditorStyles.boldLabel);
            // Get the corresponding string value from the dictionary//

            settingsFM.SelectedGptModel = (AISettingsFileManager.GptModels)
                EditorGUILayout.EnumPopup("Select an option:", settingsFM.SelectedGptModel);

            EditorGUILayout.Space();
            //we create a slider for the temperature now
            GUILayout.Label(
                new GUIContent(
                    "Temperature",
                    "The higher the temperature (0f-1f) the more random the output will be. The standard temperature is 1f."
                ),
                EditorStyles.boldLabel
            );
            settingsFM.Temperature = EditorGUILayout.Slider(settingsFM.Temperature, 0.0f, 1.0f);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Test API"))
            {
                TestAPI();
            }
            if (GUILayout.Button(new GUIContent("Save Settings", "Saves API Key to a json file")))
            {
                settingsFM.SaveSettingsInJson();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(helpBoxMessage, helpBoxMessageType);
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    public static void UpdateHelpMessage(string message, MessageType messageType)
    {
        helpBoxMessage = message;
        helpBoxMessageType = messageType;
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
            UpdateHelpMessage("API Key is valid!", MessageType.Info);
        }
        else
        {
            UpdateHelpMessage(
                "API Key is not valid! Or you have used gpt-4 or davinci. Those are work in progress!",
                MessageType.Error
            );
        }
    }
}
