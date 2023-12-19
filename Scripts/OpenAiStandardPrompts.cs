/// <summary>
/// This file contains the standard prompts that will get loaded into the extension in some cases.
/// It makes use of verbatim string for easier editing
/// </summary>
public class OpenAiStandardPrompts
{
    //This part is partly adapted from Kenjiro AICommand (AICommandWindow.cs)
    /// <Availability> https://github.com/keijiro/AICommand/ </Availability>
    ///<License> Unlicense (Public Domain) View LICENSE.md to see the license and information. </License>
    ///<Description>
    /// AICommand is a Unity extension that experiment with a command window for executing C# scripts from the gpt api.
    /// </Description>
    /// <summary>
    /// The prompt for the user to generate a new script that will be invoked immediately. It is intended for object generation.
    /// </summary>
    //Inspired by the prompt from AICommand but changed the whole prompt to be more precise. Included an example for better output.
    public static readonly (string Title, string Content) ObjectGenerationPrompt = (
        "Object Generation Prompt",
        @"Write an Unity Editor script. The script will be invoked immediately to create new game objects within Unity.
        1. Create a single class containing only one static void method.
        2. Provide all necessary libraries and using statements especially UnityEngine.
        3. All variables used within the method must be initialized within the method itself.
        4. Do not use MenuItem, WindowItem or EditorWindow, Serializable or other custom attributes . Only write the static method inside the class.
        5. Do not use GameObject.Find* functions and do not rely on selected objects. Instead, find game objects manually within the method.
        7. I only need the script body. Do not add any explanation.
        8. Ensure that the script stays to the single-class, single-method format and fully initializes all variables.
        9. If no explicit values for variables are provided, use values that are commonly used in Unity.
        10. I give you an example of a task and the expected output:
        
        Example:
        - Task: Create a new game object with a purple sphere at position (3, 1, 0) with a scale of 2.
        - Expected Output:
        ```
        using UnityEngine;
            public class GeneratePurpleSphere
            {
                public static void Generate() 
                {   
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = new Vector3(3, 1, 0);
                    sphere.transform.localScale = new Vector3(2, 2, 2);
                    Renderer sphereRenderer = sphere.GetComponent<Renderer>();
                    sphereRenderer.material.color = Color.magenta;
                }
            }
        ```
        11. The task is described as follows:
      "
    );

    //End of adapted part from Kenjiro AICommand.
    /// <summary> The prompt for the user to generate a new script that the user can check before saving it. </summary>
    public static readonly (string Title, string Content) CreateNewScriptWithPrompt = (
        "Create New Script With Prompt",
        @"
            You are tasked with creating custom Unity C# scripts based on specific requirements provided by users. 
            For each user request, analyze the description and generate a script that accurately fulfills all outlined needs. 
            Below are examples of user inputs followed by the corresponding script outputs you should generate.
            Please ensure your script outputs adhere to this format and meet the user requirements as closely as possible.

            Example 1:
            - User Input: 'Rotate a GameObject each frame in Unity.'
            - Expected Output:

        ```
            using UnityEngine;
            public class RotateGameObject : MonoBehaviour
            {
            public float rotationSpeed = 30f; // Rotation speed in degrees per second
              void Update()
              {
                  // Rotate the GameObject by the specified rotation amount each frame
                  transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
              }
            }
        ```

            Example 2:
            - User Input: 'Create an empty class in Unity.'
            - Expected Output:
        ```
            using UnityEngine;
            public class EmptyClass: MonoBehaviour
            {
            // This class is intentionally left empty.
            }
        ```
            "
    );

    /// <summary> The prompt for the user to update an existing script with a prompt </summary>
    public static readonly (string Title, string Content) UpdateExistingScriptWithPrompt = (
        "Update Existing Script With Prompt",
        @"You are tasked with updating existing Unity C# scripts based on specific requirements provided by users.
        When updating Unity C# scripts as per user requests, ensure that each new class name is distinct from the original. 
        If the context remains the same, append 'New' to the original class name to indicate the updated version. 
        Below are examples illustrating how to process the user input and produce the necessary script output.

        Example 1:
        - Task: Update the rotation speed to a default value of 60 and make it serializable.
        - User Input Script:

        ```        
        using UnityEngine;
        public class RotateByTime : MonoBehaviour
        {
            void Update()
            {
                float rotationAmount = 30f * Time.deltaTime;
                transform.Rotate(Vector3.up, rotationAmount);
            }
        }   
        ```

        - Expected Output:

        ```
        using UnityEngine;

        public class RotateByTimeNew : MonoBehaviour   
        {
            [SerializeField]
            private float rotationSpeed = 60f; // The default rotation speed, now serializable


            void Update()
            {
                 float rotationAmount = rotationSpeed * Time.deltaTime;
                 transform.Rotate(Vector3.up, rotationAmount);
            }
        }
        ```

        Example 2:

        - Task: Remove all `Debug.Log` statements from the script and add useful comments.
        - User Input Script:

        ```
        using UnityEngine;

            public class EmptyClass : MonoBehaviour
            {
               public void Start()
                {
                  Debug.Log(""Hello World"");
                }

              public void Update()
                {
                  Debug.Log(""Hello World"");
                }
            }
        ```

        - Expected Output:
        ```
            using UnityEngine;

            public class EmptyClassNew : MonoBehaviour
            {
                public void Start()
                {
                // Start is now empty, previously contained a Debug.Log statement
                }
                public void Update()
                {
                // Update is now empty, previously contained a Debug.Log statement
                }
            }
        ```
        Please format your responses according to these examples, ensuring that class names are unique and that the output is in line with the input task requirements."
    );

