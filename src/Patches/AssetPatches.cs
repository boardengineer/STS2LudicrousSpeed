using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace STS2LudicrousSpeed.Patches;

[HarmonyPatch(typeof(PreloadManager), nameof(PreloadManager.Enabled), MethodType.Getter)]
public static class PreloadEnabledPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref bool __result)
    {
        if (HeadlessController.IsActive)
            __result = false;
    }
}

[HarmonyPatch(typeof(AtlasManager), "LoadAtlas")]
public static class AtlasLoadPatch
{
    [HarmonyPrefix]
    public static bool Prefix() => !HeadlessController.IsActive;
}
