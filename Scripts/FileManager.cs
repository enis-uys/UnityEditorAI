using System;
using UnityEditor;
using System.IO;

/// <Title> Newtonsoft.JSON GitHub Repository </Title>
/// <Author> James Newton-King (JamesNK) </Author>
/// <Release Date> 08.03.2023 </Release Date>
/// <Access Date> 10.09.2023 </Access Date>
/// <Code version> 13.0.3 </Code version>
/// <Availability> https://github.com/JamesNK/Newtonsoft.Json </Availability>
/// <Usecase> JSON Serialization </Usecase>
/// <License> Open-Source MIT License https://opensource.org/licenses/MIT </License>
/// <Description>
///Newtonsoft.JSON is a popular .NET library for working with JSON data.
///It provides powerful JSON serialization and deserialization capabilities.
/// </Description>
using Newtonsoft.Json;
using System.Reflection;

/// <summary> The file manager class that contains methods for saving and loading json files. </summary>
/// <typeparam name="T">  The generic type that is used for saving and loading json files.  </typeparam>
public class FileManager<T>
{
    public static AISettingsFileManager settingsFM = AISettingsFileManager.GetInstance();

    //This part is partly adapted from Kenjiro AICommand (AICommandWindow.cs)
    /// <Availability> https://github.com/keijiro/AICommand/ </Availability>
    ///<License> Unlicense (Public Domain) View LICENSE.md to see the license and information. </License>
    ///<Description> AICommand is a Unity extension that experiment with a command window for executing C# scripts from the gpt api. </Description>
    /// <summary>
    /// Creates a script asset in Unity by invoking a private method in Unity's ProjectWindowUtil.
    /// It is not possible to use the method directly, so reflection is used to access the method.
    /// </summary>
    public static void CreateScriptAssetWithReflection(string path, string data)
    {
        // Use reflection to access the private method 'CreateScriptAssetWithContent' in Unity's ProjectWindowUtil.
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
        // Use reflection to invoke 'CreateScriptAssetWithContent' to create a script asset in Unity.
        method?.Invoke(null, new object[] { path, data });
    }

    //End of adapted part from Kenjiro AICommand.

    /// <summary>
    /// Invokes a static method in a class by using reflection. The method must be public or private.
    /// It is used to invoke a method when it is not available in runtime. (AI Object Generation)
    /// </summary>
    /// <param name="className"> The name of the class that contains the method. </param>
    /// <param name="methodName"> The name of the method that is invoked. </param>
    public static void InvokeFunction(string className, string methodName)
    {
        var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var classType = Type.GetType(className);
        if (classType != null)
        {
            var method = classType.GetMethod(methodName, flags);
            method?.Invoke(null, null);
        }
    }

    /// <summary> Saves a json file to the default path of the extension. </summary>
    /// <param name="data"> The generic data that is saved to the json file.</param>
    /// <param name="fileName"> The name of the json file. This will be converted to a path by adding the default path of the extension. </param>
    /// <returns> The json data that is saved to the file. </returns>
    public static string SaveJsonFileToDefaultPath(T data, string fileName)
    {
        string path = settingsFM.UserFilesFolderPath + fileName;
        return SaveToJsonFileWithPath(data, path);
    }

