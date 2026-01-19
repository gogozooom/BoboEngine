using System.Numerics;

namespace BoboEngine;

public class Transform : ObjectBehavior
{
    public Transform parent { get => _parent; set => SetParent(value); }

    private Transform _parent;

    private Float3 _position;
    private Float3 _scale = Float3.one;
    private float _pitch;
    private float _yaw;
    private float _roll;

    public Float3 position { get => _position; set => SetPosition(value); }
    public Float3 rotation { get => new(pitch, yaw, roll); set => SetRotation(value); }
    public Float3 scale { get => _scale; set => SetScale(value); }

    /// <summary>
    /// OR x rotation
    /// </summary>
    public float pitch { get => _pitch; set => SetRotation(new(value, _yaw, _roll)); }
    /// <summary>
    /// OR y rotation
    /// </summary>
    public float yaw { get => _yaw; set => SetRotation(new(_pitch, value, _roll)); }
    /// <summary>
    /// OR z rotation
    /// </summary>
    public float roll { get => _roll; set => SetRotation(new(_pitch, _yaw, value)); }

    public BaseVectors baseVectors;
    public BaseVectors inv_baseVectors;

    // TODO: Cache Matrix! Add support for static objects!
    public Matrix4x4 MatrixTrans => Matrix4x4.CreateTranslation(new Vector3(position.x, position.y, position.z));
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

    private void SetParent(Transform parent) // TODO: When actuall transformation stuff gets worked on make proper translation stuff
    {
        _parent = parent;
    }
    public void SetRotation(Float3 rotation)
    {
        _pitch = Maths.WrapAbs(rotation.x, 180);
        _yaw = Maths.WrapAbs(rotation.y, 180);
        _roll = Maths.WrapAbs(rotation.z, 180);

        baseVectors = GetBasisVectors();
        inv_baseVectors = GetInverseBasisVectors();
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
    BaseVectors GetInverseBasisVectors()
    {
        return new(new Float3(baseVectors.leftVector.x, baseVectors.upVector.x, baseVectors.forwardVector.x),
                   new Float3(baseVectors.leftVector.y, baseVectors.upVector.y, baseVectors.forwardVector.y),
                   new Float3(baseVectors.leftVector.z, baseVectors.upVector.z, baseVectors.forwardVector.z));
    }
    BaseVectors GetBasisVectors()
    {
        return BaseVectors.FromRotation(rotation);
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
        return baseVectors.TransformVector(point);
    }

    public Float3 TransformVector(Float3 point)
    {
        return baseVectors.TransformVector(point);
    }

    public override string ToString()
    {
        return $"P:({position}) R:({rotation}) S:({scale})";
    }
}