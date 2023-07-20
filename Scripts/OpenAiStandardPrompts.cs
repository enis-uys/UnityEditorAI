using System.Collections;

public class OpenAiStandardPrompts
{
    public static string CreateNewScriptWithPrompt(string inputPrompt)
    {
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
        "
            + EndNote()
            + inputPrompt;
    }

    public static string UpdateExistingScriptWithPrompt(string inputPrompt, string inputScript)
    {
        // Not implemented yet
        //TODO: implement this
        return @"
        Your expertise in AI enables you to update Unity C# scripts to fulfills the user's specific requirements.
        Upon receiving the user's input with the description, and the source code of the script, 
        your script generation process should carefully considering the user's request and ensuring the new script fulfills their all requirements.
        Make sure that the new class name is different than the other. If the context is the same just add an 'New' to the class name.
        This is an example task. You get the task you have to do, the input and the output you should return.
       
       
 Example 1:
        Input: '[Input]`Update the rotation speed to a default value of 60 and make it Serializable`'
        Input: '[Input Script]`
                using UnityEngine;

                public class RotateByTime : MonoBehaviour
                {
                    // Update is called once per frame
                    void Update()
                    {
                        float rotationAmount = 30f * Time.deltaTime;
                        transform.Rotate(Vector3.up, rotationAmount);
                    }
                }`'
        Output: '[OUTPUT]`
                using UnityEngine;

                public class RotateByTimeNew : MonoBehaviour
                {
                    // The rotation speed in degrees per second
                    [SerializeField]
                    public float rotationSpeed = 60f;
                
                    // Update is called once per frame
                    void Update()
                    {
                        float rotationAmount = rotationSpeed * Time.deltaTime;
                        transform.Rotate(Vector3.up, rotationAmount);
                    }
                }`'
        Example 2:
        Input Prompt: '[Input]`Remove the Debug.Logs from the script and include useful comments.`'
        Input Script: '[Input Script]`
                using UnityEngine;

                public class EmptyClass: MonoBehaviour
                {
                    public void Start()
                    {
                        Debug.Log(""Hello World"");
                    }

                    public void Update()
                    {
                        Debug.Log(""Hello World"");
                    }
                }`'
        Output: '[OUTPUT]`
                using UnityEngine;

                public class EmptyClassNew: MonoBehaviour
                {
                    // This gets called when the object is created
                    public void Start()
                    {
                    }
                    //This gets called every frame
                    public void Update()
                    {
                    }
                }`'
        "
            + EndNote()
            + inputPrompt
            + "\n"
            + inputScript;
    }

    private static string EndNote()
    {
        return @" Note:         
        Only respond with the script part inside [OUTPUT]`script`. 
        Make sure to not include unnecessary symbols because your output will be transformed to a C# class.
        Do not include the [OUTPUT] and symbols like '`. 
        When providing your script, ensure to include all necessary imports 
        and gracefully handle any errors that the user may have inadvertently introduced. Fix errors if the user wrote some.
        In situations where you encounter difficulty in code generation, gracefully create a working code without functions or attributes
        including appending a C Sharp comment explaining the reasons behind this decision.

        This is the `prompt`and/or the `script` you should use for the task:
        ";
    }
}
