namespace BoboEngine;
public class Texture
{
    public Float3[,] image { get; private set; }
    public bool loaded { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }

    int wscale;
    int hscale;

    public Texture()
    {
        LoadNullTexture();
    }

    public Texture(string filePath)
    {
        if (!LoadImageFile(filePath)) // Failed to load texture
        {
            LoadNullTexture();
        }
    }

    void LoadNullTexture()
    {
        LoadImageFile(Program.GetLocalTexturePath("NULL"));
    }
    void UpdateImageProperties()
    {
        width = image.GetLength(0);
        height = image.GetLength(1);
        wscale = width - 1;
        hscale = height - 1;
    }

    readonly byte[] bmpHeader = [66, 77];
    public bool LoadImageFile(string filePath)
    {
        // -- Error Checks --
        if (string.IsNullOrEmpty(filePath))
        {
            Program.LogError($"Please specify a file to load!");
            return false;
        }
        if (!File.Exists(filePath))
        {
            Program.LogError($"Could find model file '{filePath}'");
            return false;
        }

        byte[] data = File.ReadAllBytes(filePath);

        byte[] header = data[..2];

        // File Type Check
        if (!header.SequenceEqual(bmpHeader))
        {
            Program.LogError($"File is not in BMP format! '{filePath}'");
            return false;
        }

        uint imageSize = BitConverter.ToUInt32(data[34..38]);

        uint width = BitConverter.ToUInt32(data[18..22]);
        uint height = BitConverter.ToUInt32(data[22..26]);

        Float3[,] image = new Float3[width, height];

        uint whiteSpace = (imageSize - (width * height * 3)) / width;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float b = (float)data[54 + 3 * x + (width * 3 + whiteSpace) * y] / 255; // 54 = image starting index 
                float g = (float)data[55 + 3 * x + (width * 3 + whiteSpace) * y] / 255;
                float r = (float)data[56 + 3 * x + (width * 3 + whiteSpace) * y] / 255;

                image[x, y] = new(r, g, b);
            }
        }

        this.image = image;

        loaded = true;
        UpdateImageProperties();
        return true;
    }
    public string GetNumberOfSpaces(int number)
    {
        string output = "";

        for (int i = 0; i < number; i++)
        {
            output += " ";
        }

        return output;
    }
    public Float3 Sample(Float2 texCoord)
    {
        // Render Nearest Neighbor

        float xMap = texCoord.x;
        float yMap = texCoord.y;

        // Texture Wrapping X

        if (xMap > 1f)
        {
            xMap -= MathF.Floor(xMap);
        }
        else if (xMap < 0f)
        {
            xMap += MathF.Floor(-xMap) + 1;
        }

        // Texture Wrapping Y
        if (yMap > 1f)
        {
            yMap -= MathF.Floor(yMap);
        }
        else if (yMap < 0f)
        {
            yMap += MathF.Floor(-yMap) + 1;
        }

        int x = (int)(xMap * wscale);
        int y = (int)(yMap * hscale);

        return image[x, y];
    }
}
