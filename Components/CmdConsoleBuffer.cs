using BoboEngine;
using ConsoleCommand;

internal class CmdConsoleBuffer : ObjectBehavior
{
    private List<string> bufferedCommands = new List<string>();

    public override void Start()
    {
        Program.ConsoleInputted += ConsoleInput;
    }
    private void ConsoleInput(string input)
    {
        bufferedCommands.Add(input);
    }

    public override void Update()
    {
        foreach (var command in bufferedCommands)
        {
            ConsoleCmd.HandleConsoleInput(command);
        }

        bufferedCommands.Clear();
    }
}
