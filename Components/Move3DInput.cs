using BoboEngine;
using BoboEngine.Input;
using BoboEngine.Shaders;
using ConsoleCommand;
using GLFW;
using InputDevices;
using Minecraft;
using Cursor = BoboEngine.Input.Cursor;

public class Move3DInput : ObjectBehavior
{
    public static Move3DInput Instance { get; private set; }

    public float flyingSpeed = 10.92f; // Minecraft Flying Speed
    public float acceleration = 1.4f;
    public float flyingSpeedVertical = 7f; // Minecraft Flying Speed
    public float accelerationY = 4f;
    public float airFriction = 1.80f; // Origonal minecraft air friction = 0.09 (X 20 for from tick to seconds)
    public float yAirFriction = 15.20f;
    public float sensitivity = 1f;

    public GameObject boxSelectionMesh;

    public BlockRaycastHit lastHit;
    public string blockSelected = "minecraft:dirt";

    public Float3 velocity;

    public override void Start()
    {
        Instance = this;
        _3DMouse.StartReadingInput();

        _3DMouse.input.onRightInput += Mouse3DRightInput;
        _3DMouse.input.onLeftInput += Mouse3DLeftInput;

        Cursor.onMouseButtonChanged += OnMouseButtonPressed;
        Cursor.onScroll += OnMouseScroll;

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
        UpdateMovment();
        UpdateRaycast();
    }
    private void UpdateMovment()
    {
        Float3 rotInput = GetRotationInput();
        transform.rotation = new Float3(Math.Clamp(transform.rotation.x + rotInput.x, -90f, 90f), transform.rotation.y + rotInput.y, 0);

        Float3 posInput = Transform.GetYRotationBasisVectors(transform.yaw).TransformVector(GetPosInput());

        Float3 inputForce = new Float3(posInput.x * flyingSpeed * acceleration, posInput.y * flyingSpeedVertical * accelerationY, posInput.z * flyingSpeed * acceleration);
        
        velocity += inputForce * Time.deltaTime;

        transform.position += velocity * Time.deltaTime;

        UpdateDrag(inputForce);
    }

    public void UpdateRaycast()
    {
        lastHit = WorldDataManager.Raycast(transform.position, transform.baseVectors.forwardVector, 5);

        if (lastHit)
        {
            boxSelectionMesh.enabled = true;
            boxSelectionMesh.transform.position = lastHit.blockPosition + new Float3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            boxSelectionMesh.enabled = false;
        }
    }


    public void UpdateDrag(Float3 inputForce)
    {
        float xDrag = 0;

        bool doXDrag = inputForce.x == 0
            || MathF.Abs(velocity.x) > flyingSpeed
            || MathF.Sign(inputForce.x) != MathF.Sign(velocity.x);

        if (doXDrag) xDrag = velocity.x * airFriction;

        float yDrag = 0;

        bool doYDrag = inputForce.y == 0
            || MathF.Abs(velocity.y) > flyingSpeedVertical
            || MathF.Sign(inputForce.y) != MathF.Sign(velocity.y);

        if (doYDrag) yDrag = velocity.y * yAirFriction;

        float zDrag = 0;

        bool doZDrag = inputForce.z == 0
            || MathF.Abs(velocity.z) > flyingSpeed
            || MathF.Sign(inputForce.z) != MathF.Sign(velocity.z);

        if (doZDrag) zDrag = velocity.z * airFriction;

        Float3 drag = new Float3(xDrag, yDrag, zDrag);

        velocity -= drag * Time.deltaTime;
    }

    private Float3 GetPosInput()
    {
        Float3 posInput = Float3.zero;

        var w = InputSystem.GetKey(Keys.W);
        var a = InputSystem.GetKey(Keys.A);
        var s = InputSystem.GetKey(Keys.S);
        var d = InputSystem.GetKey(Keys.D);
        var space = InputSystem.GetKey(Keys.Space);
        var shift = InputSystem.GetKey(Keys.LeftShift);

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

        return posInput;
    }
    private Float3 GetRotationInput() => new Float3(Cursor.delta.y, Cursor.delta.x, 0) * (0.12f * sensitivity); // 0.12f resonable constant from pixels to sensitivity value

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
    private void OnMouseScroll(Float2 scroll)
    {
        Camera.main.fov -= scroll.y*2f;
    }

    private void PlaceBlock()
    {
        if (!lastHit) return;

        Int3 blockToChange = (Int3)(lastHit.blockPosition + lastHit.blockFace.GetNormal());

        WorldDataManager.SetBlock(blockToChange, blockSelected);
    }
    private void DestroyBlock()
    {
        if (!lastHit) return;

        Int3 blockToChange = lastHit.blockPosition;

        WorldDataManager.SetBlock(lastHit.blockPosition, "minecraft:air");
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