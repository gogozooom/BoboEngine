using BoboEngine;
using StbImageSharp;


namespace Minecraft;
public static class TextureManager
{
    public static Texture2DArray blockTextureArray { get; private set; }
    public static Dictionary<string, int> blockTextureIds { get; private set; }
    public static void GenerateAtlas()
    {
        List<ImageResult> images = [Texture.ReadImageFile(Program.GetLocalTexturePath("NULL"), ColorComponents.RedGreenBlueAlpha)];
        blockTextureIds = new Dictionary<string, int> { { "NULL", 0 } };

        var texturesPath = Path.Combine(Program.ProgramDirectory, "Data\\assets\\minecraft\\textures");

        if (!Directory.Exists(texturesPath))
        {
            Program.LogError("No minecraft data supported! Please put minecraft data named as 'Data' in executable directory!");
        }
        else
        {
            var files = Directory.GetFiles(texturesPath, "*.png", SearchOption.AllDirectories);

            int i = 1; // 1 to account for null texture
            int t = files.Length;

            foreach (var file in files)
            {
                var fileName = file.Remove(0, texturesPath.Length + 1).Split('.')[0].Replace('\\', '/');

                if (IgnoreFile(file.Remove(0, texturesPath.Length + 1)))
                {
                    Program.Log($"Ignoring '{fileName}'...");
                    continue;
                }

                var type = Path.GetExtension(file);

                var image = Texture.ReadImageFile(file, ColorComponents.RedGreenBlueAlpha);

                images.Add(image);

                int total16Sprites = (image.Width / 16) * (image.Height / 16);

                if(image.Width % 16f != 0 || image.Height % 16f != 0)
                {
                    Program.LogWarning($"Ignoring '{fileName}' because it's not uniform!");
                    continue;
                }

                for (int sizeI = 0; sizeI < total16Sprites; sizeI++)
                {
                    blockTextureIds.Add(fileName + (total16Sprites > 1 ? $"_{sizeI}" : ""), i); // TODO: find better way to load textures bigger than 16x16

                    i++;
                }

                Program.LogMessage($"Loaded ({images.Count}/{t}) --- '{fileName}{type}'");
            }
        }

        blockTextureArray = new Texture2DArray(images.ToArray(), "atlas", TextureSampleType.Nearest);
    }

    public static bool IgnoreFile(string file)
    {
        return !file.StartsWith("block");
    }

    public static int GetTextureID(string textureName)
    {
        if (!blockTextureIds.ContainsKey(textureName))
        {
            Program.LogError("TextureAtlasManager: Requested non-existent texture: " + textureName);
            //textureName = "NULL";
            return 0;
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