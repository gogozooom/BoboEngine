using System.Numerics;

namespace BoboEngine;

public class Transform : ObjectBehavior
{
    Float3 _position;
    Float3 _scale = Float3.one;
    public float _pitch;
    public float _yaw;
    public float _roll;

    public Float3 position { get => _position; set => SetPosition(value); }
    public Float3 rotation { get => new(pitch, yaw, roll); set => SetRotation(value); }
    public Float3 scale { get => _scale; set => SetScale(value); }

    public float pitch { get => _pitch; set => SetRotation(new(value, _yaw, _roll)); }
    public float yaw { get => _yaw; set => SetRotation(new(_pitch, value, _roll)); }
    public float roll { get => _roll; set => SetRotation(new(_pitch, _yaw, value)); }

    public Float3 leftVector { get; private set; }
    public Float3 upVector { get; private set; }
    public Float3 forwardVector { get; private set; }

    public Float3 inv_leftVector { get; private set; }
    public Float3 inv_upVector { get; private set; }
    public Float3 inv_forwardVector { get; private set; }

    // TODO: Cache Matrix! Add support for static objects!
    public Matrix4x4 MatrixTrans => Matrix4x4.CreateTranslation(new Vector3(position.x, position.y, position.z)); // Invert x to align with unity
    public Matrix4x4 MatrixScale => Matrix4x4.CreateScale(new Vector3(1, 1, 1)) * Matrix4x4.CreateScale(scale);
    public Matrix4x4 MatrixRot => Matrix4x4.CreateRotationZ(Maths.ToRad(-roll)) * Matrix4x4.CreateRotationX(Maths.ToRad(pitch)) * Matrix4x4.CreateRotationY(Maths.ToRad(-yaw));
    public Matrix4x4 MatrixInverseRot => Matrix4x4.CreateRotationY(Maths.ToRad(yaw)) * Matrix4x4.CreateRotationX(Maths.ToRad(-pitch)) * Matrix4x4.CreateRotationZ(Maths.ToRad(roll));

    public Matrix4x4 Matrix => MatrixScale * MatrixRot * MatrixTrans;
    
    public Transform()
    {
        SetPosition(_position);
        SetRotation(new(pitch, yaw, roll));
        SetScale(_scale);
    }
    public Transform(Float3 position, Float3 rotation, Float3 scale)
    {
        SetPosition(position);
        SetRotation(rotation);
        SetScale(scale);
    }

    public void SetRotation(Float3 rotation)
    {
        _pitch = Maths.WrapAbs(rotation.x, 180);
        _yaw = Maths.WrapAbs(rotation.y, 180);
        _roll = Maths.WrapAbs(rotation.z, 180);

        (leftVector, upVector, forwardVector) = GetBasisVectors();
        (inv_leftVector, inv_upVector, inv_forwardVector) = GetInverseBasisVectors();
    }
    public void SetPosition(Float3 position)
    {
        _position = position;
    }
    public void SetScale(Float3 scale)
    {
        _scale = scale;
    }
    public void Reset()
    {
        _position = new(0, 0, 0);
        rotation = new(0, 0, 0);
    }
    (Float3 leftVector, Float3 upVector, Float3 forwardVector) GetInverseBasisVectors()
    {
        return (new(leftVector.x, upVector.x, forwardVector.x),
                new(leftVector.y, upVector.y, forwardVector.y),
                new(leftVector.z, upVector.z, forwardVector.z));
    }
    (Float3 leftVector, Float3 upVector, Float3 forwardVector) GetBasisVectors()
    {
        return GetBasisVectors(rotation);
    }

    public Float3 ToWorldPoint(Float3 point)
    {
        return TransformVector(point * scale) + new Float3(position.x, position.y, position.z);
    }

    public Float3 ToLocalPoint(Float3 point)
    {
        return TransformVectorInv((point / scale) - position);
    }

    public Float3 TransformVectorInv(Float3 point)
    {
        return TransformVector((inv_leftVector, inv_upVector, inv_forwardVector), point);
    }

    public Float3 TransformVector(Float3 point)
    {
        return TransformVector((leftVector, upVector, forwardVector), point);
    }

    public static (Float3 leftVector, Float3 upVector, Float3 forwardVector) GetBasisVectors(Float3 rotation)
    {
        // --- Apply Y Rotation ---
        Float3 ihat_Y = new Float3(Maths.Cos(rotation.y), 0, Maths.Sin(rotation.y));
        Float3 jhat_Y = Float3.yAxis;
        Float3 khat_Y = new(Maths.Sin(-rotation.y), 0, Maths.Cos(rotation.y));

        // --- Apply X Rotation ---
        Float3 ihat_X = Float3.xAxis;
        Float3 jhat_X = new(0, Maths.Cos(rotation.x), Maths.Sin(rotation.x));
        Float3 khat_X = new(0, -Maths.Sin(rotation.x), Maths.Cos(rotation.x));

        // --- Apply Z Rotation ---
        Float3 ihat_Z = new Float3(Maths.Cos(rotation.z), Maths.Sin(rotation.z), 0);
        Float3 jhat_Z = new(-Maths.Sin(rotation.z), Maths.Cos(rotation.z), 0);
        Float3 khat_Z = Float3.zAxis;

        // --- Combied Vectors ---

        Float3 leftVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), ihat_Z));
        Float3 upVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), jhat_Z));
        Float3 forwardVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), khat_Z));

        return (leftVector, upVector, forwardVector);
    }
    public static Float3 TransformVector((Float3 leftVector, Float3 upVector, Float3 forwardVector) bVec, Float3 point)
    {
        return bVec.leftVector * point.x + bVec.upVector * point.y + bVec.forwardVector * point.z;
    }


    public override string ToString()
    {
        return $"P:({position}) R:({rotation}) S:({scale})";
    }
}