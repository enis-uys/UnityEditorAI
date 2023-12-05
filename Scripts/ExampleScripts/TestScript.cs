using UnityEngine;

/// <summary> This script is an intentional demonstration of bad coding practices. </summary>
public class TestScript : MonoBehaviour
{
    //TODO: write better use cases and bad behaviors that can be improved

    // Poorly named and structured variables
    private int a = 5;
    private int b = 10;
    private int c = 15;

    //    private bool neverUsed = false;
    private bool isMoving = false;

    private void Start()
    {
        Debug.Log("Start function called!");
    }

    private void Update()
    {
        if (isMoving)
        {
            Debug.Log("Object is moving!");
        }

        if (a < 0)
        {
            Debug.Log("X is negative!");
        }
    }

    // Poorly named function with redundant code
    private int Func1()
    {
        // Adding a and b
        int sum = a + b;
        return a + b;
    }

    // Unclear function purpose without return value
    private void Func2()
    {
        int result = b - c;
        Debug.Log("Func2: " + result);
        return;
    }

    // Incorrect parameter naming that contains class fields
    private int Func3(int a, int b)
    {
        int sum = a + b;
        Debug.Log("Func3: " + sum);
        return sum;
    }

    // Function with hard-coded values
    private int Func4(int number)
    {
        int value = 20;
        Debug.Log("Func4: " + value);
        return value;
    }
}
