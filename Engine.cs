using ConsoleCommand;
using StbImageSharp;
using System.Diagnostics;
using System.Reflection;

namespace BoboEngine;
public static class Engine
{
    /// <summary>
    /// Title of the window
    /// </summary>
    public const string TITLE = "BoboEngine";

    public static string ExecutablePath { get; private set; }
    public static string ProgramDirectory { get; private set; }

    public static Action<string> ConsoleInputted;

    public static void Start()
    {
        // Set Paths
        ExecutablePath = Environment.ProcessPath;
        ProgramDirectory = Directory.GetCurrentDirectory();

        Console.ForegroundColor = DEFAULT_FORGROUND_COLOR;

        ConsoleCmd.Initialize();
        StbImage.stbi_set_flip_vertically_on_load(1);

        // Read Console Loop
        Task.Run(() =>
        {
            while (true)
            {
                string? input = Console.ReadLine();

                if (input != null)
                {
                    ConsoleInputted?.Invoke(input);

                    if (!WindowManager.Initialized)
                    {
                        // Safe to run commands!
                        ConsoleCmd.HandleConsoleInput(input);
                    }
                }
            }
        });

        InitializeProgram();
    }

    public static void InitializeProgram()
    {
        var methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsClass)
            .SelectMany(x => x.GetMethods())
            .Where(x => x.GetCustomAttributes(typeof(OnEngineInitializeAttribute), false).FirstOrDefault() != null);

        foreach (var method in methods)
        {
            method.Invoke(null, []);
        }

        WindowManager.InitializeRenderLoop(854, 480);
    }

    public static string GetLocalModelPath(string model = null)
    {
        if (model == null) return Path.Combine(ProgramDirectory, "Models");

        return Path.Combine(ProgramDirectory, "Models", model + ".obj");
    }
    public static string GetLocalTexturePath(string texture = null)
    {
        if (texture == null) return Path.Combine(ProgramDirectory, "Textures");

        if (texture.Split('.').Length == 1) texture += ".bmp";

        return Path.Combine(ProgramDirectory, "Textures", texture);
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

/// <summary>
/// Will fire the connected static method once the program starts
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class OnEngineInitializeAttribute : Attribute {}