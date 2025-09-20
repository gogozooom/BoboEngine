using BoboEngine;
using InputDevices;

public class FreeCam : ObjectBehavior
{
    Camera camera;

    public override void Start()
    {
        camera = gameObject.GetComponent<Camera>();

        _3DMouse.StartReadingInput();
    }

    public override void Update()
    {
        Float3 posInput = _3DMouse.input.position;
        Float3 rotInput = _3DMouse.input.rotation;

        camera.transform.rotation += new Float3(rotInput.x, -rotInput.y, -rotInput.z) * 75f * Time.deltaTime;
        camera.transform.position += camera.transform.TransformVector(new Float3(-posInput.x, posInput.y, posInput.z)) * 3 * Time.deltaTime;

        if (_3DMouse.input.rightPressed)
        {
            camera.transform.Reset();
        }
    }
}
