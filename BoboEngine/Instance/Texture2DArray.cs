using StbImageSharp;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture2DArray : Texture
{
    public ImageResult[] images;

    public Texture2DArray(TextureSampleType sampleType = 0)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D_ARRAY;
        this.sampleType = sampleType;

        LoadNullTexture();
    }
    public Texture2DArray(ImageResult image, string name, TextureSampleType sampleType = 0)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D_ARRAY;
        this.sampleType = sampleType;

        images = [image];
        this.name = name;

        loaded = true;
        UpdateImageProperties();
    }
    public Texture2DArray(ImageResult[] images, string name, TextureSampleType sampleType = 0)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D_ARRAY;
        this.sampleType = sampleType;

        this.images = images;
        this.name = name;

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
        name = Path.GetFileNameWithoutExtension(filePath);
        var image = ReadImageFile(filePath, ColorComponents.RedGreenBlueAlpha);

        if (image == null) return false;

        images = [image];
        
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
