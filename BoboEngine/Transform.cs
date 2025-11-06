using System.Runtime.CompilerServices;

namespace BoboEngine;

public class Transform
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

    public Float3 rightVector { get; private set; }
    public Float3 upVector { get; private set; }
    public Float3 backVector { get; private set; }

    public Float3 inv_rightVector { get; private set; }
    public Float3 inv_upVector { get; private set; }
    public Float3 inv_backVector { get; private set; }

    public Transform()
    {
        SetPosition(_position);
        SetRotation(new(pitch, yaw, roll));
        SetScale(_scale);
    }
    public Transform(Float3 position, Float3 rotation, Float3 scale)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    public void SetRotation(Float3 rotation)
    {
        _pitch = rotation.x;
        _yaw = rotation.y;
        _roll = rotation.z;

        (rightVector, upVector, backVector) = GetBasisVectors();
        (inv_rightVector, inv_upVector, inv_backVector) = GetInverseBasisVectors();
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
    Float3 GetRotationFromBasisVectors((Float3 rightVector, Float3 upVector, Float3 backVector) bVec)
    {
        throw new NotImplementedException(); // Math too big brain for me...
        return new();
    }
    (Float3 rightVector, Float3 upVector, Float3 backVector) GetInverseBasisVectors()
    {
        return (new(rightVector.x, upVector.x, backVector.x),
                new(rightVector.y, upVector.y, backVector.y),
                new(rightVector.z, upVector.z, backVector.z));
    }
    (Float3 rightVector, Float3 upVector, Float3 backVector) GetBasisVectors()
    {
        return GetBasisVectors(rotation);
    }
    (Float3 rightVector, Float3 upVector, Float3 backVector) GetBasisVectors(Float3 rotation)
    {
        // --- Apply Y Rotation ---
        Float3 ihat_Y = new Float3(Maths.Cos(rotation.y), 0, -Maths.Sin(rotation.y));
        Float3 jhat_Y = Float3.yAxis;
        Float3 khat_Y = new(Maths.Sin(rotation.y), 0, Maths.Cos(rotation.y));

        // --- Apply X Rotation ---
        Float3 ihat_X = Float3.xAxis;
        Float3 jhat_X = new(0, Maths.Cos(rotation.x), Maths.Sin(rotation.x));
        Float3 khat_X = new(0, -Maths.Sin(rotation.x), Maths.Cos(rotation.x));

        // --- Apply Z Rotation ---
        Float3 ihat_Z = new Float3(Maths.Cos(rotation.z), Maths.Sin(rotation.z), 0);
        Float3 jhat_Z = new(-Maths.Sin(rotation.z), Maths.Cos(rotation.z), 0);
        Float3 khat_Z = Float3.zAxis;

        // --- Combied Vectors ---

        Float3 rightVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), ihat_Z));
        Float3 upVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), jhat_Z));
        Float3 backVector = TransformVector((ihat_Y, jhat_Y, khat_Y), TransformVector((ihat_X, jhat_X, khat_X), khat_Z));

        return (rightVector, upVector, backVector);
    }
    (Float3 rightVector, Float3 upVector, Float3 backVector) RotateBasisVectors((Float3 rightVector, Float3 upVector, Float3 backVector) bVec)
    {
        Float3 rightVector = TransformVector(bVec, this.rightVector);
        Float3 upVector = TransformVector(bVec, this.upVector);
        Float3 backVector = TransformVector(bVec, this.backVector);

        return (rightVector, upVector, backVector);
    }
    
    public void RotateWorldAxis(Float3 rotation)
    {
        rotation = GetRotationFromBasisVectors(RotateBasisVectors(GetBasisVectors(rotation)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Float3 ToWorldPoint(Float3 point)
    {
        return TransformVector(point * scale) + new Float3(position.x, position.y, position.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Float3 ToLocalPoint(Float3 point)
    {
        return TransformVectorInv((point / scale) - position);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Float3 TransformVectorInv(Float3 point)
    {
        return TransformVector((inv_rightVector, inv_upVector, inv_backVector), point);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Float3 TransformVector(Float3 point)
    {
        return TransformVector((rightVector, upVector, backVector), point);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float3 TransformVector((Float3 rightVector, Float3 upVector, Float3 backVector) bVec, Float3 point)
    {
        return bVec.rightVector * point.x + bVec.upVector * point.y + bVec.backVector * point.z;
    }

    public override string ToString()
    {
        return $"P:({position}) R:({rotation}) S:({scale})";
    }
}