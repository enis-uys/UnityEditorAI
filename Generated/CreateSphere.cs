using UnityEngine;

public class CreateSphere : MonoBehaviour
{
    // The radius of the sphere
    public float radius = 1f;
    
    // The material of the sphere
    public Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        // Create a new sphere GameObject
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // Set the position and scale of the sphere
        sphere.transform.position = transform.position;
        sphere.transform.localScale = new Vector3(radius, radius, radius);
        
        // Set the material of the sphere
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material = material;
    }
}