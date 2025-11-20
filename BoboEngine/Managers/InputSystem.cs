using GLFW;
using Microsoft.VisualBasic;
using System.Data;

namespace BoboEngine.Input;
public static class InputSystem
{
    public static Action<InputState> onKeyChanged;
    public static Action<InputState> onKeyUp;
    public static Action<InputState> onKeyDown;

    private static readonly Dictionary<Keys, bool> _inputStates = new();

    public static bool GetKey(Keys key)
    {
        if (_inputStates.TryGetValue(key, out var s))
        {
            return s;
        }

        _inputStates.Add(key, false);
        return false;
    }

    public static void key_callback(Window window, Keys key, int scanCode, GLFW.InputState state, ModifierKeys mods)
    {
        var inputState = new InputState(key, state, mods);

        onKeyChanged?.Invoke(inputState);

        var inputDown = true;

        if (state == GLFW.InputState.Press)
        {
            onKeyDown?.Invoke(inputState);
        }
        else if(state == GLFW.InputState.Release)
        {
            onKeyUp?.Invoke(inputState);
            inputDown = false;
        }

        if (!_inputStates.TryAdd(key, inputDown))
        {
            _inputStates[key] = inputDown;
        }
    }


}
public static class Cursor
{
    public static Int2 position { get => _position; set => SetCursorPosition(value); }
    private static Int2 _position;

    public static Int2 delta { get; private set; }
    public static CursorMode mode { get => _mode; set => SetCursorMode(value); }
    private static CursorMode _mode;

    public static Action<MouseInputState> onMouseButtonChanged;
    public static Action<MouseInputState> onMouseButtonDown;
    public static Action<MouseInputState> onMouseButtonUp;
    public static Action<Float2> onScroll;

    public static void mouse_button_callback(Window window, MouseButton button, GLFW.InputState state, ModifierKeys mods)
    {
        var inputState = new MouseInputState(button, state, mods);

        onMouseButtonChanged?.Invoke(inputState);

        if (state == GLFW.InputState.Press)
        {
            onMouseButtonDown?.Invoke(inputState);
        }
        else if (state == GLFW.InputState.Release)
        {
            onMouseButtonUp?.Invoke(inputState);
        }
    }
    public static void mouse_scroll_callback(Window window, double x, double y)
    {
        onScroll?.Invoke(new((float)x,(float)y));
    }

    private static Int2 lastPosition = position;
    public static void cursor_position_callback(Window window, double xPos, double yPos)
    {
        position = new((int)xPos, (int)yPos);

        delta = position - lastPosition;

        lastPosition = position;
    }

    public static void Reset()
    {
        delta = new(0, 0);
    }

    private static void SetCursorMode(CursorMode v)
    {
        switch (v)
        {
            case CursorMode.Hidden:
                Glfw.SetInputMode(WindowManager.Window, InputMode.Cursor, (int)CursorMode.Hidden);
                break;
            case CursorMode.Disabled:
                Glfw.SetInputMode(WindowManager.Window, InputMode.Cursor, (int)CursorMode.Disabled);
                Glfw.SetInputMode(WindowManager.Window, InputMode.RawMouseMotion, 1);
                break;
            default:
                Glfw.SetInputMode(WindowManager.Window, InputMode.Cursor, (int)CursorMode.Normal);
                break;
        }

        _mode = v;
    }
    private static void SetCursorPosition(Int2 v)
    {
        Glfw.SetCursorPosition(WindowManager.Window, v.x, v.y);

        _position = v;
    }
}
public struct InputState
{
    public readonly Keys key;
    public readonly GLFW.InputState state;
    public readonly ModifierKeys modifierKeys;

    public InputState(Keys key, GLFW.InputState state, ModifierKeys modifierKeys)
    {
        this.key = key;
        this.state = state;
        this.modifierKeys = modifierKeys;
    }
}
public struct MouseInputState
{
    public readonly MouseButton button;
    public readonly GLFW.InputState state;
    public readonly ModifierKeys modifierKeys;

    public MouseInputState(MouseButton button, GLFW.InputState state, ModifierKeys modifierKeys)
    {
        this.button = button;
        this.state = state;
        this.modifierKeys = modifierKeys;
    }
}