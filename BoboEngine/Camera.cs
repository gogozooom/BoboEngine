using System.Numerics;

namespace BoboEngine
{
    public class Camera : ObjectBehavior
    {
        public static Camera main { get; private set; }

        public float fov = 80f;

        public float nearPlane = 0.01f;
        public float farPlane = 100f;

        public Camera()
        {
            if (main)
            {
                return;
            }

            main = this;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            var perspectiveMatrix = Matrix4x4.CreateScale(new Vector3(-1, 1, -1)) * Matrix4x4.CreatePerspectiveFieldOfView(Maths.ToRad(fov), WindowManager.WindowAspectRatio, nearPlane, farPlane);



            Matrix4x4.Invert(transform.MatrixTrans, out var transM);

            return transM * transform.MatrixInverseRot * perspectiveMatrix;
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

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (main == this) main = null;
        }
    }
}