    /// <summary> Saves a json file to a specified path. </summary>
    /// <param name="data"> The generic data that is saved to the json file. </param>
    /// <param name="filePath"> The path of the json file. </param>
    /// <returns> The json string that is saved to the file. </returns>
    public static string SaveToJsonFileWithPath(T data, string filePath)
    {
        HelpBox helpBox = HelpBox.GetInstance();
        string helpBoxMessage;
        try
        {
            helpBoxMessage = "Saving data to file: " + filePath;
            MessageType messageType = MessageType.Info;
            helpBox.UpdateMessage(helpBoxMessage, messageType);

            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            string folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                helpBoxMessage = "Directory does not exist, creating directory at: " + folderPath;
                helpBox.UpdateMessage(helpBoxMessage, messageType, true);
                Directory.CreateDirectory(folderPath);
            }
            CreateFileIfNotExisting(filePath);
            File.WriteAllText(filePath, jsonData);
            helpBoxMessage = "Data saved to file: " + filePath;
            helpBox.UpdateMessage(helpBoxMessage, messageType);
            return jsonData;
        }
        catch (Exception ex)
        {
            helpBoxMessage = "An error occurred: " + ex.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, false, true);
            return null;
        }
    }

    /// <summary> Loads a json file from a file panel and deserializes it to a generic type. </summary>
    /// <param name="screenTitle"> The title of the file panel. The default value is "Load Json File". </param>
    /// <returns> The generic data that is deserialized from the json file. </returns>
    public static T LoadDeserializedJsonPanel(string screenTitle = "Load Json File")
    {
        HelpBox helpBox = HelpBox.GetInstance();
        string helpBoxMessage;
        try
        {
            helpBoxMessage = "Loading data from file panel";
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
            string filePath = EditorUtility.OpenFilePanel(
                screenTitle,
                settingsFM.UserFilesFolderPath,
                "json"
            );
            if (!string.IsNullOrEmpty(filePath) && filePath.ToLower().EndsWith(".json"))
            {
                string fileName = Path.GetFileName(filePath);
                string directoryPath = Path.GetDirectoryName(filePath) + "\\";
                return LoadDeserializedJsonFromPath(directoryPath, fileName);
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                helpBoxMessage = "File not found or not a json file: " + filePath;
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, true, true);
            }
        }
        catch (Exception ex)
        {
            helpBoxMessage = "An error occurred: " + ex.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, true, true);
        }
        return ReturnEmptyT();
    }

    /// <summary>
    /// Loads a json file from a specified path and deserializes it to a generic type.
    /// It combines the folder path and the file name to a path and calls an overloaded method with the path as parameter.
    /// </summary>
    /// <param name="folderPath"> The path of the folder that contains the json file. </param>
    /// <param name="fileName"> The name of the loaded json file. </param>
    /// <returns> The generic data that is deserialized from the json file. </returns>
    public static T LoadDeserializedJsonFromPath(string folderPath, string fileName)
    {
        string path = folderPath + fileName;
        return LoadDeserializedJsonFromPath(path);
    }

    /// <summary> Loads a json file from a specified path and deserializes it to a generic type.
    /// </summary>
    /// <param name="filePath"> The path of the json file. </param>
    /// <returns> The generic data that is deserialized from the json file. </returns>
    public static T LoadDeserializedJsonFromPath(string filePath)
    {
        HelpBox helpBox = HelpBox.GetInstance();
        string helpBoxMessage = "Loading data from: " + filePath;
        helpBox.UpdateMessage(helpBoxMessage, MessageType.Warning);
        try
        {
            string folderPath = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            if (!Directory.Exists(folderPath))
            {
                helpBoxMessage = "Directory does not exist, creating directory at: " + folderPath;
                helpBox.UpdateMessage(helpBoxMessage, null, true);
                Directory.CreateDirectory(folderPath);
            }
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                try
                {
                    T data = JsonConvert.DeserializeObject<T>(jsonData);
                    helpBoxMessage = "Data loaded from: " + filePath;
                    helpBox.UpdateMessage(helpBoxMessage, MessageType.Info);
                    if (data == null)
                    {
                        return ReturnEmptyT();
                    }
                    return data;
                }
                catch (JsonException jsonEx)
                {
                    throw jsonEx;
                }
            }
            else
            {
                helpBoxMessage = "File not found: " + filePath;
                helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, true, true);
                return ReturnEmptyT();
            }
        }
        catch (JsonException jsonEx)
        {
            throw jsonEx;
        }
        catch (Exception ex)
        {
            helpBoxMessage = "An error occurred: " + ex.Message;
            helpBox.UpdateMessage(helpBoxMessage, MessageType.Error, true, true);
            return ReturnEmptyT();
        }
    }

    /// <summary> Returns an empty generic type. This is used when the deserialization of a json file fails.
    /// </summary>
    /// <returns> An empty generic type. </returns>
    public static T ReturnEmptyT()
    {
        if (!typeof(T).IsValueType)
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
        return default;
    }

    /// <summary> Creates a file if it does not exist. </summary>
    /// <param name="filePath"> The path of the file. </param>
    /// <returns> Returns true if the file exists, false if the file does not exist. </returns>
    public static bool CreateFileIfNotExisting(string filePath)
    {
        bool fileExists = File.Exists(filePath);
        if (!fileExists)
        {
            string helpBoxMessage = "File does not exist, creating file at: " + filePath;
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Info, true);
            using StreamWriter sw = File.CreateText(filePath);
        }
        return fileExists;
    }

    /// <summary> Serializes data to a json string. </summary>
    /// <param name="data"> The generic data that is serialized to a json string. </param>
    /// <param name="formatting"> The formatting of the json string. The default value is Formatting.Indented. </param>
    /// <returns> Returns the serialized data as a json string. </returns>
    public static string SerializeDataToJson(T data, Formatting? formatting = Formatting.Indented)
    {
        string json;
        json = JsonConvert.SerializeObject(data, formatting.Value);
        return json;
    }

    /// <summary> Deserializes a json string to a generic type. </summary>
    /// <param name="jsonData"> The json string that is deserialized to a generic type. </param>
    /// <returns> Returns the deserialized data as a generic type. </returns>
    public static T DeserializeJsonString(string jsonData)
    {
        try
        {
            T data = JsonConvert.DeserializeObject<T>(jsonData);
            return data;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return default;
        }
    }
}
