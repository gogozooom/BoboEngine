using StbImageSharp;
using System.Runtime.InteropServices;
using static OpenGL.GL;

namespace BoboEngine;
public abstract class Texture
{
    public string name { get; internal set; }
    public bool loaded { get; internal set; }
    public int width { get; internal set; }
    public int height { get; internal set; }

    protected TextureSampleType sampleType; 

    protected void LoadNullTexture()
    {
        LoadImageFile(Program.GetLocalTexturePath("NULL"));
    }

    protected abstract void UpdateImageProperties();

    protected readonly byte[] bmpHeader = [66, 77];
    public static ImageResult ReadImageFile(string filePath, ColorComponents colorComponents = ColorComponents.Default)
    {
        // -- Error Checks --
        if (string.IsNullOrEmpty(filePath))
        {
            Program.LogError($"Please specify a file to load!");
            return null;
        }
        if (!File.Exists(filePath))
        {
            Program.LogError($"Could find texture file '{filePath}'");
            return null;
        }

        using (var stream = File.OpenRead(filePath))
        {
            var result = ImageResult.FromStream(stream, colorComponents);

            return result;
        }

        /* Manual BMP importer

        data = File.ReadAllBytes(filePath);

        byte[] header = data[..2];

        // File Type Check
        if (!header.SequenceEqual(bmpHeader))
        {
            Program.LogError($"File is not in BMP format! '{filePath}'");
            return null;
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

        //*/
    }

    public abstract bool LoadImageFile(string filePath);

    protected uint textureRef;
    public abstract void BindOpenGL();

    protected bool PreBindOpenGL()
    {
        if (!WindowManager.Initialized)
        {
            Program.LogError("Cannot bind open gl without a window!");
            return false;
        }

        if (!loaded)
        {
            Program.LogError("Cannot bind open gl without a loaded texture!");
            return false;
        }

        return true;
    }
    protected void PostBindOpenGL(byte[] texData)
    {
        GCHandle gCHandle = GCHandle.Alloc(texData, GCHandleType.Pinned);
        IntPtr textureDataPointer = gCHandle.AddrOfPinnedObject();

        textureRef = glGenTexture();

        glBindTexture(GL_TEXTURE_TYPE, textureRef);

        /* (Context - Minecraft):
            * GL_LINEAR:
                X Do not use, looks bad with pixel art textures
            * GL_NEAREST:
                - Full resolution constantly 
                X Causes pixely artifacts far away

            * GL_NEAREST_MIPMAP_NEAREST: 
                - takes the nearest mipmap to match the pixel size and uses nearest neighbor interpolation for texture sampling.
                X Causes noticable pink haze on ground far away
            * GL_LINEAR_MIPMAP_NEAREST: 
                - takes the nearest mipmap level and samples that level using linear interpolation.
                X Causes intrusive artifacts on the edges of block textures far away
            * GL_NEAREST_MIPMAP_LINEAR: 
                - linearly interpolates between the two mipmaps that most closely match the size of a pixel and samples the interpolated level via nearest neighbor interpolation.
                X DITTO to GL_NEAREST_MIPMAP_NEAREST
            * GL_LINEAR_MIPMAP_LINEAR:
                - linearly interpolates between the two closest mipmaps and samples the interpolated level via linear interpolation.
                X DITTO to GL_LINEAR_MIPMAP_NEAREST
        */

        switch (sampleType)
        {
            case TextureSampleType.Nearest:
                glTexParameteri(GL_TEXTURE_TYPE, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_LINEAR);
                glTexParameteri(GL_TEXTURE_TYPE, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
                break;
            default:
                glTexParameteri(GL_TEXTURE_TYPE, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
                glTexParameteri(GL_TEXTURE_TYPE, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                break;
        }

        glTexParameteri(GL_TEXTURE_TYPE, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_TYPE, GL_TEXTURE_WRAP_T, GL_REPEAT);

        // Set to higher value depending on how big the texture atlas is
        //glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAX_LEVEL, 1); 

        try
        {
            /*
            if (isTextureArray)
            {
                glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, 0, width, height, images.Count, GL_RGB, GL_UNSIGNED_BYTE, textureDataPointer);
            }
            else
            {
                glTexImage2D(GL_TEXTURE_2D_ARRAY, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, textureDataPointer);
            }*/

            //glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, 0, width, height, images.Count, GL_RGB, GL_UNSIGNED_BYTE, textureDataPointer);
            if (GL_TEXTURE_TYPE == GL_TEXTURE_2D)
            {
                glTexImage2D(GL_TEXTURE_TYPE, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, textureDataPointer);
            }
            else if(GL_TEXTURE_TYPE == GL_TEXTURE_2D_ARRAY)
            {
                glTexImage3D(GL_TEXTURE_TYPE, 0, GL_RGBA, width, height, texData.Length / (width*height*4), 0, GL_RGBA, GL_UNSIGNED_BYTE, textureDataPointer);
            }
            else
            {
                throw new NotImplementedException($"Type of '{GL_TEXTURE_TYPE}' is not implemented!");
            }

            glGenerateMipmap(GL_TEXTURE_TYPE);
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

    protected int GL_TEXTURE_TYPE;
    public void BindTexture()
    {
        if (textureRef == 0) BindOpenGL();

        glBindTexture(GL_TEXTURE_TYPE, textureRef);
    }
    public void UnbindTexture()
    {
        glBindTexture(GL_TEXTURE_TYPE, 0);
    }
    public void Delete()
    {
        if (textureRef == 0) return;

        glDeleteTexture(textureRef);
        textureRef = 0;
    }
}
public enum TextureSampleType
{
    Linear,
    Nearest
}