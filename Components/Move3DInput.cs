using BoboEngine;
using BoboEngine.Input;
using BoboEngine.Shaders;
using ConsoleCommand;
using GLFW;
using InputDevices;
using Cursor = BoboEngine.Input.Cursor;
using Minecraft;

public class Move3DInput : ObjectBehavior
{
    public static Move3DInput Instance { get; private set; }

    public float movementSpeed = 10f;
    public float rotationSpeed = 75f;
    public float sensitivity = 3f;

    public GameObject boxSelectionMesh;

    public BlockRaycastHit lastHit;
    public string blockSelected = "minecraft:dirt";

    public override void Start()
    {
        Instance = this;
        _3DMouse.StartReadingInput();

        _3DMouse.input.onRightInput += Mouse3DRightInput;
        _3DMouse.input.onLeftInput += Mouse3DLeftInput;

        Cursor.onMouseButtonChanged += OnMouseButtonPressed;

        boxSelectionMesh = new GameObject("BoxSelection");
        
        var boxMesh = boxSelectionMesh.AddComponent<Mesh>();
        boxMesh.LoadObjFile(Program.GetLocalModelPath("CubeOutline"));

        string shaderID = "boxSelect";

        ShaderManager.EnsureShader(shaderID, "Shader/boxSelectionShader.vert", "Shader/boxSelectionShader.frag");

        boxMesh.material = new Material(shaderID, cullBackFaces: false, renderOrder: 1);

        Cursor.mode = CursorMode.Disabled;
    }

    public override void Update()
    {
        Float3 posInput = _3DMouse.input.position;
        Float3 rotInput = _3DMouse.input.rotation;

        var w = InputSystem.GetKey(Keys.W);
        var a = InputSystem.GetKey(Keys.A);
        var s = InputSystem.GetKey(Keys.S);
        var d = InputSystem.GetKey(Keys.D);
        var space = InputSystem.GetKey(Keys.Space);
        var shift = InputSystem.GetKey(Keys.LeftShift);

        if(posInput == Float3.zero)
        {
            posInput = Float3.zero;

            if (w)
            {
                posInput += Float3.zAxis;
            }
            if (s)
            {
                posInput -= Float3.zAxis;
            }
            if (a)
            {
                posInput += Float3.xAxis;
            }
            if (d)
            {
                posInput -= Float3.xAxis;
            }
            if (space)
            {
                posInput += Float3.yAxis;
            }
            if (shift)
            {
                posInput -= Float3.yAxis;
            }

            rotInput = new(Cursor.delta.y * sensitivity, Cursor.delta.x * sensitivity, 0);
        }



        bool isUpsideDown = transform.upVector.y < 0;
        bool isZFliped = transform.rotation.z < -90 || transform.rotation.z > 90;

        transform.rotation += new Float3(rotInput.x * (isZFliped ? -1 : 1), rotInput.y * (isUpsideDown ? -1 : 1), 0) * rotationSpeed * Time.deltaTime; // -rotInput.z

        transform.position += SceneManager.currentScene.camera.transform.TransformVector(posInput) * movementSpeed * Time.deltaTime;

        lastHit = WorldDataManager.Raycast(transform.position, transform.forwardVector, 5);

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

    private void OnMouseButtonPressed(MouseInputState state)
    {
        if(state.state == GLFW.InputState.Press)
        {
            switch (state.button)
            {
                case MouseButton.Left:
                    DestroyBlock();
                    break;
                case MouseButton.Right:
                    PlaceBlock();
                    break;
                case MouseButton.Middle:
                    PickBlock();
                    break;
            }
        }
    }
    private void Mouse3DRightInput(bool down)
    {
        if (down)
        {
            PlaceBlock();
        }
    }
    private void Mouse3DLeftInput(bool down) 
    {
        if (down)
        {
            DestroyBlock();
        }
    }

    private void PlaceBlock()
    {
        if (!lastHit) return;

        Int3 blockToChange = (Int3)(lastHit.blockPosition + lastHit.blockFace.GetNormal());

        WorldDataManager.SetBlock(blockSelected, blockToChange);
    }
    private void DestroyBlock()
    {
        if (!lastHit) return;

        Int3 blockToChange = lastHit.blockPosition;

        WorldDataManager.SetBlock("minecraft:air", lastHit.blockPosition);
    }
    private static void PickBlock()
    {
        if (!Instance.lastHit) return;

        Instance.blockSelected = Instance.lastHit.blockHit.block_id;
        Program.Log($"Selected: '{Instance.blockSelected}'");
    }

    [Command("Block", "['blockID'] Sets the block to place")]
    public static void ChangeBlock(string type)
    {
        Instance.blockSelected = type;
        Program.Log($"Selected: '{type}'");
    }
}