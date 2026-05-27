using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace STS2LudicrousSpeed.Patches;

// Force FastModeType.Instant in headless mode so all combat animations
// (card plays, EndTurn enemy sequences, draw animations) complete without
// waiting. The FastMode system is STS2's own built-in skip-animation path —
// using it is safer than patching individual animation nodes.
[HarmonyPatch(typeof(PrefsSave), nameof(PrefsSave.FastMode), MethodType.Getter)]
public static class FastModePatch
{
    [HarmonyPostfix]
    public static void Postfix(ref FastModeType __result)
    {
        if (HeadlessController.IsActive)
            __result = FastModeType.Instant;
    }
}
