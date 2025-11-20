using BoboEngine;
using StbImageSharp;


namespace Minecraft;
public static class TextureManager
{
    private const int GridSize = 16; // Assumed to be 16x16 textures for now
    public static Texture2DArray blockTextureArray { get; private set; }
    public static Dictionary<string, int> blockTextureIds { get; private set; } = new();
    public static void GenerateAtlas()
    {
        List<Texture2D> textures = new();

        var nullT = new Texture2D(Program.GetLocalTexturePath("NULL"));

        textures.Add(nullT); // Guarantee null texture!


        var blockTexturePath = Path.Combine(Program.ProgramDirectory, "Data\\assets\\minecraft\\textures\\block");

        if (!Directory.Exists(blockTexturePath))
        {
            Program.LogError("No minecraft data supported! Please put minecraft data named as 'Data' in executable directory!");
        }
        else
        {
            foreach (var file in Directory.GetFiles(blockTexturePath))
            {
                var name = Path.GetFileName(file);
                var type = Path.GetExtension(file);

                if (type == ".png")
                    textures.Add(new Texture2D(file));
                else if (type != ".mcmeta")
                    Program.LogWarning($"File of type {name}.'{type}'");

            }
        }

        blockTextureArray = GenerateAtlas(textures, nullT);
    }

    public static Texture2DArray GenerateAtlas(List<Texture2D> textures, Texture2D backgroundTexture)
    {
        int indexWH = (int)MathF.Ceiling(MathF.Sqrt(textures.Count));
        int gridWH = indexWH * GridSize;

        // UVS
        blockTextureIds.Clear();

        List<ImageResult> output = new();

        int i = 0;
        foreach (var texture in textures)
        {
            output.Add(texture.image);

            blockTextureIds.Add(texture.name, i); // Change to block id
            i++;
        }

        return new Texture2DArray(output.ToArray(), "atlas", TextureSampleType.Nearest);
    }

    public static int GetTextureID(string textureName)
    {
        if (!blockTextureIds.ContainsKey(textureName))
        {
            Program.LogError("TextureAtlasManager: Requested non-existent texture: " + textureName);
            textureName = "NULL";
        }

        return blockTextureIds[textureName];
    }

    public static void DrawToBMP(Float3[,] image, string fileName) // Borrowed from Sebastian Lague
    {
        string outputPath = Path.Combine(Program.ProgramDirectory, "Output", fileName.Split('.')[0] + ".bmp");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        using BinaryWriter writer = new(File.Open(outputPath, FileMode.Create));
        uint[] ByteCounts = { 14, 40, (uint)image.Length * 4 }; // BMP header, DIP header, data

        // -- Headers --
        writer.Write("BM"u8.ToArray()); // BMP header start
        writer.Write(ByteCounts[0] + ByteCounts[1] + ByteCounts[2]); // total file size
        writer.Write((uint)0); // unused
        writer.Write(ByteCounts[0] + ByteCounts[1]); // data offset (from start)
        writer.Write(ByteCounts[1]); // DIP header size
        writer.Write((uint)image.GetLength(0)); // image width
        writer.Write((uint)image.GetLength(1)); // image height
        writer.Write((ushort)1); // num color planes (?)
        writer.Write((ushort)(8 * 4)); // bits per pixel (1 byte per channel, plus 1 for alignment)
        writer.Write((uint)0); // RGB format, no compression
        writer.Write(ByteCounts[2]); // data size
        writer.Write(new byte[16]); // print resolution and palette info (ignoring)

        // --- Data ---
        for (int y = 0; y < image.GetLength(1); y++)
        {
            for (int x = 0; x < image.GetLength(0); x++)
            {
                Float3 col = image[x, y];
                writer.Write((byte)(col.b * 255));
                writer.Write((byte)(col.g * 255));
                writer.Write((byte)(col.r * 255));
                writer.Write((byte)0); // Padding (Alpha?)
            }
        }

        Program.LogMessage($"Created file at: '{Path.GetFullPath(outputPath)}'");

        writer.Close();
        //Process.Start("explorer.exe", '"'+outputPath+'"');
    }
}