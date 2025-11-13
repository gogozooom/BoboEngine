using BoboEngine;

public static class TextureAtlasManager
{
    public static Texture blockTextureAtlas { get; private set; }

    public static void GenerateAtlas()
    {

        var blockTexturePath = Path.Combine(Program.GetLocalTexturePath(), "Blocks");

        List<Texture> textures = new();

        foreach (var file in Directory.GetFiles(blockTexturePath))
        {
            Program.Log("Loading Block Texture: " + file);

            textures.Add(new Texture(file));
        }

        var nullT = new Texture(Program.GetLocalTexturePath("NULL"));

        textures.Add(nullT); // Guarantee null texture!

        blockTextureAtlas = GenerateAtlas(textures, nullT);
    }

    public static Texture GenerateAtlas(List<Texture> textures, Texture backgroundTexture)
    {
        const int GridSize = 16; // Assumed to be 16x16 textures for now

        int indexWH = (int)MathF.Ceiling(MathF.Sqrt(textures.Count));
        int gridWH = indexWH * GridSize;

        Float3[,] output = new Float3[gridWH, gridWH];

        for (int y = 0; y < gridWH; y++)
        {
            for (int x = 0; x < gridWH; x++)
            {
                int xIndex = (int)MathF.Floor((float)x / GridSize);
                int yIndex = (int)MathF.Floor((float)y / GridSize);

                int localX = x - (xIndex * GridSize);
                int localY = y - (yIndex * GridSize);

                int textureIndex = yIndex * indexWH + xIndex;

                Texture selectedTexture = backgroundTexture;

                if(textureIndex < textures.Count)
                {
                    selectedTexture = textures[textureIndex];
                }

                output[x, y] = selectedTexture.image[localX, localY];
            }
        }


        return new Texture(output);
    }

    public static void DrawToBMP(Float3[,] image, string fileName) // Borrowed from Sebastian Lague
    {
        string outputPath = Path.Combine(Program.ProgramDirectory, "Output", fileName.Split('.')[0] + ".bmp");

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