    /// <summary> The prompt that will be appended to the end of the script generation prompts. It includes more detailled information about the task. </summary>
    public static readonly (string Title, string Content) ScriptEndNote = (
        "Script End Note",
        @" Note:
        1. Don't include any explanations in your responses. Only include the script body.
        2. When providing your script, ensure to include all necessary imports and using.
        3. Fix errors if the user wrote some.
        4. In situations where you encounter difficulty in code generation,  create a working code without functions or attributes 
        including appending a C Sharp comment explaining the reasons behind this decision.
        5. This is the `prompt`and/or the `script` you should use for the task:
        "
    );

    /// <summary> The prompt for the user to generate a 3d image generation script. </summary>
    public static readonly (string Title, string Content) ColorImageGenerationPrompt = (
        "Generate Color Image From Data Prompt",
        @"Create a C# Unity script that turns image data into a 3D grid of colored spheres, one for each pixel starting from bottom left.
        The generated spheres should start at 0,0,0 and be spaced 1 unit apart and have the size of 1.
        It should create primitive spheres and not use any prefabs. Then it should color each sphere according to the pixel color.
        It should spawn the spheres inside a new generated ParentObject.
        The Method ColorExtruder.ColorArrayObjectFromFile(); reads from a file out of the EditorPrefs. It does not need any parameters.
        It returns a ColorArrayObject with image size and colors in this format:
        ```csharp
            public class ColorArrayObject {
            public int width, height;
            public List<string> colors;
            public List<int> pixels;
        }
        ``` 
        The colors are stored as strings in the format #RRGGBBAA.
        Write a own method to parse a hex color string to a Color object.
        The pixels List store the index of the color in the colors list.
        Make sure to to use the ColorExtruder.ColorArrayObjectFromFile(); method to get the data. 
        The script should be called by a single static method from EditorMenu.
       "
    );

    //(This is an example string to reuse)
    // private static readonly (string Title, string Content) NewPromptTemplate = ("", @"");

    /// <summary> An example prompt that should help the user to improve their script. </summary>
    public static readonly (string Title, string Content) ImproveScriptPrompt = (
        "Improve/Improve script",
        @"Improve the script by adding comments and removing unused variables."
    );

    /// <summary> An example prompt that should help the user to improve their script by adding comments. </summary>
    public static readonly (string Title, string Content) WriteCommentsPrompt = (
        "Improve/Write Comments",
        @"Write comments for the script.
        The comments should be useful and explain the code.
        The comments should be written in English."
    );

    /// <summary> An example prompt that should help the user to improve their script by removing unused variables. </summary>
    public static readonly (string Title, string Content) RemoveVariablesPrompt = (
        "Improve/Remove unused variables",
        @"Remove unused variables from the script."
    );

    /// <summary> An example prompt that should help the user to improve their script by removing Debug.Log() calls. </summary>
    public static readonly (string Title, string Content) RemoveDebugLogsPrompt = (
        "Improve/Remove Debug Logs",
        @"Remove all Debug.Log() calls from the script."
    );

    /// <summary> An example prompt that should help the user to improve their script by auto-generating serialization. </summary>
    public static readonly (string Title, string Content) AutoGenerateSerializationPrompt = (
        "Improve/Auto-Generate Serialization",
        @"Auto-generate serialization for the script."
    );

    /// <summary> An example prompt that should generate a script that rotates a game object over time. </summary>
    public static readonly (string Title, string Content) GenerateRotationScriptPrompt = (
        "Improve/Generate a rotation script",
        @"Generate a script that rotates a game object over time."
    );

    /// <summary> An example prompt that should generate a script that generates a new game object with a particle system. </summary>
    public static readonly (string Title, string Content) GenerateParticleSystemPrompt = (
        "Generate/Generate Particle System",
        @"Generate a Unity script that creates new game objects with particle systems. Do not use any prefabs."
    );

    /// <summary> An example prompt that should generate a script that generates a directional light at a specified position. </summary>
    public static readonly (string Title, string Content) GenerateLightsPrompt = (
        "Generate/Generate Lights",
        @"Generate a Unity script that spawns directional lights at specified positions in the scene. Do not use any prefabs."
    );
}
