using GLFW;

namespace BoboEngine
{
    public static class Time
    {
        /// <summary>
        /// Time this frame
        /// Set by Renderer.cs
        /// </summary>
        public static float deltaTime = 0;
        /// <summary>
        /// Time since game started
        /// Set by Renderer.cs
        /// </summary>
        public static float time => (float)Glfw.Time;
    }
}
