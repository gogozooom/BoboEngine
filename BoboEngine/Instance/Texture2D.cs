using StbImageSharp;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture2D : Texture
{
    public ImageResult image;

    public Texture2D(TextureSampleType sampleType = 0)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D;
        this.sampleType = sampleType;

        LoadNullTexture();
    }
    public Texture2D(string filePath, TextureSampleType sampleType = 0)
    {
        GL_TEXTURE_TYPE = GL_TEXTURE_2D;
        this.sampleType = sampleType;

        if (!LoadImageFile(filePath)) // Failed to load texture
        {
            LoadNullTexture();
        }
    }

    protected override void UpdateImageProperties()
    {
        if (loaded == false) return;

        width = image.Width;
        height = image.Height;
    }
    public override bool LoadImageFile(string filePath)
    {
        var image = ReadImageFile(filePath, ColorComponents.RedGreenBlueAlpha);

        if (image == null) return false;

        this.image = image;

        loaded = true;
        UpdateImageProperties();
        return true;
    }
    public override void BindOpenGL()
    {
        if(!PreBindOpenGL()) return;

        PostBindOpenGL(image.Data);
    }
}