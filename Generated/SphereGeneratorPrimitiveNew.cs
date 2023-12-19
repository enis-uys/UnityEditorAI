
using UnityEngine;

public class SphereGeneratorPrimitiveNew : MonoBehaviour
{
    [SerializeField]
    private float spawnRate = 1f; // Rate at which spheres will spawn
    private float nextSpawnTime;

    // Update is called once per frame
    void Update()
    {
        // Check if it's time to spawn a new sphere
        if (Time.time > nextSpawnTime)
        {
            SpawnSphere();
            // Set the time for the next spawn
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnSphere()
    {
        // Create a primitive sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Set the script's gameObject as the parent
        sphere.transform.SetParent(transform, false);
        // Optional: Randomize the position of the sphere within a certain range if needed
        sphere.transform.localPosition =
            new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        // Add a Rigidbody component to make the sphere subject to gravity
        sphere.AddComponent<Rigidbody>().useGravity = true;
        // Destroy the sphere after a certain amount of time
        Destroy(sphere, 5f); // You can make this a serialized field if you want it configurable
    }
}
