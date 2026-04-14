using System.Numerics;

namespace BoboEngine
{
    public class Camera : ObjectBehavior
    {
        public static Camera main { get; private set; }

        public float fov { get => _fov; set => SetFOV(value); }
        private float _fov = 70f;
        private void SetFOV(float v) => _fov = Math.Clamp(v, 0.1f, 179.9f);

        public float nearPlane = 0.01f;
        public float farPlane = 1000f;

        public Camera()
        {
            if (main)
            {
                return;
            }

            main = this;
        }

        public Matrix4x4 GetProjectionMatrix(bool applyTransform = true, bool applyRotation = true)
        {
            var perspectiveMatrix = Matrix4x4.CreateScale(new Vector3(-1, 1, -1)) * Matrix4x4.CreatePerspectiveFieldOfView(Maths.ToRad(fov), WindowManager.WindowAspectRatio, nearPlane, farPlane);

            Matrix4x4 transM = Matrix4x4.Identity;
            Matrix4x4 rotM = Matrix4x4.Identity;

            if (applyTransform) transM = GetProjectionPositionMatrix();
            if (applyRotation) rotM = transform.MatrixInverseRot;

            return transM * rotM * perspectiveMatrix;
        }
        public Matrix4x4 GetProjectionPositionMatrix()
        {
            Matrix4x4.Invert(transform.MatrixTrans, out var transM);

            return transM;
        }

        public Matrix4x4 Get2DProjectionMatrix()
        {
            float left = transform.position.x - WindowManager.WindowSize.x / 2f;
            float right = transform.position.x + WindowManager.WindowSize.x / 2f;
            float top = transform.position.y - WindowManager.WindowSize.y / 2f;
            float bottom = transform.position.y + WindowManager.WindowSize.y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.0001f, 100f);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(MathF.Max(transform.position.z*transform.position.z, 1));

            return orthoMatrix * zoomMatrix;
        }

        // May be usefull for some effects later idk
        public Float3 VertexToScreen(Float3 vertex, Transform transform = null)
        {
            if (!WindowManager.Initialized)
            {
                Engine.LogError("Cannot use VertexToScreen() without a window!");
                return Float3.zero;
            }

            Float3 vertex_world = vertex;
            if (transform != null) vertex_world = transform.ToWorldPoint(vertex);
            Float3 vertex_camera = transform.ToLocalPoint(vertex_world);

            vertex_camera.y = -vertex_camera.y; // Fix top bottom rendering

            float screenHeight_world = Maths.Tan(fov / 2) * 2;
            float pixelsPerWorldUnit = WindowManager.WindowSize.x / screenHeight_world / vertex_camera.z;

            Float2 pixelOffset = (Float2)vertex_camera * pixelsPerWorldUnit;
            Float2 vertex_screen = pixelOffset + WindowManager.WindowSize / 2f;

            return new Float3(vertex_screen.x, vertex_screen.y, vertex_camera.z); // Center 0,0 (And mirror for top bottom rendering)
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (main == this) main = null;
        }
    }
}
