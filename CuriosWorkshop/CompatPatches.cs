using BepInEx.Logging;
using HarmonyLib;
using RogueLibsCore;

namespace CuriosWorkshop
{
    public static class CompatPatches
    {
        public static RoguePatcher Patcher => CuriosPlugin.Patcher;
        public static ManualLogSource Logger => CuriosPlugin.Logger;

        public static void Apply()
        {
            NeutralizePlugin(@"abbysssal.streetsofrogue.chaosathomebase", "Chaos at Home Base");
        }

        private static void NeutralizePlugin(string guid, string name)
        {
            if (Harmony.HasAnyPatches(guid))
            {
                Logger.LogWarning($"""

                    ||=================== Outdated Mod ===================||
                    || One of the mods you have installed is outdated!    ||
                    ||                                                    ||
                    || Mod GUID: {guid,-40} ||
                    || Mod Name: {name,-40} ||
                    ||                                                    ||
                    || please disable it.                                 ||
                    ||====================================================||
                    """);
                //Harmony.UnpatchID(guid);
            }
        }

    }
}
