using System.Text.RegularExpressions;

using UnityEditor;

public class ScriptUtil
{
    //TODO: check for csharp after the backticks

    public const string NameNotFound = "NameNotFound";

    public static string CleanScript(string inputString)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            string helpBoxMessage = "Input string is null or empty";
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Warning);
            return inputString;
        }
        //Step 1: Remove the term csharp from the script
        string pattern1 = @"csharp|c#";
        string replacement1 = "";
        string outputString1 = Regex.Replace(
            inputString,
            pattern1,
            replacement1,
            RegexOptions.Multiline
        );

        // Step 2: Remove text before backticks
        // Pattern: Match two or more backticks at the start or end and remove before the start and after the end backticks
        string pattern2 = @"^[\s\S]*?(`{2,}[\s\S]*?`{2,})";
        string replacement2 = "$1";

        string outputString2 = Regex.Replace(
            outputString1,
            pattern2,
            replacement2,
            RegexOptions.Multiline
        );
        //Step 3: Remove text after backticks
        // Pattern: Match two or more backticks at the start and end and remove after the end backticks
        string pattern3 = @"(`{2,}[\s\S]*?`{2,})[\s\S]*$";
        string replacement3 = "$1";
        string outputString3 = Regex.Replace(
            outputString2,
            pattern3,
            replacement3,
            RegexOptions.Multiline
        );
        // Step 4: Remove any remaining backtick series of 2 or more
        string pattern4 = @"(`{2,})";
        string replacement4 = "";
        string outputString4 = Regex.Replace(
            outputString3,
            pattern4,
            replacement4,
            RegexOptions.Multiline
        );
        return outputString4;
    }

    public string RemoveTagsFromScript()
    {
        return "";
    }

    public static string ExtractNameAfterKeyWordFromScript(string scriptString, string keyword)
    {
        //checks for the word after the first void keyword and uses it as the function name

        string pattern = GetWordAfterKeyWordPattern(keyword);
        Match match = Regex.Match(scriptString, pattern);
        if (match.Success)
        {
            string name = match.Groups[1].Value;
            string helpBoxMessage = $"{keyword} name extracted from script: {name}";
            HelpBox.GetInstance().UpdateMessage(helpBoxMessage, MessageType.Info);
            return name;
        }
        else
        {
            return NameNotFound;
        }
    }

    private static string GetWordAfterKeyWordPattern(string keyword)
    {
        // Pattern: Find the keyword and match the word after it
        // \b is a word boundary
        // keyword is the keyword to find
        // \s is a whitespace
        // \w is a the match of any word character
        return @"\b" + keyword + @"\s+(\w+)\b";
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
        string className = ExtractNameAfterKeyWordFromScript(cleanedScript, "class");

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
        else if (className == NameNotFound)
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
