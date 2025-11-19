using static OpenGL.GL;

namespace BoboEngine;
public class Texture2D : Texture
{
    public Float3[,] image;

    public Texture2D()
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D;

        LoadNullTexture();
    }
    public Texture2D(string filePath)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D;

        if (!LoadImageFile(filePath)) // Failed to load texture
        {
            LoadNullTexture();
        }
    }

    protected override void UpdateImageProperties()
    {
        if (loaded == false) return;

        width = image.GetLength(0);
        height = image.GetLength(1);
    }
    public override bool LoadImageFile(string filePath)
    {
        var image = ReadImageFile(filePath);

        if (image == null) return false;

        this.image = image;

        loaded = true;
        UpdateImageProperties();
        return true;
    }
    public override void BindOpenGL()
    {
        if(!PreBindOpenGL()) return;

        byte[] texData = new byte[width * height * 3];

        int textureIndex = 0;
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

        PostBindOpenGL(texData);
    }
}
