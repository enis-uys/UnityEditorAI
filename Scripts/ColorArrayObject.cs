using System.Collections.Generic;

[System.Serializable]
/// <summary> Class that represents an image as a color array object of colors and  </summary>
public class ColorArrayObject
{
    /// <summary> The width of the image. </summary>
    public int width;

    /// <summary> The height of the image. </summary>
    public int height;

    /// <summary> All different colors of the image. </summary>
    public List<string> colors = new();

    /// <summary> All pixels of the image saved as an index of the colors list. </summary>
    public List<int> pixels = new();

    /// <summary> Constructor of the color array object. </summary>
    /// <param name="width"> The width of the image. </param>
    /// <param name="height"> The height of the image. </param>
    /// <param name="colors"> All unique colors of the image. </param>
    /// <param name="pixels"> All pixels of the image saved as an index of the colors list. </param>
    public ColorArrayObject(int width, int height, List<string> colors, List<int> pixels)
    {
        this.width = width;
        this.height = height;
        this.colors = colors;
        this.pixels = pixels;
    }

    /// <summary> Method that converts a json string to a color array object. </summary>
    /// <param name="jsonData"> The json string that should be converted to a color array object. </param>
    /// <returns> Returns a color array object. </returns>
    public ColorArrayObject ColorArrayObjectFromString(string jsonData)
    {
        if (!string.IsNullOrEmpty(jsonData))
        {
            return FileManager<ColorArrayObject>.DeserializeJsonString(jsonData);
        }
        else
            return null;
    }
}
