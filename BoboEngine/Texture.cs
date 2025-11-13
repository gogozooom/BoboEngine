//using StbImageSharp;
using System.Runtime.InteropServices;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture
{
    //public ImageResult imageR { get; private set; }
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

    public Texture(Float3[,] image)
    {
        this.image = image;
        loaded = true;

        UpdateImageProperties();
    }

    void LoadNullTexture()
    {
        LoadImageFile(Program.GetLocalTexturePath("NULL"));
    }
    void UpdateImageProperties()
    {
        /*
        width = imageR.Width;
        height = imageR.Height;
        wscale = width - 1;
        hscale = height - 1;
        */
        if (image == null) return;

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

        /*
        using (var stream = File.OpenRead(filePath))
        {
            imageR = ImageResult.FromStream(stream);
        }
        */

        //* Manual BMP importer
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

        if (width <= 2 || height <= 2)
        {
            Program.LogWarning($"Texture is very small, may look buggy! '{filePath}'");
        }

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
        //*/

        loaded = true;
        UpdateImageProperties();

        return true;
    }

    private uint textureRef;
    public void BindOpenGL()
    {
        if (!WindowManager.Initialized)
        {
            Program.LogError("Cannot bind open gl without a window!");
            return;
        }

        if (!loaded)
        {
            Program.LogError("Cannot bind open gl without a loaded texture!");
            return;
        }

        // OpenGl Time

        byte[] texData = new byte[width * height * 3];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixelData = image[x, y];

                texData[(y * width + x) * 3]     = (byte)(pixelData.r * 255);
                texData[(y * width + x) * 3 + 1] = (byte)(pixelData.g * 255);
                texData[(y * width + x) * 3 + 2] = (byte)(pixelData.b * 255);


                //texData[(y * width + x) * 3] =     255;
                //texData[(y * width + x) * 3 + 1] = 255;
                //texData[(y * width + x) * 3 + 2] = 255;
            }
        }

        GCHandle gCHandle = GCHandle.Alloc(texData, GCHandleType.Pinned);
        IntPtr textureDataPointer = gCHandle.AddrOfPinnedObject();

        textureRef = glGenTexture();

        glBindTexture(GL_TEXTURE_2D, textureRef);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

        try
        {
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, textureDataPointer);
            glGenerateMipmap(GL_TEXTURE_2D);
        }
        catch (Exception e)
        {
            Program.LogError("Failed to bind texture to OpenGL!");
            Program.LogError("--- Exception: \n" + e.Message);
            Program.LogError("--- Inner Exception: \n" + e.InnerException);
        }
        finally
        {
            gCHandle.Free();
        }

        glUniform1i(glGetUniformLocation(textureRef, "mainTexture"), 0);
    }

    public void BindTexture()
    {
        if (textureRef == 0) BindOpenGL();

        glBindTexture(GL_TEXTURE_2D, textureRef);
    }
    public void UnbindTexture()
    {
        glBindTexture(GL_TEXTURE_2D, 0);
    }
    public void Delete()
    {
        if (textureRef == 0) return;

        glDeleteTexture(textureRef);
        textureRef = 0;
    }

    /*
    public string GetNumberOfSpaces(int number)
    {
        string output = "";

        for (int i = 0; i < number; i++)
        {
            output += " ";
        }

        return output;
    }
    */
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
