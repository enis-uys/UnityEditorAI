using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary> The color extruder, that extracts the colors of an image and saves them as a color array object. </summary>
public class ColorExtruder : SingleExtensionApplication
{
    /// <summary> The display name of the color extruder. </summary>
    public override string DisplayName => "Color Extruder";
    public bool HasInit { get; set; } = false;

    /// <summary> The image that should be used to extract the colors. Can be dragged into the field. </summary>
    public Sprite imageSprite;

    /// <summary> The output of the color array object as a string. </summary>
    public static string colorArrayOutput = "";

    /// <summary> The name of the file that the color array object is saved to. </summary>
    private const string colorArrayObjectFileName = "colorArrayObject.json";

    /// <summary> Bool that indicates if the color array content should be shown because it is too long. </summary>
    private bool showColorArrayContent = false;

    /// <summary> The content of the color script that the AI did generate. </summary>
    private string colorScriptContent;

    /// <summary> The scroll position of the displayed prompt text. </summary>
    private Vector2 scrollPosition;

    /// <summary> The GUI of the color extruder. </summary>
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
            AddDefaultSpace();
            RenderOutputScriptField();
            RenderOutputColorArrayField();
            AddDefaultSpace();
            RenderHelpBox();
            SetEditorPrefs();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary> Method that renders the ImageField and a prompt that is used for "Let AI Generate Color Generation Code". </summary>
    private void RenderImageField()
    {
        GUILayout.Label("Input an Image to get the color array", EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 50;
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

        RenderActionButtons();
    }

    /// <summary> Method that renders the action buttons of the color extruder. </summary>
    private void RenderActionButtons()
    {
        GUILayout.BeginHorizontal();
        try
        {
            if (GUILayout.Button("Clear"))
            {
                imageSprite = null;
                ResetKeyboardControl();
            }
            else if (GUILayout.Button("Show/Hide Color Array Content"))
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
                SendAIColorArrayPrompt();
            }
            else if (GUILayout.Button("Use Existing Color Generation Code"))
            {
                ColorScriptDemo.GenerateColors();
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
        }
    }

    /// <summary> Method that sends the color array prompt to the OpenAI API. </summary>
    private async void SendAIColorArrayPrompt()
    {
        ShowProgressBar(0.1f);
        string helpBoxMessage = "Sending message to OpenAI API...";
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
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
            colorScriptContent = cleanedColorScriptContent;
            FinishProgressBarWithDelay();
            Repaint();
        }
    }

    /// <summary> Method that gets the color array of the image and saves it to a file inside the UserFiles folder. </summary>
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
                        index = colorList.Count - 1;
                    }
                    pixelList.Add(index);
                }

                var hexStringColorList = colorList
                    .Select(color => "#" + ColorUtility.ToHtmlStringRGB(color))
                    .ToList();

                ColorArrayObject colorArrayObject =
                    new(textureWidth, textureHeight, hexStringColorList, pixelList);
                string colorArrayObjectJson = SaveColorArrayObjectToFile(colorArrayObject);
                colorArrayOutput = colorArrayObjectJson;
            }
        }
    }

    /// <summary> Method that saves a color array object to a file as a json string. </summary>
    /// <param name="colorArrayObject"> The color array object that should be saved to a file. </param>
    /// <returns> The json string of the color array object. </returns>
    private string SaveColorArrayObjectToFile(ColorArrayObject colorArrayObject)
    {
        string colorArrayObejctJson = FileManager<ColorArrayObject>.SaveJsonFileToDefaultPath(
            colorArrayObject,
            colorArrayObjectFileName
        );
        return colorArrayObejctJson;
    }

    /// <summary> Method that reads a color array string from a file and returns it as a color array object. </summary>
    /// <returns> Returns the color array object that was read from the file or null if the file could not be read. </returns>
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

    /// <summary> Method that renders the output script field. </summary>
    private void RenderOutputScriptField()
    {
        bool scriptContentEmpty = string.IsNullOrEmpty(colorScriptContent);
        if (!scriptContentEmpty)
        {
            AddDefaultSpace();
            EditorGUILayout.BeginVertical();
            try
            {
                GUIStyle codeStyle = CreateCodeStyle();
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.TextArea(colorScriptContent, codeStyle, GUILayout.ExpandHeight(true));
                }
                using (new EditorGUI.DisabledScope(scriptContentEmpty))
                {
                    GUIStyle customButtonStyle = CreateHighlightButtonStyle();
                    EditorGUILayout.BeginHorizontal();
                    try
                    {
                        if (GUILayout.Button("Delete Content"))
                        {
                            colorScriptContent = "";
                        }
                        if (GUILayout.Button("Copy Content"))
                        {
                            EditorGUIUtility.systemCopyBuffer = colorScriptContent;
                            string helpBoxMessage = "Copied script to clipboard.";
                            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                        }
                        if (GUILayout.Button("Save Color Generation Script", customButtonStyle))
                        {
                            WriteColorScriptInFile();
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

    /// <summary> Saves the generated script into a file inside the GenerateFolder </summary>
    private void WriteColorScriptInFile()
    {
        string gptScriptClassName = ScriptUtil.ExtractNameAfterKeyWordFromScript(
            colorScriptContent,
            "class"
        );
        if (string.IsNullOrEmpty(gptScriptClassName))
        {
            string helpBoxMessage = "Could not extract class name from script.";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return;
        }
        string generatePath =
            AISettings.GetGenerateFilesFolderPathFromEditorPrefs() + gptScriptClassName + ".cs";
        FileManager<string>.CreateScriptAssetWithReflection(generatePath, colorScriptContent);
        AssetDatabase.Refresh();
        colorScriptContent = "";
    }

    /// <summary> Method that renders the output color array field. </summary>
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

    /// <summary> Dictionary that contains the keys for the editor prefs. </summary>
    private readonly Dictionary<EditorPrefKey, string> editorPrefKeys =
        new()
        {
            { EditorPrefKey.SpriteGUID, "SpriteGUIDKey" },
            { EditorPrefKey.ColorArrayObjectString, "ColorArrayObjectStringKey" },
            { EditorPrefKey.ShowColorArrayContent, "ShowColorArrayContentKey" },
        };

    /// <summary> Method that loads the editor prefs. </summary>
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

    /// <summary> Method that sets the editor prefs. </summary>
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
