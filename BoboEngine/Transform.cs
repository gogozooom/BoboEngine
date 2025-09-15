using BoboEngine.GMath;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace BoboEngine
{
    public class Transform
    {
        float3 _position;
        float3 _scale = float3.one;
        public float _pitch;
        public float _yaw;
        public float _roll;

        public float3 position { get => _position; set => SetPosition(value); }
        public float3 rotation { get => new(pitch, yaw, roll); set => SetRotation(value); }
        public float3 scale { get => _scale; set => SetScale(value); }

        public float pitch { get => _pitch; set => SetRotation(new(value, _yaw, _roll)); }
        public float yaw { get => _yaw; set => SetRotation(new(_pitch, value, _roll)); }
        public float roll { get => _roll; set => SetRotation(new(_pitch, _yaw, value)); }

        public float3 rightVector { get; private set; }
        public float3 upVector { get; private set; }
        public float3 backVector { get; private set; }

        public float3 inv_rightVector { get; private set; }
        public float3 inv_upVector { get; private set; }
        public float3 inv_backVector { get; private set; }

        public Transform()
        {
            SetPosition(_position);
            SetRotation(new(pitch, yaw, roll));
            SetScale(_scale);
        }
        public Transform(float3 position, float3 rotation, float3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void SetRotation(float3 rotation)
        {
            _pitch = rotation.x;
            _yaw = rotation.y;
            _roll = rotation.z;

            (rightVector, upVector, backVector) = GetBasisVectors();
            (inv_rightVector, inv_upVector, inv_backVector) = GetInverseBasisVectors();
        }
        public void SetPosition(float3 position)
        {
            _position = position;
        }
        public void SetScale(float3 scale) // TODO: scale basic vectors?
        {
            _scale = scale;
        }
        public void Reset()
        {
            _position = new(0, 0, 0);
            rotation = new(0, 0, 0);
        }
        float3 GetRotationFromBasisVectors((float3 rightVector, float3 upVector, float3 backVector) bVec)
        {
            throw new NotImplementedException(); // Math too big brain for me...
            return new();
        }
        (float3 rightVector, float3 upVector, float3 backVector) GetInverseBasisVectors()
        {
            return (new(rightVector.x, upVector.x, backVector.x),
                    new(rightVector.y, upVector.y, backVector.y),
                    new(rightVector.z, upVector.z, backVector.z));
        }
        (float3 rightVector, float3 upVector, float3 backVector) GetBasisVectors()
        {
            return GetBasisVectors(rotation);
        }
        (float3 rightVector, float3 upVector, float3 backVector) GetBasisVectors(float3 rotation)
        {
            // --- Apply Y Rotation ---
            float3 ihat_Y = new float3(Maths.Cos(rotation.y), 0, -Maths.Sin(rotation.y));
            float3 jhat_Y = float3.yAxis;
            float3 khat_Y = new(Maths.Sin(rotation.y), 0, Maths.Cos(rotation.y));

            // --- Apply X Rotation ---
            float3 ihat_X = float3.xAxis;
            float3 jhat_X = new(0, Maths.Cos(rotation.x), Maths.Sin(rotation.x));
            float3 khat_X = new(0, -Maths.Sin(rotation.x), Maths.Cos(rotation.x));

            // --- Apply Z Rotation ---
            float3 ihat_Z = new float3(Maths.Cos(rotation.z), Maths.Sin(rotation.z), 0);
            float3 jhat_Z = new(-Maths.Sin(rotation.z), Maths.Cos(rotation.z), 0);
            float3 khat_Z = float3.zAxis;

            // --- Combied Vectors ---

            float3 rightVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), ihat_Z));
            float3 upVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), jhat_Z));
            float3 backVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), khat_Z));

            return (rightVector, upVector, backVector);
        }
        (float3 rightVector, float3 upVector, float3 backVector) RotateBasisVectors((float3 rightVector, float3 upVector, float3 backVector) bVec)
        {
            float3 rightVector = TransformVector(bVec, this.rightVector);
            float3 upVector = TransformVector(bVec, this.upVector);
            float3 backVector = TransformVector(bVec, this.backVector);

            return (rightVector, upVector, backVector);
        }
        
        public void RotateWorldAxis(float3 rotation)
        {
            rotation = GetRotationFromBasisVectors(RotateBasisVectors(GetBasisVectors(rotation)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 ToWorldPoint(float3 point)
        {
            return TransformVector(point * scale) + new float3(position.x, position.y, position.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 ToLocalPoint(float3 point)
        {
            return TransformVector((inv_rightVector, inv_upVector, inv_backVector), (point / scale) - position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 TransformVector(float3 point)
        {
            return rightVector * point.x + upVector * point.y + backVector * point.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 TransformVector((float3 rightVector, float3 upVector, float3 backVector) bVec, float3 point)
        {
            return bVec.rightVector * point.x + bVec.upVector * point.y + bVec.backVector * point.z;
        }

        public override string ToString()
        {
            return $"P:({position}) R:({rotation}) S:({scale})";
        }
    }
}