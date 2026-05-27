using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2LudicrousSpeed;

[ModInitializer("Initialize")]
public class Plugin
{
    public static void Initialize()
    {
        var harmony = new Harmony("com.author.sts2ludicrousspeed");
        harmony.PatchAll(typeof(Plugin).Assembly);

        if (HeadlessController.ShouldActivate())
            HeadlessController.Initialize();
    }
}
