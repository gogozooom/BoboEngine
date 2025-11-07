using GLFW;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using static OpenGL.GL;

namespace BoboEngine
{
    internal static class WindowManager
    {
        public static bool Initialized => Window != Window.None;

        public static Window Window { get; private set; }
        public static Float2 WindowSize { get; private set; }
        public static float WindowAspectRatio => WindowSize.x / WindowSize.y;
        public static Vector4 ClearColor { get; private set; }

        public static unsafe void CreateWindow(int width, int height, string title)
        {
            WindowSize = new Float2(width, height);

            Glfw.Init();

            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
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
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glEnable(GL_DEPTH_TEST);
            glEnable(GL_LEQUAL);

            Glfw.SetFramebufferSizeCallback(Window, framebuffer_size_callback);

            // Errors
            Glfw.SetErrorCallback((code, message) =>
            {
                Console.WriteLine(message);
            });
        }

        static void framebuffer_size_callback(Window window, int width, int height)
        {
            WindowSize = new Float2(width, height);
            glViewport(0, 0, width, height);
        }

        public static void CloseWindow()
        {
            Glfw.DestroyWindow(Window);
            Glfw.Terminate();
        }

        public static void ClearBuffer()
        {
            glClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }


        public static void SetClearColor(Vector4 _clearColor)
        {
            ClearColor = _clearColor;
        }
    }
}