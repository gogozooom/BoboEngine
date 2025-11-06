using ConsoleCommand;
using GLFW;
using Raylib_cs;
using System.Numerics;
using static OpenGL.GL;

namespace BoboEngine;

public enum renderType
{
    def,
    depth,
    normals,
    uv
}

public static class Renderer
{
    public static int renderFrameWidth = 1680 / 2;
    public static int renderFrameHeight = 1050 / 2;

    public static renderType debug_renderType = renderType.def;
    public static bool debug_viewNormals = false;
    public static bool debug_viewWireFrame = false;

    public static void Initalize()
    {
        RenderWindowLoopGL(); // Runs on same thread

        //Task loop = Task.Run(RenderWindowLoopGL); // Runs on a separate thread
    }

    static void RenderWindowLoopGL()
    {
        WindowManager.SetClearColor(new Vector4(1, 0, 1, 1));

        WindowManager.CreateWindow(renderFrameWidth, renderFrameHeight, Program.TITLE);

        //var testMesh = SceneManager.currentScene.Find("Cobblestone2").GetComponent<Mesh>();

        var testMesh = new Mesh();

        testMesh.LoadObjFile(Program.GetLocalModelPath("Test"));

        while (!Glfw.WindowShouldClose(WindowManager.window))
        {
            Glfw.PollEvents();

            // update

            // render
            WindowManager.ClearBuffer();

            testMesh.shader.Bind();

            testMesh.BindVAO();
            glDrawArrays(GL_TRIANGLES, 0, (int)testMesh.GetVertexBufferSize());
            testMesh.UnBindVAO();


            testMesh.shader.Unbind();

            Glfw.SwapBuffers(WindowManager.window);
        }


        WindowManager.CloseWindow();
    }

    /*
    static void RenderWindowLoopRAY()
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

            
            Debug.WriteLine($"Preformance Report: FPS '{(int)(1 / Time.deltaTime)}'" +
                $"\n - Meshes:        '{debug_meshes}'" +
                $"\n - Tris:        '{debug_trianglesRendered}'" +
                $"\n - Pixel Checks '{debug_pixelsChecked}'");
            

            debug_trianglesRendered = 0;
            debug_pixelsChecked = 0;
            debug_meshes = 0;

            i++;
        }

        Raylib.CloseWindow();
    }
    */

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
    
    /*
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

            var depthLock = new object();

            Parallel.For(0, mesh.faces.Length, i =>
            {
                FaceInfo face = mesh.faces[i];

                Float3 a = VertexToScreen(cam, mesh.vertices[face.vertex_indexs[0]], transform);
                Float3 b = VertexToScreen(cam, mesh.vertices[face.vertex_indexs[1]], transform);
                Float3 c = VertexToScreen(cam, mesh.vertices[face.vertex_indexs[2]], transform);
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

                Float3 invDepths = new(1 / a.z, 1 / b.z, 1 / c.z);
                Float2 tx1 = mesh.textureCoords[face.texture_indexs[0]] * invDepths.x;
                Float2 tx2 = mesh.textureCoords[face.texture_indexs[1]] * invDepths.y;
                Float2 tx3 = mesh.textureCoords[face.texture_indexs[2]] * invDepths.z;
                Float3 n1 = mesh.normals[face.normal_indexs[0]];
                Float3 n2 = mesh.normals[face.normal_indexs[1]];
                Float3 n3 = mesh.normals[face.normal_indexs[2]];

                if (debug_viewNormals)
                {
                    Float3 normalLineColor = new(0, 0.3f, 1f);

                    Float3 pointWorldPos1 = transform.ToWorldPoint(mesh.vertices[face.vertex_indexs[0]]);
                    Float3 pointWorldPos2 = transform.ToWorldPoint(mesh.vertices[face.vertex_indexs[1]]);
                    Float3 pointWorldPos3 = transform.ToWorldPoint(mesh.vertices[face.vertex_indexs[2]]);

                    DrawLine(pointWorldPos1, pointWorldPos1 + transform.TransformVector(n1), new(1, 0, 0), cam, ref colorBuffer, ref depthBuffer);
                    DrawLine(pointWorldPos2, pointWorldPos2 + transform.TransformVector(n2), new(0, 1, 0), cam, ref colorBuffer, ref depthBuffer);
                    DrawLine(pointWorldPos3, pointWorldPos3 + transform.TransformVector(n3), new(0, 0, 1), cam, ref colorBuffer, ref depthBuffer);
                }

                if (debug_viewWireFrame)
                {
                    Float3 wireLineColor = Float3.one;

                    DrawLine((Int2)a, (Int2)b, wireLineColor, cam, ref colorBuffer, ref depthBuffer);
                    DrawLine((Int2)b, (Int2)c, wireLineColor, cam, ref colorBuffer, ref depthBuffer);
                    DrawLine((Int2)c, (Int2)a, wireLineColor, cam, ref colorBuffer, ref depthBuffer);
                    return;
                }

                for (int y = blockStartY; y <= blockEndY; y++)
                {
                    for (int x = blockStartX; x <= blockEndX; x++)
                    {
                        debug_pixelsChecked++;

                        Float2 p = new(x, y);

                        if (Maths.PointInTriangle((Float2)a, (Float2)b, (Float2)c, p, out Float3 weights))
                        {
                            int px = y * renderFrameWidth + x;

                            float depth = 1 / (invDepths.x * weights.x + invDepths.y * weights.y + invDepths.z * weights.z);

                            lock (depthLock)
                            {
                                if (depth > depthBuffer[px] && depthBuffer[px] != 0) continue; // -1 == TMP line render

                                depthBuffer[px] = depth;
                            }

                            Float2 texCoord = (tx1 * weights.x + tx2 * weights.y + tx3 * weights.z) * depth;
                            Float3 normal = Float3.zero;

                            if (face.normal_indexs[0] == face.normal_indexs[1] && face.normal_indexs[1] == face.normal_indexs[2]) // Not shade smooth
                            {
                                normal = mesh.normals[face.normal_indexs[0]].Normalized();
                            }
                            else
                            {
                                normal = (n1 * weights.x + n2 * weights.y + n3 * weights.z).Normalized();
                            }

                            normal = transform.TransformVector(normal);

                            switch (debug_renderType)
                            {
                                case renderType.depth:
                                    colorBuffer[px] = Float3.white * MathF.Pow(2f, -depth); // Depth Color Render
                                    break;
                                case renderType.uv:
                                    colorBuffer[px] = new(texCoord.x, texCoord.y); // Debug UV Render
                                    break;
                                default:
                                    colorBuffer[px] = mesh.shader.PixelColor(new(x, y), texCoord, normal, depth, mesh.transform); // Shader Renderer
                                    break;
                            }
                        }
                    }
                }
                //DrawToBMP(image, $"{fileName}[i.ToString()]"); // Draw Every Triangle
            });

            // Test line renderer
        }

        return colorBuffer;
    }
    //*/

