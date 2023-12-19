
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
    public int numberOfSpheres = 10;
    public float destroyTime = 5f; // Time after which the sphere will be destroyed

    void Start()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.AddComponent<Rigidbody>().useGravity = true;
            Destroy(sphere, destroyTime);
        }
    }
}
