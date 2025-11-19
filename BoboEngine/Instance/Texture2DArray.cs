using System.Runtime.InteropServices;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture2DArray : Texture
{
    public List<Float3[,]> images { get; private set; } = new();

    public Texture2DArray()
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D_ARRAY;
        LoadNullTexture();
    }
    public Texture2DArray(Float3[,] image, string name)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D_ARRAY;

        images = [image];
        this.name = name;

        loaded = true;
        UpdateImageProperties();
    }

    public Texture2DArray(List<Float3[,]> images, string name)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D_ARRAY;

        this.images = images;
        this.name = name;

        loaded = true;
        UpdateImageProperties();
    }

    protected override void UpdateImageProperties()
    {
        if (loaded == false) return;

        width = images[0].GetLength(0);
        height = images[0].GetLength(1);
    }

    public override bool LoadImageFile(string filePath)
    {
        var image = ReadImageFile(filePath);

        if (image == null) return false;

        images.Clear();
        
        loaded = true;
        UpdateImageProperties();
        return true;
    }

    public override void BindOpenGL()
    {
        if (!PreBindOpenGL()) return;

        byte[] texData = new byte[width * height * 3 * images.Count];

        int textureIndex = 0;
        foreach (var image in images)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixelData = image[x, y];

                    var offset = (textureIndex * width * height * 3) + (y * width + x) * 3;

                    texData[offset] = (byte)(pixelData.r * 255);
                    texData[offset + 1] = (byte)(pixelData.g * 255);
                    texData[offset + 2] = (byte)(pixelData.b * 255);
                }
            }
            textureIndex++;
        }

        PostBindOpenGL(texData);
    }
}
