using StbImageSharp;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture2DArray : Texture
{
    public ImageResult[] images;

    protected override int GetGL_TEXTURE_TYPE() => GL_TEXTURE_2D_ARRAY;

    public Texture2DArray(TextureSampleType sampleType = 0)
    {
        this.sampleType = sampleType;

        LoadNullTexture();
    }
    public Texture2DArray(ImageResult image, string name, TextureSampleType sampleType = 0)
    {
        this.sampleType = sampleType;
        this.name = name;

        images = [image];

        loaded = true;
        UpdateImageProperties();
    }
    public Texture2DArray(ImageResult[] images, string name, TextureSampleType sampleType = 0)
    {
        this.sampleType = sampleType;
        this.name = name;

        this.images = images;

        loaded = true;
        UpdateImageProperties();
    }
    /// <summary>
    /// Loads in a sprite sheet
    /// </summary>
    /// <param name="width">The width in pixels of each sprite</param>
    /// <param name="height">The height in pixels of each sprite</param>
    public Texture2DArray(string filePath, string name, int width, int height, TextureSampleType sampleType = 0)
    {
        this.sampleType = sampleType;
        this.name = name;

        LoadImageFile(filePath, width, height);

        loaded = true;
        UpdateImageProperties();
    }

    protected override void UpdateImageProperties()
    {
        if (loaded == false) return;

        width = images[0].Width;
        height = images[0].Height;
    }

    public override bool LoadImageFile(string filePath)
    {
        var image = ReadImageFile(filePath, ColorComponents.RedGreenBlueAlpha);

        if (image == null) return false;

        images = [image];

        loaded = true;
        UpdateImageProperties();
        return true;
    }
    public bool LoadImageFile(string filePath, int width, int height)
    {
        var image = ReadImageFile(filePath, ColorComponents.RedGreenBlueAlpha);

        if (image == null) return false;

        int spritesAlongX = image.Width/width;
        int spritesAlongY = image.Height/height;

        if(spritesAlongX != (float)image.Width/width || spritesAlongY != (float)image.Height/height)
        {
            Engine.LogError($"Could not evenly load image '({image.Width},{image.Height})' with width and height of ({width},{height})");
            return false;
        }

        var imageResults = new List<byte[]>();

        images = new ImageResult[spritesAlongX*spritesAlongY];

        for (int i = 0; i < images.Length; i++)
        {
            images[i] = new ImageResult();

            images[i].SourceComp = image.SourceComp;
            images[i].Comp = image.Comp;
            images[i].Width = width;
            images[i].Height = height;

            imageResults.Add(new byte[width*height*4]);
        }

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                // Flip Y Order so it goes top left to bottom right
                int imageIndex = x / width + (image.Height-y-1) / height * spritesAlongX;

                int dataIndex = 4 * (x + width * spritesAlongX * y);
                int localDataIndex = 4 * (Maths.Mod(x, width) + width * Maths.Mod(y, height));

                var r = image.Data[dataIndex];
                var g = image.Data[dataIndex + 1];
                var b = image.Data[dataIndex + 2];
                var a = image.Data[dataIndex + 3];

                imageResults[imageIndex][localDataIndex] = r;
                imageResults[imageIndex][localDataIndex + 1] = g;
                imageResults[imageIndex][localDataIndex + 2] = b;
                imageResults[imageIndex][localDataIndex + 3] = a;
            }
        }

        for (int i = 0; i < images.Length; i++)
        {
            images[i].Data = imageResults[i];
        }

        loaded = true;
        UpdateImageProperties();
        return true;
    }

    public override void BindOpenGL()
    {
        if (!PreBindOpenGL()) return;

        byte[] texData = [];

        foreach (var image in images)
        {
            texData = texData.Concat(image.Data).ToArray();
        }

        PostBindOpenGL(texData);
    }

}
