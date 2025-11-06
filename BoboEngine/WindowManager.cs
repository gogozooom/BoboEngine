using GLFW;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using static OpenGL.GL;

namespace BoboEngine
{
    internal static class WindowManager
    {
        public static Window window { get; private set; }
        public static Float2 windowSize { get; private set; }
        public static Vector4 clearColor { get; private set; }

        public static unsafe void CreateWindow(int width, int height, string title)
        {
            windowSize = new Float2(width, height);

            Glfw.Init();

            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            Glfw.WindowHint(Hint.Focused, true);
            Glfw.WindowHint(Hint.Resizable, false);

            window = Glfw.CreateWindow(width, height, title, GLFW.Monitor.None, Window.None);

            if (window == Window.None)
            {
                Console.WriteLine("Failed to create window!");
                return;
            }

            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - width) / 2;
            int y = (screen.Height - height) / 2;

            Glfw.SetWindowPosition(window, x, y);

            Glfw.MakeContextCurrent(window);

            // Import all gl functions
            Import(Glfw.GetProcAddress);

            glViewport(0, 0, width, height);

            // GL Setting
            Glfw.SwapInterval(0); // VSync

            glClearDepth(1);

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glEnable(GL_DEPTH_TEST);
            glEnable(GL_LEQUAL);

            // Errors
            Glfw.SetErrorCallback((code, message) =>
            {
                Console.WriteLine(message);
            });
        }

        public static void CloseWindow()
        {
            Glfw.DestroyWindow(window);
            Glfw.Terminate();
        }

        public static void ClearBuffer()
        {
            glClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }


        public static void SetClearColor(Vector4 _clearColor)
        {
            clearColor = _clearColor;
        }
    }
}