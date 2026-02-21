using BoboEngine;

public class Rotater : ObjectBehavior
{
    Float3 startPosition;
    public override void Start()
    {
        startPosition = transform.position;
    }
    public override void Update()
    {
        transform.yaw += Time.deltaTime * 50f;
        transform.position = startPosition + new Float3(0, 0.5f + Maths.Sin(Time.time * 120f)/2, 0);
    }
}
