using RogueLibsCore;

namespace CuriosWorkshop
{
    public static class PhotographyPatches
    {
        private static RoguePatcher Patcher => CuriosPlugin.Patcher;
        public static GameController gc => GameController.gameController;

        public static void Apply()
        {
            Patcher.TypeWithPatches = typeof(PhotographyPatches);

            Patcher.Prefix(typeof(QuestMarker), "OnDisable");

            Patcher.Postfix(typeof(MainGUI), nameof(MainGUI.HideEverything));
        }

        public static bool preventQuestMarkerDestruction;
        public static bool QuestMarker_OnDisable() => !preventQuestMarkerDestruction;

        public static void MainGUI_HideEverything(MainGUI __instance)
            => PhotoUI.Get(__instance).Hide();

    }
}
