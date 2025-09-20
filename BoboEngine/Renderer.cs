using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace BoboEngine;

public static class Renderer
{
    public static int renderFrameWidth = 1680 / 2;
    public static int renderFrameHeight = 1050 / 2;
    public static void Initalize()
    {
        //RenderWindowLoop(); // Runs on same thread

        Task loop = Task.Run(RenderWindowLoop); // Runs on a separate thread
    }
    static void RenderWindowLoop()
    {
        Camera camera = SceneManager.currentScene.camera;

        Raylib.InitWindow(renderFrameWidth, renderFrameHeight, "BoboEngine");
        Texture2D texture = Raylib.LoadTextureFromImage(Raylib.GenImageColor(renderFrameWidth, renderFrameHeight, Color.Black));
        Color[] texColBuffer = new Color[renderFrameWidth * renderFrameHeight * 4]; // RGBA

        int i = 0;

        while (!Raylib.WindowShouldClose())
        {
            Time.deltaTime = Raylib.GetFrameTime();
            SceneManager.currentScene.Update();

            texColBuffer = ToFlatByteArray(Render(SceneManager.currentScene)); // ToFlatByteArray also doubles as a background remover

            Raylib.UpdateTexture(texture, texColBuffer);

            Rectangle src = new(0, texture.Height, texture.Width, texture.Height);
            Rectangle dest = new(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Vector2 origin = new(0, 0);

            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(texture, src, dest, origin, 0.0f, Color.White);
            Raylib.EndDrawing();

            /*
            Debug.WriteLine($"Preformance Report: FPS '{(int)(1 / Time.deltaTime)}'" +
                $"\n - Meshes:        '{debug_meshes}'" +
                $"\n - Tris:        '{debug_trianglesRendered}'" +
                $"\n - Pixel Checks '{debug_pixelsChecked}'");
            //*/

            debug_trianglesRendered = 0;
            debug_pixelsChecked = 0;
            debug_meshes = 0;

            i++;
        }

        Raylib.CloseWindow();
    }

    static Color[] ToFlatByteArray(Float3[] colorBuffer)
    {
        Color[] data = new Color[colorBuffer.Length];

        for (int i = 0; i < colorBuffer.Length; i++)
        {
            Float3 col = colorBuffer[i];
            data[i] = new Color((int)(Math.Clamp(col.r, 0, 1) * 255), (int)(Math.Clamp(col.g, 0, 1) * 255), (int)(Math.Clamp(col.b, 0, 1) * 255), 255);
        }

        return data;
    }

    public static int debug_trianglesRendered = 0;
    public static int debug_pixelsChecked = 0;
    public static int debug_meshes = 0;
    public static Float3[] Render(Scene scene)
    {
        Camera cam = scene.camera;

        Float3[] colorBuffer = new Float3[renderFrameWidth * renderFrameHeight];
        float[] depthBuffer = new float[renderFrameWidth * renderFrameHeight];

        List<Mesh> meshes = new();

        foreach (var obj in scene.objects)
        {
            Mesh mesh = obj.GetComponent<Mesh>();

            if (mesh == null) continue;

            if (mesh.shader == null)
            {
                Debug.WriteLine($"Could not render '{obj}' because no shader is assigned!");
                continue;
            }

            meshes.Add(mesh);
        }

        foreach (var mesh in meshes)
        {
            debug_meshes++;

            Transform transform = mesh.transform;

            Parallel.For(0, mesh.faces.Length, i =>
            {
                FaceInfo face = mesh.faces[i];

                Float3 a = VertexToScreen(mesh.vertices[face.vertex_indexs[0]], transform, cam);
                Float3 b = VertexToScreen(mesh.vertices[face.vertex_indexs[1]], transform, cam);
                Float3 c = VertexToScreen(mesh.vertices[face.vertex_indexs[2]], transform, cam);
                if (a.z <= 0 || b.z <= 0 || c.z <= 0) return; // Make better fix later

                debug_trianglesRendered++;

                // Bounds
                float minX = Maths.Min(a.x, b.x, c.x);
                float minY = Maths.Min(a.y, b.y, c.y);

                float maxX = Maths.Max(a.x, b.x, c.x);
                float maxY = Maths.Max(a.y, b.y, c.y);

                int blockStartX = Math.Clamp((int)minX, 0, renderFrameWidth - 1);
                int blockStartY = Math.Clamp((int)minY, 0, renderFrameHeight - 1);
                int blockEndX = Math.Clamp((int)maxX, 0, renderFrameWidth - 1);
                int blockEndY = Math.Clamp((int)maxY, 0, renderFrameHeight - 1);

                for (int y = blockStartY; y <= blockEndY; y++)
                {
                    for (int x = blockStartX; x <= blockEndX; x++)
                    {
                        debug_pixelsChecked++;

                        Float2 p = new(x, y);

                        /* Test 
                        int px = y * renderFrameWidth + x;
                        colorBuffer[px] = mesh.faceColors[i]; // Color Render
                        continue;
                        // Test */

                        if (Maths.PointInTriangle((Float2)a, (Float2)b, (Float2)c, p, out Float3 weights))
                        {
                            int px = y * renderFrameWidth + x;

                            Float3 depths = new(a.z, b.z, c.z);

                            float depth = 1 / Float3.Dot(1 / depths, weights);

                            if (depth > depthBuffer[px] && depthBuffer[px] != 0) continue;

                            depthBuffer[px] = depth;

                            Float2 texCoord = Float2.zero;
                            texCoord += mesh.textureCoords[face.texture_indexs[0]] / depths.x * weights.x;
                            texCoord += mesh.textureCoords[face.texture_indexs[1]] / depths.y * weights.y;
                            texCoord += mesh.textureCoords[face.texture_indexs[2]] / depths.z * weights.z;
                            texCoord *= depth;

                            colorBuffer[px] = mesh.shader.PixelColor(new(x,y), texCoord, mesh.normals[face.normal_indexs[0]], depth); // Shader Renderer
                            
                            //colorBuffer[px] = TMPTexture.Sample(new((float)x/renderFrameWidth, (float)y /renderFrameHeight)); // Texture Render Screen

                            //colorBuffer[px] = new((float)x/renderFrameWidth, (float)y /renderFrameHeight, 0); // Debug UV Render Screen
                            //colorBuffer[px] = new(texCoord.x, texCoord.y); // Debug UV Render
                            //colorBuffer[px] = float3.white * MathF.Pow(2f, -depth); // Depth Color Render
                        }
                    }
                }
                //DrawToBMP(image, $"{fileName}[i.ToString()]"); // Draw Every Triangle
            });
        }

        return colorBuffer;
    }


    public static void DrawToBMP(Float3[] image, string fileName) // Borrowed from Sebastian Lague
    {
        throw new NotImplementedException("DEPRICATED!");

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
                Float3 col = image[y * x];
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

    public static Float3 VertexToScreen(Float3 vertex, Transform transform, Camera cam)
    {
        Float3 vertex_world = transform.ToWorldPoint(vertex);
        Float3 vertex_camera = cam.transform.ToLocalPoint(vertex_world);

        vertex_camera.y = -vertex_camera.y; // Fix top bottom rendering

        float screenHeight_world = Maths.Tan(cam.fov / 2) * 2;
        float pixelsPerWorldUnit = renderFrameWidth / screenHeight_world / vertex_camera.z;

        Float2 pixelOffset = (Float2)vertex_camera * pixelsPerWorldUnit;
        Float2 vertex_screen = pixelOffset + new Float2(renderFrameWidth, renderFrameHeight) / 2f;

        return new Float3(vertex_screen.x, vertex_screen.y, vertex_camera.z); // Center 0,0 (And mirror for top bottom rendering)
    }
}