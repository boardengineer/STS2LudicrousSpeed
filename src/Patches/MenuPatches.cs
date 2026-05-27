using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Debug;

namespace STS2LudicrousSpeed.Patches;

[HarmonyPatch(typeof(NGame), "LoadMainMenu")]
public static class MainMenuPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (HeadlessController.IsActive)
        {
            // First time: wire the per-frame command pump into the SceneTree.
            HeadlessController.HookSceneTree();

            HeadlessController.Emit(new
            {
                type    = "game_state",
                phase   = "main_menu",
                message = "Send {\"type\":\"start_run\",\"character\":\"ironclad\",\"seed\":0} to begin"
            });
            return;
        }

        // Non-headless: debug console message.
        var console = NDevConsole.Instance;
        var outputBuffer = console.GetNode<RichTextLabel>("OutputContainer/OutputBuffer");
        outputBuffer.Text += "[STS2LudicrousSpeed] Hello World!\n";
    }
}
