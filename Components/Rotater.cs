using BoboEngine;

public class Rotater : ObjectBehavior
{
    public override void Update()
    {
        transform.yaw += Time.deltaTime * 50f;
        transform.position = new(-2, (2 + Maths.Sin(Time.time * 120f))/2, 2);
    }
}
