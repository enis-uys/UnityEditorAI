using UnityEngine;
using System.Text.RegularExpressions;

using UnityEditor;

public class ScriptUtil
{
    public static string CleanScript(string inputString)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            string helpBoxMessage = "Input string is null or empty";
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Warning);
            return inputString;
        }
        // Step 1: Remove text before and after backticks

        // Pattern: Match two or more backticks at the start or end and remove before and after
        string pattern1 = ".*?(`{2,})(.*?)(`{2,})(.*)";
        string replacement1 = "$1$2$3";
        string outputString1 = Regex.Replace(
            inputString,
            pattern1,
            replacement1,
            RegexOptions.Multiline
        );

        // Step 2: Remove any remaining backticks
        string pattern2 = @"(`{2,})";
        string replacement2 = "";
        string outputString2 = Regex.Replace(
            outputString1,
            pattern2,
            replacement2,
            RegexOptions.Multiline
        );

        return outputString2;
    }

    public static string ExtractClassNameFromScript(string scriptString)
    {
        //checks for the word after the first class keyword and uses it as the class name
        string pattern = @"\bclass\s+(\w+)\b";

        Match match = Regex.Match(scriptString, pattern);
        if (match.Success)
        {
            string className = match.Groups[1].Value;
            string helpBoxMessage = "Class name extracted from script: " + className;
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Info);
            return className;
        }
        else
        {
            return "ClassNameNotFound";
        }
    }

    public static bool IsValidMessageFormat(string message)
    {
        return Regex.IsMatch(message, "^(User|System): .+$");
    }

    public static bool IsValidScript(string scriptString)
    {
        // Clean the script string to remove unwanted characters
        string cleanedScript = CleanScript(scriptString);

        // Extract the class name from the cleaned script
        string className = ExtractClassNameFromScript(cleanedScript);

        if (string.IsNullOrEmpty(cleanedScript))
        {
            string helpBoxMessage = "Script is null or empty";
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Error);
            // Script is null or empty
            return false;
        }
        else if (string.IsNullOrEmpty(className))
        {
            string helpBoxMessage = "Class name is null or empty";
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Error);
            // Class name is null or empty
            return false;
        }
        // Check if the extracted class name is valid
        else if (className == "ClassNameNotFound")
        {
            string helpBoxMessage = "Class name not found in the script";
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Error);
            // Class name not found in the script
            return false;
        }
        else
        {
            // Script is valid
            return true;
        }
    }
}
