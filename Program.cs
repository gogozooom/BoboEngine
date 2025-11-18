using BoboEngine;
using BoboEngine.Input;
using BoboEngine.Shaders;
using ConsoleCommand;
using System.Diagnostics;
using System.Reflection;

internal static class Program
{
    /// <summary>
    /// Title of the window
    /// </summary>
    public const string TITLE = "BoboEngine";

    public static string ExecutablePath { get; private set; }
    public static string ProgramDirectory { get; private set; }

    public static Action<string> ConsoleInputted;

    static void Main(string[] args)
    {
        // Set Paths
        ExecutablePath = Environment.ProcessPath;
        ProgramDirectory = Directory.GetCurrentDirectory();

        Console.ForegroundColor = DEFAULT_FORGROUND_COLOR;

        ConsoleCmd.Initialize();

        // Read Console Loop
        Task.Run(() =>
        {
            while (true)
            {
                string? input = Console.ReadLine();

                if (input != null)
                {
                    if(SceneManager.currentScene == null)
                    {
                        ConsoleCmd.HandleConsoleInput(input);
                    }
                    else // !! FW Use better implementation down the line
                    {
                        ConsoleInputted?.Invoke(input);
                    }
                }
            }
        });

        StartProgram();
    }

    public static void Test(float input, float expected)
    {
        if (input == expected)
        {
            Log($"Passed test!  E: '{expected}' == R: '{input}' !");
        }
        else
        {
            LogWarning($"Failed test! E: '{expected}' == R: '{input}' !");
        }
    }

    public static void StartProgram()
    {
        WindowManager.InitializeRenderLoop(800, 600);
    }

    public static string GetLocalModelPath(string model = null)
    {
        if (model == null) return Path.Combine(ProgramDirectory, "Models");

        return Path.Combine(ProgramDirectory, "Models", model + ".obj");
    }
    public static string GetLocalTexturePath(string texture = null)
    {
        if (texture == null) return Path.Combine(ProgramDirectory, "Textures");

        return Path.Combine(ProgramDirectory, "Textures", texture + ".bmp");
    }

    #region ConsoleLogging
    public static void LogLine()
    {
        Console.WriteLine();
    }

    const ConsoleColor DEFAULT_FORGROUND_COLOR = ConsoleColor.White;
    const int FO_START_SPACE = 7;
    const int FO_AFTER_SPACE = 10;
    static string FormatOrigin(MethodBase from, string type)
    {
        string result = "[";

        result += type;

        int spaceLeft = FO_START_SPACE - type.Length;

        for (int i = 0; i < spaceLeft; i++) // Hopfully negative numbers just get ignored?
        {
            result += ' ';
        }

        string finalName = from.ReflectedType.Name;

        if (finalName.Equals("<>c", StringComparison.OrdinalIgnoreCase)) // Avoid strange logs
        {
            finalName = "*Action";
        }

        result += ':';

        int afterSpaceToAdd = FO_AFTER_SPACE - finalName.Length;

        for (int i = 0; i < afterSpaceToAdd; i++)
        {
            result += ' ';
        }

        return result + $"{finalName}]";
    }
    static void Log(MethodBase from, string type, object message, ConsoleColor color = ConsoleColor.Gray)
    {
        var className = from.ReflectedType.Name;

        Console.ForegroundColor = color;
        Console.WriteLine($"{FormatOrigin(from, type)} {message}");
        Console.ForegroundColor = DEFAULT_FORGROUND_COLOR;
    }
    public static void Log(object message)
    {
        var methodInfo = new StackTrace().GetFrame(1).GetMethod();

        Log(methodInfo, "Info", message, ConsoleColor.Gray);
    }
    public static void LogMessage(object message)
    {
        var methodInfo = new StackTrace().GetFrame(1).GetMethod();

        Log(methodInfo, "Message", message, ConsoleColor.White);
    }
    public static void LogWarning(object message)
    {
        var methodInfo = new StackTrace().GetFrame(1).GetMethod();

        Log(methodInfo, "Warning", message, ConsoleColor.Yellow);
    }
    public static void LogError(object message)
    {
        var methodInfo = new StackTrace().GetFrame(1).GetMethod();

        Log(methodInfo, "Error", message, ConsoleColor.Red);
    }
    #endregion
}