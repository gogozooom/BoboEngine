using BoboEngine;
using ComputeSharp.Resources;
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

        bool isUpsideDown = transform.upVector.y < 0;
        bool isZFliped = transform.rotation.z < -90 || transform.rotation.z > 90;

        transform.rotation += new Float3(rotInput.x * (isZFliped ? -1 : 1), rotInput.y * (isUpsideDown ? -1 : 1), rotInput.z) * rotationSpeed * Time.deltaTime; // -rotInput.z


        transform.position += SceneManager.currentScene.camera.transform.TransformVector(posInput) * speed * Time.deltaTime;

        //Program.Log(transform.position);

        if (_3DMouse.input.rightPressed)
        {
            transform.Reset();
        }
    }
}
