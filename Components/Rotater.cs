using BoboEngine;

public class Rotater : ObjectBehavior
{
    public override void Update()
    {
        transform.yaw += Time.deltaTime * 50f;
    }
}
