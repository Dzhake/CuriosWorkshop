using System;
using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using RogueLibsCore;

namespace CuriosWorkshop
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RogueLibs.GUID, RogueLibs.CompiledVersion)]
    public class CuriosPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "abbysssal.streetsofrogue.curiosworkshop";
        public const string PluginName = "Curio's Workshop";
        public const string PluginVersion = "0.1.0";

        public new static ManualLogSource Logger = null!;
        public static RoguePatcher Patcher = null!;

        public void Awake()
        {
            Logger = base.Logger;
            Patcher = new RoguePatcher(this);
            RogueLibs.LoadFromAssembly();

            Patcher.Prefix(typeof(QuestMarker), "OnDisable");

        }

        private static bool preventQuestMarkerDestruction;
        public static bool QuestMarker_OnDisable() => !preventQuestMarkerDestruction;

        public static void WithHiddenInterface([InstantHandle] Action action)
        {
            static void SetInterface(bool value)
            {
                GameController gc = GameController.gameController;
                gc.nonClickableGUI.go.SetActive(value);
                gc.mainGUI.gameObject.SetActive(value);
                preventQuestMarkerDestruction = true;
                gc.questMarkerList.ForEach(q => q.go.SetActive(value));
                preventQuestMarkerDestruction = false;
                gc.questMarkerSmallList.ForEach(q => q.gameObject.SetActive(value));
            }
            try
            {
                preventQuestMarkerDestruction = true;
                SetInterface(false);
                action();
            }
            finally
            {
                SetInterface(true);
                preventQuestMarkerDestruction = false;
            }
        }

    }
}
