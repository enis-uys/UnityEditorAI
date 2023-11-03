using System.Collections.Generic;

[System.Serializable]
public class ColorArrayObject
{
    public int width;
    public int height;

    public List<string> colors = new();
    public List<int> pixels = new();

    public ColorArrayObject(int width, int height, List<string> colors, List<int> pixels)
    {
        this.width = width;
        this.height = height;
        this.colors = colors;
        this.pixels = pixels;
    }

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
