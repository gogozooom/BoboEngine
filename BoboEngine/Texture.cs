//using StbImageSharp;
using System.Runtime.InteropServices;
using static OpenGL.GL;

namespace BoboEngine;
public class Texture
{
    //public ImageResult imageR { get; private set; }
    public string name { get; private set; }
    public List<Float3[,]> images { get; private set; }
    public bool isTextureArray => images.Count > 1;

    public bool loaded { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }


    public Texture()
    {
        LoadNullTexture();
    }

    public Texture(string filePath)
    {
        images = new();
        if (!LoadImageFile(filePath)) // Failed to load texture
        {
            LoadNullTexture();
        }
    }

    public Texture(Float3[,] image, string name)
    {
        images = [image];
        this.name = name;

        loaded = true;

        UpdateImageProperties();
    }

    public Texture(List<Float3[,]> images, string name)
    {
        this.images = images;
        this.name = name;

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
        if (images.Count == 0) return;

        width = images[0].GetLength(0);
        height = images[0].GetLength(1);
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

        images.Clear();

        /*
        using (var stream = File.OpenRead(filePath))
        {
            imageR = ImageResult.FromStream(stream);
        }
        */

        name = Path.GetFileNameWithoutExtension(filePath);

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


        images.Add(image);
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

        GCHandle gCHandle = GCHandle.Alloc(texData, GCHandleType.Pinned);
        IntPtr textureDataPointer = gCHandle.AddrOfPinnedObject();

        textureRef = glGenTexture();

        glBindTexture(GL_TEXTURE_2D_ARRAY, textureRef);

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

        glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_LINEAR);
        glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

        glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_T, GL_REPEAT);

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
            glTexImage3D(GL_TEXTURE_2D_ARRAY, 0, GL_RGB, width, height, images.Count, 0, GL_RGB, GL_UNSIGNED_BYTE, textureDataPointer);
            glGenerateMipmap(GL_TEXTURE_2D_ARRAY);
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

        glBindTexture(GL_TEXTURE_2D_ARRAY, textureRef);
    }
    public void UnbindTexture()
    {
        glBindTexture(GL_TEXTURE_2D_ARRAY, 0);
    }
    public void Delete()
    {
        if (textureRef == 0) return;

        glDeleteTexture(textureRef);
        textureRef = 0;
    }
}
