using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class CheckForPluginsOnInit
{
    static CheckForPluginsOnInit()
    {
        string newtonSoftPath = Path.Combine(
            "Assets/UnityEditorAI/Plugins/",
            "Newtonsoft.Json.dll"
        );
        if (!File.Exists(newtonSoftPath))
        {
            // The Newtonsoft.Json.dll file is missing
            Debug.LogError(
                "Newtonsoft.Json is missing in your project. "
                    + "Please download it from https://github.com/JamesNK/Newtonsoft.Json/releases and add it to your project. UnityEditorAI/Plugins folder"
            );
        }
        string uniTaskPath = Path.Combine("Assets/UnityEditorAI/Plugins/", "UniTask");

        if (!Directory.Exists(uniTaskPath))
        {
            // The UniTask Folder is missing
            Debug.LogError(
                "UniTask is missing in your project. "
                    + "Please download it from https://github.com/Cysharp/UniTask/releases and add it to your project inside the UnityEditorAI/Plugins folder."
            );
        }
    }
}
