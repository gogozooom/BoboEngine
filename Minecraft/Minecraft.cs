using BoboEngine;
using ConsoleCommand;
using Minecraft.Entites;
using Minecraft.World;

namespace Minecraft;

public static class Minecraft
{
    public static bool frozen = false;
    public static int tick { get; private set; }
    public static int tickRate = 20;
    public const int ticksPerSecond = 20;
    public static float partialTickTime { get; private set; }

    public static Action onTick;
    private static Task ticker;

    [OnProgramInitialize]
    public static void Initialize()
    {
        BaseVectors.FacingTowards(Float3.zAxis, Float3.yAxis);

        TextureManager.GenerateAtlas();
        BlockTypeManager.GenerateBlockData();

        Program.Log("Minecraft Initialized!");

        SceneManager.sceneLoaded += OnSceneLoad;
    }

    public static void OnSceneLoad()
    {
        WorldChunkManager.GenerateTestChunk();
        SceneManager.currentScene.skybox.gameObject.AddComponent<Sky>();

        new GameObject("Player").AddComponent<Player>();

        if (ticker != null)
        {
            Program.LogError("Cannot start another tick timer!");
            return;
        }
        ticker = Task.Run(Ticker);
    }

    public static void Ticker()
    {
        var lastTicked = Time.time;

        while (true)
        {
            partialTickTime = (Time.time - lastTicked) / (1f / tickRate);

            if (partialTickTime >= 1)
            {
                lastTicked = Time.time;
                tick++;
                try
                {
                    onTick?.Invoke();
                }
                catch (Exception ex)
                {
                    Program.LogError($"{ex.Message}");
                }
            }

            while (frozen);
        }
    }

    [Command("tick")]
    public static void TickCommand(string type, int value)
    {
        switch (type)
        {
            case null:
            case "":
                Program.Log($"Current tick rate is '{tickRate}'");
                break;
            case "rate":
                tickRate = value;
                Program.Log($"Tick rate set to '{tickRate}'");
                break;
            case "freeze":
                frozen = true;
                Program.Log($"Tick rate froze set to '{frozen}'");
                break;
            case "unfreeze":
                frozen = false;
                Program.Log($"Tick rate froze set to '{frozen}'");
                break;
            default:
                Program.LogWarning($"Command type: '{type}' not recogized!");
                break;
        }
    }
}