    public static void DrawLine(Int2 startPointA, Int2 endPointB, Float3 lineColor, Camera cam, ref Float3[] colorBuffer, ref float[] depthBuffer)
    {
        // Bruh, I'm way to lazy to figure this out myself. SOURCE: https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        int w = endPointB.x - startPointA.x;
        int h = endPointB.y - startPointA.y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            if (startPointA.x < 0 || startPointA.y < 0 || startPointA.x >= renderFrameWidth || startPointA.y >= renderFrameHeight) continue; // Point off of screen

            int px = startPointA.y * renderFrameWidth + startPointA.x;

            colorBuffer[px] = lineColor;
            depthBuffer[px] = -1;

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                startPointA += new Int2(dx1, dy1);
            }
            else
            {
                startPointA += new Int2(dx2, dy2);
            }
        }
    }
    public static void DrawLine(Float3 linePointA, Float3 linePointB, Float3 lineColor, Camera cam, ref Float3[] colorBuffer, ref float[] depthBuffer)
    {
        Float3 screenLinePointA = VertexToScreen(cam, linePointA);
        Float3 screenLinePointB = VertexToScreen(cam, linePointB);

        if (screenLinePointA.z <= 0 || screenLinePointB.z <= 0) return; // Line behind the screen

        Int2 startPointA = (Int2)(Float2)screenLinePointA;
        Int2 endPointB = (Int2)(Float2)screenLinePointB;

        DrawLine(startPointA, endPointB, lineColor, cam, ref colorBuffer, ref depthBuffer);
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

    public static Float3 VertexToScreen(Camera cam, Float3 vertex, Transform transform = null)
    {
        Float3 vertex_world = vertex;
        if (transform != null) vertex_world = transform.ToWorldPoint(vertex);
        Float3 vertex_camera = cam.transform.ToLocalPoint(vertex_world);

        vertex_camera.y = -vertex_camera.y; // Fix top bottom rendering

        float screenHeight_world = Maths.Tan(cam.fov / 2) * 2;
        float pixelsPerWorldUnit = renderFrameWidth / screenHeight_world / vertex_camera.z;

        Float2 pixelOffset = (Float2)vertex_camera * pixelsPerWorldUnit;
        Float2 vertex_screen = pixelOffset + new Float2(renderFrameWidth, renderFrameHeight) / 2f;

        return new Float3(vertex_screen.x, vertex_screen.y, vertex_camera.z); // Center 0,0 (And mirror for top bottom rendering)
    }

    // -- Commands --

    [Command("SetRenderType", "['default/depth/uv/normals'] Sets the render type.")]
    public static void SetRenderType(string type)
    {
        switch (type)
        {
            case "default":
                debug_renderType = renderType.def;
                break;
            case "depth":
                debug_renderType = renderType.depth;
                break;
            case "uv":
                debug_renderType = renderType.uv;
                break;
            case "normals":
                debug_renderType = renderType.normals;
                break;
            default:
                Program.LogError($"Could not parse '{type}'! Please input one of these: 'default/depth/uv/normals'");
                return;
        }

        Program.Log($"RenderType is now set to '{debug_renderType}'");
    }

    [Command("DebugNormals", "Toggles normal debug render view.")]
    public static void ToggleDebugNormalVisuals()
    {
        debug_viewNormals = !debug_viewNormals;
        Program.Log($"Wireframe is now equal to: '{debug_viewNormals}'");
    }
    [Command("DebugWireframe", "Toggles wireframe debug render view.")]
    public static void ToggleDebugWireframeVisuals()
    {
        debug_viewWireFrame = !debug_viewWireFrame;
        Program.Log($"Wireframe is now equal to: '{debug_viewWireFrame}'");
    }
}