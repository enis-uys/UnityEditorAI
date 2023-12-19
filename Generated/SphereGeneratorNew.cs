using UnityEngine;

public class SphereGeneratorNew : MonoBehaviour
{
    [SerializeField]
    private GameObject spherePrefab; // Assign a prefab of the sphere in the inspector

    [SerializeField]
    private Transform spheresParent; // Assign the parent transform in the inspector

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
        if (spherePrefab != null && spheresParent != null)
        {
            // Instantiate a new sphere as a child of the assigned parent
            GameObject sphere = Instantiate(spherePrefab, spheresParent);
            // Optional: Randomize the position of the sphere within a certain range if needed
            sphere.transform.position =
                spheresParent.position
                + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            // Add a Rigidbody component to make the sphere subject to gravity
            sphere.AddComponent<Rigidbody>().useGravity = true;
            // Destroy the sphere after a certain amount of time
            Destroy(sphere, 5f); // You can make this a serialized field if you want it configurable
        }
        else
        {
            Debug.LogError("Sphere prefab or parent transform is not assigned.");
        }
    }
}
