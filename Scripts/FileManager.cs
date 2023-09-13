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

    private static string defaultPath = "Assets/UnityEditorAI/UserFiles/";
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
        helpBox.UpdateMessageAndType(
            "Saving data to file: " + folderPath + fileNameAndType,
            MessageType.Info
        );
        if (!Directory.Exists(folderPath))
        {
            helpBox.AppendMessage("Directory does not exist, creating directory: " + folderPath);
            Directory.CreateDirectory(folderPath);
        }
        if (!File.Exists(folderPath + fileNameAndType))
        {
            helpBox.AppendMessage(
                "File does not exist, creating file: " + folderPath + fileNameAndType
            );
            using (StreamWriter sw = File.CreateText(folderPath + fileNameAndType)) { }
        }
        File.WriteAllText(folderPath + fileNameAndType, data);
        helpBox.AppendMessage("Data saved to file: " + folderPath + fileNameAndType);
    }

    // TODO: Create general functions for datatype and lead to specific functions --> next for script / cs
    public static void SaveJsonToDefaultPath(T data, string fileName)
    {
        SaveToJsonFileWithPath(data, defaultPath, fileName);
    }

    // TODO: Try Catch

    public static void SaveToJsonFileWithPath(T data, string folderPath, string fileName)
    {
        HelpBox helpBox = HelpBox.GetInstance();
        var jsonData = JsonConvert.SerializeObject(data);
        helpBox.UpdateMessageAndType(
            "Saving data to file: " + folderPath + fileName,
            MessageType.Info
        );
        if (!Directory.Exists(folderPath))
        {
            helpBox.AppendMessage("Directory does not exist, creating directory at: " + folderPath);
            Directory.CreateDirectory(folderPath);
        }
        if (!File.Exists(folderPath + fileName))
        {
            helpBox.AppendMessage("File does not exist, creating file: " + folderPath + fileName);
            using (StreamWriter sw = File.CreateText(folderPath + fileName)) { }
        }
        File.WriteAllText(folderPath + fileName, jsonData);
        helpBox.AppendMessage("Data saved to file: " + folderPath + fileName);
    }

    public static T LoadDeserializedJsonFromDefaultPath(string fileName)
    {
        return LoadDeserializedJsonFromPath<T>(defaultPath, fileName);
    }

    public static T LoadDeserializedJsonPanel(string screenTitle)
    {
        try
        {
            HelpBox helpBox = HelpBox.GetInstance();
            string filePath = EditorUtility.OpenFilePanel(screenTitle, defaultPath, "json");
            if (!string.IsNullOrEmpty(filePath) && filePath.ToLower().EndsWith(".json"))
            {
                string fileName = Path.GetFileName(filePath);
                string directoryPath = Path.GetDirectoryName(filePath) + "\\";
                return LoadDeserializedJsonFromPath<T>(directoryPath, fileName);
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                helpBox.AppendMessageAndType("File not found: " + filePath, MessageType.Error);
                Debug.LogError("File not found: " + filePath);
            }
        }
        catch (System.Exception ex)
        {
            HelpBox
                .GetInstance()
                .UpdateMessageAndType("An error occurred: " + ex.Message, MessageType.Error);
            Debug.LogError("An error occurred: " + ex.Message);
        }
        return default;
    }

    public static J LoadDeserializedJsonFromPath<J>(string folderPath, string fileName)
    {
        HelpBox helpBox = HelpBox.GetInstance();
        helpBox.UpdateMessageAndType(
            "Loading data from: " + folderPath + fileName,
            MessageType.Warning
        );
        if (!Directory.Exists(folderPath))
        {
            helpBox.AppendMessage("Directory does not exist, creating directory at: " + folderPath);
            Directory.CreateDirectory(folderPath);
        }
        if (File.Exists(folderPath + fileName))
        {
            string jsonData = File.ReadAllText(folderPath + fileName);
            J data = JsonConvert.DeserializeObject<J>(jsonData);
            helpBox.AppendMessage("Data loaded from: " + folderPath + fileName);
            return data;
        }
        else
        {
            helpBox.AppendMessageAndType(
                "File not found: " + folderPath + fileName,
                MessageType.Error
            );
            Debug.LogError("File not found: " + folderPath + fileName);
            return default;
        }
    }
}
