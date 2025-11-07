using BoboEngine;
using InputDevices;

public class Move3DInput : ObjectBehavior
{
    public float speed = 10f;
    public float rotationSpeed = 75f;

    public GameObject obj;

    public override void Start()
    {
        _3DMouse.StartReadingInput();
    }

    public override void Update()
    {
        Float3 posInput = _3DMouse.input.position;
        Float3 rotInput = _3DMouse.input.rotation;

        transform.rotation += new Float3(rotInput.x, rotInput.y, 0) * rotationSpeed * Time.deltaTime; // -rotInput.z
        transform.position += SceneManager.currentScene.camera.transform.TransformVector(posInput) * speed * Time.deltaTime;

        //Program.Log(transform.position);

        if (_3DMouse.input.rightPressed)
        {
            transform.Reset();
        }
    }
}
