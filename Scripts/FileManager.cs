using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
//permissive open-source license (MIT License) --> https://opensource.org/licenses/MIT
//TODO: Add the dll to the project but include the proper licence and attribution
// --> include in code references
using Newtonsoft.Json;

// for class name extraction
using System.Text.RegularExpressions;

public class FileManager<T>
{
    //TODO: Update this with a Path Manager that reads file path from config / settings

    private static string defaultPath = "Assets/UnityEditorAI/Saved Files/";
    private static string defaultGeneratedPath = "Assets/UnityEditorAI/Generated/";

    public static void SaveStringToFileInDefaultPath(string data, string fileNameAndType)
    {
        SaveStringToFileWithPath(defaultPath, fileNameAndType, data);
    }

    public static void SaveStringToFileInGeneratedPath(string data, string fileNameAndType)
    {
        SaveStringToFileWithPath(defaultGeneratedPath, fileNameAndType, data);
    }

    public static void SaveStringToFileWithPath(
        string folderPath,
        string fileNameAndType,
        string data
    )
    {
        HelpBox helpBox = HelpBox.GetInstance();
        helpBox.UpdateHelpBoxMessageAndType(
            "Saving data to file: " + folderPath + fileNameAndType,
            MessageType.Info
        );
        if (!Directory.Exists(folderPath))
        {
            helpBox.AppendHelpBoxMessage(
                "Directory does not exist, creating directory: " + folderPath
            );
            Directory.CreateDirectory(folderPath);
        }
        if (!File.Exists(folderPath + fileNameAndType))
        {
            helpBox.AppendHelpBoxMessage(
                "File does not exist, creating file: " + folderPath + fileNameAndType
            );
            using (StreamWriter sw = File.CreateText(folderPath + fileNameAndType)) { }
        }
        File.WriteAllText(folderPath + fileNameAndType, data);
        helpBox.AppendHelpBoxMessage("Data saved to file: " + folderPath + fileNameAndType);
    }

    // TODO: Create generall functions for datatype and lead to specific functions --> next for script / cs
    public static void SaveJsonToDefaultPath(T data, string fileName)
    {
        SaveToJsonFileWithPath(data, defaultPath, fileName);
    }

    // TODO: Try Catch

    public static void SaveToJsonFileWithPath(T data, string folderPath, string fileName)
    {
        HelpBox helpBox = HelpBox.GetInstance();
        var jsonData = JsonConvert.SerializeObject(data);
        string fileNameWithJson = fileName + ".json";
        helpBox.UpdateHelpBoxMessageAndType(
            "Saving data to file: " + folderPath + fileNameWithJson,
            MessageType.Info
        );
        if (!Directory.Exists(folderPath))
        {
            helpBox.AppendHelpBoxMessage(
                "Directory does not exist, creating directory at: " + folderPath
            );
            Directory.CreateDirectory(folderPath);
        }
        if (!File.Exists(folderPath + fileNameWithJson))
        {
            helpBox.AppendHelpBoxMessage(
                "File does not exist, creating file: " + folderPath + fileNameWithJson
            );
            using (StreamWriter sw = File.CreateText(folderPath + fileNameWithJson)) { }
        }
        File.WriteAllText(folderPath + fileNameWithJson, jsonData);
        helpBox.AppendHelpBoxMessage("Data saved to file: " + folderPath + fileNameWithJson);
    }

    public static T LoadDeserializedJsonFromDefaultPath(string fileName)
    {
        return LoadDeserializedJsonFromPath<T>(defaultPath, fileName);
    }

    public static J LoadDeserializedJsonFromPath<J>(string folderPath, string fileName)
    {
        HelpBox helpBox = HelpBox.GetInstance();
        helpBox.UpdateHelpBoxMessageAndType(
            "Loading data from: " + folderPath + fileName + ".json",
            MessageType.Warning
        );
        string fileNameWithJson = fileName + ".json";
        if (!Directory.Exists(folderPath))
        {
            helpBox.AppendHelpBoxMessage(
                "Directory does not exist, creating directory at: " + folderPath
            );
            Directory.CreateDirectory(folderPath);
        }
        if (File.Exists(folderPath + fileNameWithJson))
        {
            string jsonData = File.ReadAllText(folderPath + fileNameWithJson);
            J data = JsonConvert.DeserializeObject<J>(jsonData);
            helpBox.AppendHelpBoxMessageAndType(
                "Data loaded from: " + folderPath + fileNameWithJson,
                MessageType.Info
            );
            return data;
        }
        else
        {
            helpBox.AppendHelpBoxMessageAndType(
                "File not found: " + folderPath + fileNameWithJson,
                MessageType.Error
            );
            Debug.LogError("File not found: " + folderPath + fileNameWithJson);
            return default;
        }
    }

    public static string ExtractClassNameFromScript(string scriptString)
    {
        //pattern done with chatgpt
        string pattern =
            @"(?:public\s+|private\s+|protected\s+)?(?:static\s+)?class\s+([A-Za-z_]\w*)\b";
        Match match = Regex.Match(scriptString, pattern);
        if (match.Success)
        {
            HelpBox
                .GetInstance()
                .UpdateHelpBoxMessageAndType(
                    "Class name extracted from script: " + match.Groups[1].Value,
                    MessageType.Info
                );
            return match.Groups[1].Value;
        }
        else
        {
            return "NameNotFound";
        }
    }
}
