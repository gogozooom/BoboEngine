using BoboEngine.Input;
using ConsoleCommand;
using GLFW;
using System.Drawing;
using System.Numerics;
using static OpenGL.GL;
using Cursor = BoboEngine.Input.Cursor;

namespace BoboEngine
{
    public static class WindowManager
    {
        public static bool Initialized => Window != Window.None;

        public static float targetFPS = 0;

        public static Window Window { get; private set; }
        public static Float2 WindowSize { get; private set; }
        public static float WindowAspectRatio => WindowSize.x / WindowSize.y;
        public static Float3 ClearColor { get; private set; }

        public static Action update;
        public static Action afterUpdate;

        public static Matrix4x4 cameraMatrixThisFrame { get; private set; }
        public static Matrix4x4 localCameraMatrixThisFrame { get; private set; }

        /// <summary>
        /// Opens a window and starts rendering the currently loaded scene
        /// Returns once window is closed
        /// </summary>
        public static unsafe void InitializeRenderLoop(int windowWidth, int windowHeight)
        {
            CreateWindow(windowWidth, windowHeight, Engine.TITLE);

            SceneManager.LoadScene();

            float timeLastFrame = Time.time;

            // Start Render
            while (!Glfw.WindowShouldClose(Window))
            {
                Time.deltaTime = Time.time - timeLastFrame;
                timeLastFrame = Time.time;
                Cursor.Update();

                Glfw.PollEvents();

                // update

                update?.Invoke();
                afterUpdate?.Invoke();

                // render

                ClearBuffer();

                List<MeshRenderer> validRenderObjects = new();

                foreach (var obj in SceneManager.currentScene.objects.ToArray())
                {
                    if (!obj.enabled) continue;

                    var renderObject = obj.GetComponent<MeshRenderer>();

                    if (!renderObject) continue;
                    if (!renderObject.ShouldRender()) continue;

                    validRenderObjects.Add(renderObject);
                }

                validRenderObjects.Sort();

                cameraMatrixThisFrame = Camera.main.GetProjectionMatrix();
                localCameraMatrixThisFrame = Camera.main.GetProjectionMatrix(false);

                foreach (MeshRenderer targetRenderObject in validRenderObjects)
                {
                    targetRenderObject.glBind();

                    targetRenderObject.glDraw();

                    targetRenderObject.glUnBind();
                }

                Glfw.SwapBuffers(Window);

                // Limit Framerate
                if (targetFPS > 0)
                    while ((1f / targetFPS) >= (Time.time - timeLastFrame)); // Much better than Thread.Sleep :sob:
            }

            CloseWindow();
            SceneManager.UnloadScene();
        }



        /// <summary>
        /// Creates a window to be used by RenderLoop() 
        /// </summary>
        static unsafe void CreateWindow(int width, int height, string title)
        {
            WindowSize = new Float2(width, height);

            Glfw.Init();

            Glfw.WindowHint(Hint.ContextVersionMajor, 4);
            Glfw.WindowHint(Hint.ContextVersionMinor, 5);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            Glfw.WindowHint(Hint.Focused, true);
            Glfw.WindowHint(Hint.Resizable, true);

            Window = Glfw.CreateWindow(width, height, title, GLFW.Monitor.None, Window.None);

            if (Window == Window.None)
            {
                Console.WriteLine("Failed to create window!");
                return;
            }

            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - width) / 2;
            int y = (screen.Height - height) / 2;

            Glfw.SetWindowPosition(Window, x, y);

            Glfw.MakeContextCurrent(Window);

            // Import all gl functions
            Import(Glfw.GetProcAddress);

            glViewport(0, 0, width, height);

            // GL Setting
            Glfw.SwapInterval(0); // VSync

            glClearDepth(1);

            glEnable(GL_BLEND);
            glEnable(GL_CULL_FACE);

            glEnable(GL_DEPTH_TEST);
            glEnable(GL_LEQUAL);

            Glfw.SetFramebufferSizeCallback(Window, framebuffer_size_callback);
            Glfw.SetKeyCallback(Window, InputSystem.key_callback);
            Glfw.SetCursorPositionCallback(Window, Cursor.cursor_position_callback);
            Glfw.SetMouseButtonCallback(Window, Cursor.mouse_button_callback);
            Glfw.SetScrollCallback(Window, Cursor.mouse_scroll_callback);
            
            // Errors
            Glfw.SetErrorCallback((code, message) =>
            {
                Console.WriteLine(message);
            });
        }

        /// <summary>
        /// The event call back when the window size is changed
        /// </summary>
        static void framebuffer_size_callback(Window window, int width, int height)
        {
            WindowSize = new Float2(width, height);
            glViewport(0, 0, width, height);
        }

        /// <summary>
        /// Closes the current window and terminates all Glfw processes
        /// </summary>
        public static void CloseWindow()
        {
            Glfw.DestroyWindow(Window);
            Glfw.Terminate();
            Window = Window.None;
        }

        /// <summary>
        /// Clears the visual buffer with the specified <see cref="ClearColor"/>
        /// </summary>
        /// <param name="fullClear">Determines if clear color is to be used OR only clear depth</param>
        public static void ClearBuffer(bool fullClear = true)
        {
            if (!fullClear)
            {
                glClear(GL_DEPTH_BUFFER_BIT);
                return;
            }

            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }

        /// <summary>
        /// Sets <see cref="ClearColor"/>
        /// </summary>
        public static void SetClearColor(Float3 _clearColor)
        {
            ClearColor = _clearColor;
            if (Initialized) glClearColor(_clearColor.r, _clearColor.g, _clearColor.b, 1);
        }


        [Command("RenderMode", "['renderMode'] 'default' | 'wireframe' ")]
        public static void SetRenderMode(string mode)
        {
            if (mode == null) mode = "default";

            switch (mode.ToLower())
            {
                case "default":
                    glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                    break;
                case "wireframe":
                    glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
                    break;
                default:
                    Engine.Log($"Invalid mode: '{mode}'");
                    break;
            }
        }

        [Command("FPS", "['MaxFPS'] Limits the framerate (0 for unlimited)")]
        public static void SetFPS(int fps) => targetFPS = fps;
    }
}