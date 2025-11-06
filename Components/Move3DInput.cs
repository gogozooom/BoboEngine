using BoboEngine;
using InputDevices;

public class Move3DInput : ObjectBehavior
{
    public override void Start()
    {
        _3DMouse.StartReadingInput();
    }

    public override void Update()
    {
        Float3 posInput = _3DMouse.input.position;
        Float3 rotInput = _3DMouse.input.rotation;

        transform.rotation += new Float3(rotInput.x, -rotInput.y, 0) * 75f * Time.deltaTime; // -rotInput.z
        //transform.position += SceneManager.currentScene.camera.transform.TransformVector(new Float3(-posInput.x, posInput.y, posInput.z)) * 3 * Time.deltaTime;

        if (_3DMouse.input.rightPressed)
        {
            transform.Reset();
        }
    }
}
