using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Debug;

namespace STS2LudicrousSpeed;

[ModInitializer("Initialize")]
public class Plugin
{
    public static void Initialize()
    {
        var harmony = new Harmony("com.author.sts2ludicrousspeed");
        harmony.PatchAll(typeof(Plugin).Assembly);
    }
}

[HarmonyPatch(typeof(NGame), "LoadMainMenu")]
public class MainMenuPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
    }
}
