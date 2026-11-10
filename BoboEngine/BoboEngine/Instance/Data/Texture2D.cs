using StbImageSharp;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture2D : Texture
{
    public ImageResult image;

    public Texture2D(TextureSampleType sampleType = 0)
    {
        this.sampleType = sampleType;

        LoadNullTexture();
    }
    public Texture2D(string filePath, string name, TextureSampleType sampleType = 0)
    {
        this.sampleType = sampleType;
        this.name = name;

        if (!LoadImageFile(filePath)) // Failed to load texture
        {
            LoadNullTexture();
        }
    }

    protected override int GetGL_TEXTURE_TYPE() => GL_TEXTURE_2D;
    protected override void UpdateImageProperties()
    {
        if (loaded == false) return;

        width = image.Width;
        height = image.Height;
    }
    public override bool LoadImageFile(string filePath)
    {
        name = Path.GetFileNameWithoutExtension(filePath);
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