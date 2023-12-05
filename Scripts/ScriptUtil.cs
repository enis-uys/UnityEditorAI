using System.Text.RegularExpressions;

using UnityEditor;

/// <summary>
/// The class that contains utility methods for script editing.
/// </summary>
public class ScriptUtil
{
    /// <summary> A const that will be used for scripts where the name was not found </summary>
    public const string NameNotFound = "NameNotFound";

    /// <summary> Cleans the script string to remove unwanted characters.
    /// Within the OpenAI Api these are backticks (```) and the term csharp as well as additional comments outside the code.
    /// </summary>
    /// <param name="inputString"> The script string to clean. </param>
    /// <returns> Returns the cleaned script string. </returns>
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
        // Pattern: Match two or more backticks at the start or end and remove before the start of the backticks
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

    /// <summary> Extracts the name after a keyword from a script. This is used for finding class names and function names. </summary>
    /// <param name="scriptString"> The script string to extract the name from. </param>
    /// <param name="keyword"> The keyword to find. </param>
    /// <returns></returns>
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

    /// <summary> Returns the pattern for finding a word after a keyword </summary>
    /// <param name="keyword"> The keyword to search for. </param>
    /// <returns> Returns the regex pattern for finding a word after a keyword. </returns>
    private static string GetWordAfterKeyWordPattern(string keyword)
    {
        // Pattern: Find the keyword and match the word after it
        // \b is a word boundary
        // keyword is the keyword to find
        // \s is a whitespace
        // \w is a the match of any word character
        return @"\b" + keyword + @"\s+(\w+)\b";
    }

    /// <summary>
    /// Checks if the message is in the format "User: message" or "System: message"
    /// </summary>
    /// <param name="message"> The message to check. </param>
    /// <returns> Returns true if the message is in the format "User: message" or "System: message", false otherwise.</returns>
    /// NOTE: This method is not used anymore, but is kept for future use.
    public static bool IsValidMessageFormat(string message)
    {
        return Regex.IsMatch(message, "^(User|System): .+$");
    }

    /// <summary>
    /// Checks if the script is valid. The scriptString will get cleaned first and is valid if it is not null or empty and contains a class name.
    /// </summary>
    /// <param name="scriptString"> The script string to check. </param>
    /// <returns> Returns true if the script is valid, false otherwise. </returns>
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
