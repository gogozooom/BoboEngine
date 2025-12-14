using BoboEngine;
using ConsoleCommand;

internal static class CmdConsoleBuffer
{
    private static readonly List<string> bufferedCommands = new();

    public static void Initialize()
    {
        WindowManager.update += BeforeRender;
        Program.ConsoleInputted += ConsoleInput;
    }
    private static void ConsoleInput(string input)
    {
        bufferedCommands.Add(input);
    }

    private static void BeforeRender()
    {
        foreach (var command in bufferedCommands.ToArray())
        {
            ConsoleCmd.HandleConsoleInput(command);
        }

        bufferedCommands.Clear();
    }
}
