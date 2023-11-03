using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ColorExtruder : SingleExtensionApplication
{
    public override string DisplayName => "Color Extruder";
    public bool HasInit { get; set; } = false;
    public Sprite imageSprite;
    public static string colorArrayOutput = "";
    private const string colorArrayObjectFileName = "colorArrayObject.json";
    private bool showColorArrayContent = false;
    private Vector2 scrollPosition;

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
            RenderImageField();
            AddDefaultSpace();
            RenderOutputColorArrayField();
            RenderHelpBox();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void RenderImageField()
    {
        GUILayout.Label("Input an Image to get the color array", EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 50; // Adjust the label width
        GUILayout.BeginHorizontal();
        try
        {
            imageSprite =
                EditorGUILayout.ObjectField(
                    imageSprite,
                    typeof(Sprite),
                    false,
                    GUILayout.MaxWidth(132),
                    GUILayout.MaxHeight(132)
                ) as Sprite;
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.MaxHeight(132)
            );
            using (new EditorGUI.DisabledScope(true))
            {
                GUILayout.TextArea(OpenAiStandardPrompts.ColorImageGenerationPrompt.Content);
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
        AddDefaultSpace();

        GUILayout.BeginHorizontal();
        try
        {
            if (GUILayout.Button("Clear"))
            {
                imageSprite = null;
                ResetKeyboardControl();
            }
            else if (GUILayout.Button("Show Color Array Content"))
            {
                showColorArrayContent = !showColorArrayContent;
            }
            else if (GUILayout.Button("Get Color Array And Save It To File"))
            {
                GetColorArrayAndSaveItToFile();
                SetEditorPrefs();
            }
            else if (GUILayout.Button("Let AI Generate Color Generation Code"))
            {
                SendColorArrayMessageToAI();
            }
            else if (GUILayout.Button("Use Existing Color Generation Code"))
            {
                ColorSciptTest.GenerateColors();
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    private async void SendColorArrayMessageToAI()
    {
        ShowProgressBar(0.1f);
        string helpBoxMessage = "Sending message to OpenAI API...";
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
        Debug.Log(OpenAiStandardPrompts.ColorImageGenerationPrompt.Content);
        var messageListBuilder = new MessageListBuilder().AddMessage(
            OpenAiStandardPrompts.ColorImageGenerationPrompt.Content,
            "system"
        );
        ShowProgressBar(0.3f);
        string gptScriptResponse = await OpenAiApiManager.RequestToGpt(messageListBuilder);
        if (string.IsNullOrEmpty(gptScriptResponse))
        {
            helpBoxMessage = "No response from OpenAI API.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            FinishProgressBarWithDelay();
            return;
        }
        else
        {
            helpBoxMessage = "Successfully received response from OpenAI API.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            // Saves the response from the OpenAI API into doTaskScriptContent
            string cleanedColorScriptContent = ScriptUtil.CleanScript(gptScriptResponse);
            Debug.Log(gptScriptResponse);
            Debug.Log(cleanedColorScriptContent);
            FinishProgressBarWithDelay();
            Repaint();
        }
    }

    private void GetColorArrayAndSaveItToFile()
    {
        if (imageSprite != null)
        {
            var texture = imageSprite.texture;

            var textureImporter =
                AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
                int textureWidth = texture.width;
                int textureHeight = texture.height;
                var texture2D = texture;
                var texturePixels = texture2D.GetPixels();
                var pixelList = new List<int>();
                var colorList = new List<Color>();
                foreach (var pixel in texturePixels)
                {
                    // Check if color is already in colorList
                    int index = colorList.IndexOf(pixel);
                    // If the color is not found in colorList, add it
                    if (index == -1)
                    {
                        colorList.Add(pixel);
                        index = colorList.Count - 1; // Set index to the last added color
                    }
                    pixelList.Add(index);
                }

                var hexStringColorList = colorList
                    .Select(color => "#" + ColorUtility.ToHtmlStringRGB(color))
                    .ToList();

                ColorArrayObject colorArrayObject =
                    new(textureWidth, textureHeight, hexStringColorList, pixelList);
                string colorArrayObjectJson = SaveMessageHistoryToFile(colorArrayObject);
                colorArrayOutput = colorArrayObjectJson;
            }
        }
    }

    private string SaveMessageHistoryToFile(ColorArrayObject colorArrayObject)
    {
        string colorArrayObejctJson = FileManager<ColorArrayObject>.SaveJsonFileToDefaultPath(
            colorArrayObject,
            colorArrayObjectFileName
        );
        return colorArrayObejctJson;
    }

    public static ColorArrayObject ColorArrayObjectFromFile()
    {
        string userPath = AISettings.GetUserFilesFolderPathFromEditorPrefs();
        ColorArrayObject loadedColorArrayJson =
            FileManager<ColorArrayObject>.LoadDeserializedJsonFromPath(
                userPath,
                colorArrayObjectFileName
            );
        if (loadedColorArrayJson != null)
        {
            return loadedColorArrayJson;
        }
        else
            return null;
    }

    private void RenderOutputColorArrayField()
    {
        bool isColorArrayOutputEmpty = string.IsNullOrEmpty(colorArrayOutput);
        if (showColorArrayContent)
        {
            if (!isColorArrayOutputEmpty)
            {
                AddDefaultSpace();
                EditorGUILayout.BeginVertical();
                try
                {
                    GUIStyle codeStyle = CreateCodeStyle();
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GUILayout.TextArea(
                            colorArrayOutput,
                            codeStyle,
                            GUILayout.ExpandHeight(true)
                        );
                    }
                    using (new EditorGUI.DisabledScope(isColorArrayOutputEmpty))
                    {
                        GUIStyle customButtonStyle = CreateHighlightButtonStyle();
                        EditorGUILayout.BeginHorizontal();
                        try
                        {
                            if (GUILayout.Button("Delete Content"))
                            {
                                colorArrayOutput = "";
                            }
                            if (GUILayout.Button("Copy Content"))
                            {
                                EditorGUIUtility.systemCopyBuffer = colorArrayOutput;
                                string helpBoxMessage = "Copied colors to clipboard.";
                                helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                            }
                        }
                        finally
                        {
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                finally
                {
                    EditorGUILayout.EndVertical();
                }
            }
        }
    }

    public enum EditorPrefKey
    {
        SpriteGUID,
        ColorArrayObjectString,
        ShowColorArrayContent,
    }

    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.SpriteGUID, "SpriteGUIDKey" },
            { EditorPrefKey.ColorArrayObjectString, "ColorArrayObjectStringKey" },
            { EditorPrefKey.ShowColorArrayContent, "ShowColorArrayContentKey" },
        };

    private void LoadEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.SpriteGUID:
                    // Updated to use GUID because it is more reliable than path

                    string spritePath = AssetDatabase.GUIDToAssetPath(
                        EditorPrefs.GetString(kvp.Value)
                    );
                    imageSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    break;
                case EditorPrefKey.ColorArrayObjectString:
                    colorArrayOutput = EditorPrefs.GetString(kvp.Value);
                    break;
                case EditorPrefKey.ShowColorArrayContent:
                    showColorArrayContent = EditorPrefs.GetBool(kvp.Value);
                    break;
            }
        }
    }

    private void SetEditorPrefs()
    {
        foreach (var kvp in editorPrefKeys)
        {
            switch (kvp.Key)
            {
                case EditorPrefKey.SpriteGUID:

                    if (imageSprite != null)
                    {
                        // Updated to use GUID because it is more reliable than path
                        string spriteGUID = AssetDatabase.AssetPathToGUID(
                            AssetDatabase.GetAssetPath(imageSprite)
                        );
                        EditorPrefs.SetString(kvp.Value, spriteGUID);
                    }
                    else
                    {
                        EditorPrefs.SetString(kvp.Value, "");
                    }
                    break;
                case EditorPrefKey.ColorArrayObjectString:
                    EditorPrefs.SetString(kvp.Value, colorArrayOutput);
                    break;
                case EditorPrefKey.ShowColorArrayContent:
                    EditorPrefs.SetBool(kvp.Value, showColorArrayContent);
                    break;
            }
        }
    }
}
