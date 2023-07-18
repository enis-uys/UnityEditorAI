using System.Collections;

public class OpenAiStandardPrompts
{
    public static string CreateNewBaseScriptPrompt(string inputPrompt)
    {
        // return
        // @"Create a C# script that rotates a GameObject over time.";
        // use verbatim string for easier editing
        //TODO: Watch the gpt course again for better output formatting []<>, etc.
        return @"
        Your expertise in AI enables you to generate custom Unity C# scripts that fulfills the user's specific requirements.
        Upon receiving the user's input with the description, 
        your script generation process should carefully considering the user's request and ensuring it fulfills their all requirements.
        This is an example. For an input a user gave you and the output you should create with that you could have created. 
        You get the task you have to do, the input and the output you should return.
        Example 1:
        Input: '[Input]`Rotate a gameObject each frame.`'
        Output: '[OUTPUT]`
                using UnityEngine;

                public class RotateByTime : MonoBehaviour
                {
                    // The rotation speed in degrees per second
                    public float rotationSpeed = 30f;
                
                    // Update is called once per frame
                    void Update()
                    {
                        float rotationAmount = rotationSpeed * Time.deltaTime;
                        transform.Rotate(Vector3.up, rotationAmount);
                    }
                }`'
        Example 2:
        Input: '[Input]`Empty Class`'
        Output: '[OUTPUT]`
                using UnityEngine;

                public class EmptyClass
                {
                    //This is an empty class without content.
                }`'                
                
        Note:         
        Only respond with the script part inside [OUTPUT]`script`. Do not include the [OUTPUT]` and `.
        When providing your script, ensure to include all necessary imports 
        and gracefully handle any errors that the user may have inadvertently introduced. Fix errors if the user wrote some.
        In situations where you encounter difficulty in code generation, gracefully create a working code without functions or attributes
        including appending a C Sharp comment explaining the reasons behind this decision.

        This is the `prompt` you should use for the task:
        
        " + inputPrompt;
    }

    private static string EditScriptExamplePrompt()
    {
        // Not implemented yet
        //TODO: implement this
        return @"
        Your expertise in AI enables you to update Unity C# scripts to fulfills the user's specific requirements.
        Upon receiving the user's input with the description, and the source code of the script, 
        your script generation process should carefully considering the user's request and ensuring the new script fulfills their all requirements.
        This is an example task. You get the task you have to do, the input and the output you should return.
        Task: ''
        Input: ''
        Output: ''
        Only respond with the '[Output]<output>'.
        When providing your script, ensure to include all necessary imports 
        and gracefully handle any errors that the user may have inadvertently introduced. Fix errors if the user wrote some and remove unneccesary code.
        In situations where you encounter difficulty in code generation, gracefully create a working code without functions or attributes
        including appending a C Sharp comment explaining the reasons behind this decision.
   
        ";
    }
}
