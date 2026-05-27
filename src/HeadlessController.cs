using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace STS2LudicrousSpeed;

// Central orchestrator for headless mode.
// Activate by launching the game with --headless or by setting STS2_HEADLESS=1.
//
// Protocol: newline-delimited JSON on stdin/stdout.
//   Commands (stdin):  {"type":"start_run","character":"ironclad","seed":0}
//   Events   (stdout): {"type":"game_state","phase":"main_menu"}
public static class HeadlessController
{
    public static bool IsActive { get; private set; }
    public static bool ExitWhenDone { get; private set; }

    private static readonly ConcurrentQueue<JsonObject> CommandQueue = new();
    private static readonly ConcurrentQueue<Action>     DeferredActions = new();
    private static Thread? _stdinThread;
    private static bool _sceneTreeHooked;
    private static volatile bool _shouldQuit;

    public static bool ShouldActivate()
    {
        // Godot consumes --headless itself and does NOT expose it via GetCmdlineArgs().
        // The correct way to detect headless mode is to ask the display server directly.
        if (DisplayServer.GetName() == "headless")
            return true;
        // Fallback: explicit opt-in without --headless (game will have a window)
        var env = System.Environment.GetEnvironmentVariable("STS2_HEADLESS");
        return env is "1" or "true";
    }

    public static void Initialize()
    {
        if (IsActive) return;
        IsActive = true;

        var exitEnv = System.Environment.GetEnvironmentVariable("STS2_EXIT_WHEN_DONE");
        ExitWhenDone = exitEnv is "1" or "true";

        // Uncap frame rate — each dispatched command waits at least one engine tick,
        // so higher fps = lower inter-command idle time.
        Engine.MaxFps = 10000;

        // Use the direct await path in ActionExecutor instead of the per-frame
        // polling loop — eliminates ~N frames of overhead per game action.
        NonInteractiveMode.AutoSlayerCheck = () => true;

        _stdinThread = new Thread(StdinLoop) { IsBackground = true, Name = "ls-stdin" };
        _stdinThread.Start();

        Emit(new { type = "ready", version = "v0.1.0" });
    }

    // Called from the LoadMainMenu patch — at that point the SceneTree is alive.
    public static void HookSceneTree()
    {
        if (_sceneTreeHooked) return;
        if (Engine.GetMainLoop() is not SceneTree tree) return;
        tree.ProcessFrame += ProcessCommands;
        _sceneTreeHooked = true;
    }

    private static void StdinLoop()
    {
        while (true)
        {
            string? line;
            try { line = Console.ReadLine(); }
            catch { break; }

            if (line == null)
            {
                _shouldQuit = true;
                break;
            }
            line = line.Trim();
            if (line.Length == 0) continue;

            try
            {
                if (JsonNode.Parse(line)?.AsObject() is { } obj)
                    CommandQueue.Enqueue(obj);
                else
                    Emit(new { type = "error", message = "Command must be a JSON object", raw = line });
            }
            catch (JsonException)
            {
                Emit(new { type = "error", message = "Failed to parse JSON", raw = line });
            }
        }
    }

    // Schedule an action to run on the next SceneTree.ProcessFrame (main Godot thread).
    public static void EnqueueAction(Action action) => DeferredActions.Enqueue(action);

    // Runs on the main Godot thread each frame via SceneTree.ProcessFrame.
    public static void ProcessCommands()
    {
        if (_shouldQuit && CommandQueue.IsEmpty && DeferredActions.IsEmpty)
        {
            if (Engine.GetMainLoop() is SceneTree tree) tree.Quit();
            return;
        }

        while (DeferredActions.TryDequeue(out var action))
        {
            try { action(); }
            catch (Exception ex) { Emit(new { type = "error", message = ex.Message }); }
        }

        while (CommandQueue.TryDequeue(out var cmd))
        {
            var cmdType = cmd["type"]?.GetValue<string>() ?? "";
            try { Dispatch(cmdType, cmd); }
            catch (Exception ex)
            {
                Emit(new { type = "error", message = ex.Message, command = cmdType });
            }
        }
    }

    private static void Dispatch(string cmdType, JsonObject cmd)
    {
        switch (cmdType)
        {
            case "get_state":
                Emit(new { type = "state", message = "not yet implemented" });
                break;
            case "start_run":
            case "play_card":
            case "end_turn":
            case "map_select":
            case "event_choice":
            case "rest_site":
            case "shop_buy":
            case "reward_select":
            case "replay_run":
            case "discover":
                Emit(new { type = "error", message = "not yet implemented", command = cmdType });
                break;
            default:
                Emit(new { type = "error", message = $"Unknown command type: {cmdType}", command = cmdType });
                break;
        }
    }

    public static void Emit(object data)
    {
        Console.WriteLine(JsonSerializer.Serialize(data));
        Console.Out.Flush();
    }
}
