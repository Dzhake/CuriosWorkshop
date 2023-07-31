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
        }

        public static bool preventQuestMarkerDestruction;
        public static bool QuestMarker_OnDisable() => !preventQuestMarkerDestruction;

    }
}
