using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2LudicrousSpeed.Patches;

// Patches that suppress display/rendering operations which fail or are
// meaningless in Godot headless mode (--headless / dummy display server).

// NTransition.FadeIn is async (returns Task). In headless mode it tries to set
// a shader parameter with a null texture (dummy renderer has no textures).
// Returning false from a Harmony prefix on an async method leaves the Task null,
// so we must set __result = Task.CompletedTask before returning false.
[HarmonyPatch(typeof(NTransition), "FadeIn")]
public static class TransitionFadeInPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref Task __result)
    {
        if (!HeadlessController.IsActive) return true;
        __result = Task.CompletedTask;
        return false;
    }
}

[HarmonyPatch(typeof(NTransition), "FadeOut")]
public static class TransitionFadeOutPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref Task __result)
    {
        if (!HeadlessController.IsActive) return true;
        __result = Task.CompletedTask;
        return false;
    }
}
