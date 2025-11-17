using BoboEngine;
using BoboEngine.Shaders;
using ConsoleCommand;
using InputDevices;

public class Move3DInput : ObjectBehavior
{
    public static Move3DInput Instance { get; private set; }

    public float speed = 10f;
    public float rotationSpeed = 75f;

    public GameObject obj;
    public GameObject boxSelectionMesh;

    public BlockRaycastHit lastHit;
    public string blockSelected = "minecraft:dirt";

    public override void Start()
    {
        Instance = this;
        _3DMouse.StartReadingInput();

        _3DMouse.input.onRightInput += RightPressed;
        _3DMouse.input.onLeftInput += LeftPressed;

        obj = new GameObject("Debug");
        obj.transform.scale = Float3.one / 5;

        var mesh = obj.AddComponent<Mesh>();

        mesh.LoadObjFile(Program.GetLocalModelPath("Cube"));
        mesh.shader = new Shader(new Texture(Program.GetLocalTexturePath("NULL")));


        boxSelectionMesh = new GameObject("BoxSelection");
        
        var boxMesh = boxSelectionMesh.AddComponent<Mesh>();

        boxMesh.LoadObjFile(Program.GetLocalModelPath("Cube"));
        boxMesh.shader = new Shader(null, "Shader/boxSelectionShader.vert", "Shader/boxSelectionShader.frag");
        boxMesh.shader.renderMode = RenderMode.lineStrip;
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

        if (lastHit)
        {
            boxSelectionMesh.enabled = true;
            boxSelectionMesh.transform.position = lastHit.blockPosition + new Float3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            boxSelectionMesh.enabled = false;
        }

        //Program.Log(result);

        //Program.Log(transform.position);
    }

    private void RightPressed(bool down)
    {
        if (down && lastHit)
        {
            Int3 blockToChange = (Int3)(lastHit.blockPosition + lastHit.blockFace.GetNormal());

            WorldDataManager.SetBlock(blockSelected, blockToChange);
        }
    }
    private void LeftPressed(bool down) 
    {
        if (down && lastHit)
        {
            Int3 blockToChange = lastHit.blockPosition;

            WorldDataManager.SetBlock("minecraft:air", lastHit.blockPosition);
        }
    }

    [Command("Block", "['blockID'] Sets the block to place")]
    public static void ChangeBlock(string type)
    {
        Instance.blockSelected = type;
        Program.Log($"Selected: '{type}'");
    }

    [Command("Pick", "Sets the block to place to whatever you're looking at!")]
    public static void PickBlock()
    {
        Instance.blockSelected = Instance.lastHit.blockHit.block_id;
        Program.Log($"Selected: '{Instance.blockSelected}'");
    }
}