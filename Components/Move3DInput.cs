using BoboEngine;
using BoboEngine.Shaders;
using InputDevices;

public class Move3DInput : ObjectBehavior
{
    public float speed = 10f;
    public float rotationSpeed = 75f;

    public GameObject obj;

    public BlockRaycastHit lastHit;
    public string blockSelected = "minecraft:dirt";

    public override void Start()
    {
        _3DMouse.StartReadingInput();

        _3DMouse.input.onRightInput += RightPressed;
        _3DMouse.input.onLeftInput += LeftPressed;

        obj = new GameObject("Debug");
        obj.transform.scale = Float3.one / 5;

        var mesh = obj.AddComponent<Mesh>();

        mesh.LoadObjFile(Program.GetLocalModelPath("Cube"));
        mesh.shader = new Shader(new Texture(Program.GetLocalTexturePath("NULL")));
    }

    public override void Update()
    {
        Float3 posInput = _3DMouse.input.position;
        Float3 rotInput = _3DMouse.input.rotation;

        bool isUpsideDown = transform.upVector.y < 0;
        bool isZFliped = transform.rotation.z < -90 || transform.rotation.z > 90;

        transform.rotation += new Float3(rotInput.x * (isZFliped ? -1 : 1), rotInput.y * (isUpsideDown ? -1 : 1), 0) * rotationSpeed * Time.deltaTime; // -rotInput.z

        transform.position += SceneManager.currentScene.camera.transform.TransformVector(posInput) * speed * Time.deltaTime;

        lastHit = WorldDataManager.Raycast(transform.position, transform.forwardVector, 16);

        obj.transform.position = lastHit.hitPosition;

        //Program.Log(result);


        //Program.Log(transform.position);
    }

    private void RightPressed(bool down)
    {
        if (down && lastHit)
        {
            Int3 blockToChange = lastHit.blockPosition;

            WorldDataManager.SetBlock("minecraft:air", lastHit.blockPosition);
        }
    }
    private void LeftPressed(bool down) 
    {
        if (down && lastHit)
        {
            Program.Log(lastHit.blockFace);

            Int3 blockToChange = (Int3)(lastHit.blockPosition + lastHit.blockFace.GetNormal());

            WorldDataManager.SetBlock(blockSelected, blockToChange);
        }
    }
}
