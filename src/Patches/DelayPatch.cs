using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;

namespace STS2LudicrousSpeed.Patches;

[HarmonyPatch(typeof(Task), nameof(Task.Delay), new[] { typeof(int) })]
public static class TaskDelayNoCTPatch
{
    public static void Prefix(ref int millisecondsDelay)
    {
        if (HeadlessController.IsActive && millisecondsDelay > 0)
            millisecondsDelay = 0;
    }
}

[HarmonyPatch(typeof(Task), nameof(Task.Delay), new[] { typeof(int), typeof(CancellationToken) })]
public static class TaskDelayWithCTPatch
{
    public static void Prefix(ref int millisecondsDelay)
    {
        if (HeadlessController.IsActive && millisecondsDelay > 0)
            millisecondsDelay = 0;
    }
}
