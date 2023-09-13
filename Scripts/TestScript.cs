using UnityEngine;

public class TestScript : MonoBehaviour
{
    //TODO: write better use cases and bad behaviors that can be improved

    // Some variables (may seem unnecessary or poorly structured)
    private int x = 5;
    private int y = 10;
    private int z = 15;

    //    private bool neverUsed = false;
    private bool isMoving = false;

    // Some debug logs (may clutter the console)
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

        if (x < 0)
        {
            Debug.Log("X is negative!");
        }
    }

    // A function with poor naming and unnecessary comments
    private void FunctionA()
    {
        // Some unnecessary comment here
        int temp = x + y;
        Debug.Log("Function A: " + temp);
    }

    // Another function with unclear purpose
    private void FunctionB()
    {
        int result = y - z;
        Debug.Log("Function B: " + result);
    }

    // Function with incorrect parameter naming
    private void FunctionC(int x, int y)
    {
        int sum = x + y;
        Debug.Log("Function C: " + sum);
    }

    // Function with hard-coded values
    private void FunctionD()
    {
        int result = 20;
        Debug.Log("Function D: " + result);
    }
}
