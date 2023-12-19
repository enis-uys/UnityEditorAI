using UnityEngine;

public class ColorScriptDemo : MonoBehaviour
{
    /// <summary>
    /// This class was written by GPT-3.5-Turbo
    /// </summary>
    public static void GenerateColors()
    {
        ColorArrayObject colorArrayObject = ColorExtruder.ColorArrayObjectFromFile();

        GameObject findParent = GameObject.Find("ImageParent");
        if (findParent != null)
        {
            DestroyImmediate(findParent);
        }
        GameObject imageParent = new GameObject("ImageParent");

        for (int i = 0; i < colorArrayObject.width * colorArrayObject.height; i++)
        {
            int x = i % colorArrayObject.width;
            int y = i / colorArrayObject.width;
            Vector3 position = new(x, y, colorArrayObject.width);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one;
            sphere.transform.parent = imageParent.transform;
            int colorIndex = colorArrayObject.pixels[i];
            Color newColor;
            if (ColorUtility.TryParseHtmlString(colorArrayObject.colors[colorIndex], out newColor))
            {
                Renderer renderer = sphere.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Standard")) { color = newColor };
            }
        }
    }
}
