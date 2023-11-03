using UnityEngine;
using System.Collections.Generic;

public class SphereGenerator : MonoBehaviour
{
    public void Start()
    {
        // Read color data from file
        ColorArrayObject colorArrayObject = ColorExtruder.ColorArrayObjectFromFile();

        // Create parent object
        GameObject parentObject = new GameObject("ParentObject");

        // Iterate through each pixel
        for (int i = 0; i < colorArrayObject.pixels.Count; i++)
        {
            // Get pixel color index
            int colorIndex = colorArrayObject.pixels[i];

            // Get pixel color
            string colorHex = colorArrayObject.colors[colorIndex];
            Color color = ParseHexColor(colorHex);

            // Calculate position of sphere
            int x = i % colorArrayObject.width;
            int y = i / colorArrayObject.width;
            Vector3 position = new Vector3(x, y, 0);

            // Create sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one;
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.transform.parent = parentObject.transform;
        }
    }

    private static Color ParseHexColor(string hexColor)
    {
        hexColor = hexColor.Replace("#", "");
        byte r = byte.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = byte.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, a);
    }
}
