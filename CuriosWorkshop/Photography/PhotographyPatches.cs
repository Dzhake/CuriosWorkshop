using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
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
            Patcher.Postfix(typeof(MainGUI), nameof(MainGUI.HideBigImage));

            Patcher.Transpiler(typeof(MainGUI), nameof(MainGUI.MainGUIUpdate), nameof(PatchMenuGUIOpenedInventory));
            Patcher.Transpiler(typeof(CameraScript), "Update", nameof(PatchMenuGUIOpenedInventory));
        }

        public static bool preventQuestMarkerDestruction;
        public static bool QuestMarker_OnDisable() => !preventQuestMarkerDestruction;

        public static void MainGUI_HideEverything(MainGUI __instance)
            => PhotoUI.Get(__instance).Hide();
        public static void MainGUI_HideBigImage(MainGUI __instance)
            => PhotoUI.Get(__instance).Hide();

        public static IEnumerable<CodeInstruction> PatchMenuGUIOpenedInventory(IEnumerable<CodeInstruction> code)
            => code.ReplaceRegion(new Func<CodeInstruction, bool>[]
            {
                static i => i.LoadsField(openedInventoryField),
            }, new Func<CodeInstruction[], CodeInstruction>[]
            {
                static m => new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PhotographyPatches), nameof(GetIsInterfaceOpen))),
            });
        private static readonly FieldInfo openedInventoryField = AccessTools.Field(typeof(MainGUI), nameof(MainGUI.openedInventory));

        private static bool GetIsInterfaceOpen(MainGUI mainGUI)
            => mainGUI.openedInventory || PhotoUI.Get(mainGUI).IsOpened;

    }
}
