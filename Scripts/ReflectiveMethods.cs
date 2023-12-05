using System;

using UnityEditor;
using System.Reflection;

public class ReflectiveMethods
{
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
}
