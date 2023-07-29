using System;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    public static class PhotographyPatches
    {
        private static RoguePatcher Patcher => CuriosPlugin.Patcher;

        public static void Apply()
        {
            Patcher.TypeWithPatches = typeof(PhotographyPatches);

            Patcher.Prefix(typeof(QuestMarker), "OnDisable");

            Type[] gunShootPars = { typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(string) };
            Patcher.Prefix(typeof(Gun), nameof(Gun.Shoot), nameof(Gun_Shoot_Prefix), gunShootPars);
            Patcher.Postfix(typeof(Gun), nameof(Gun.Shoot), nameof(Gun_Shoot_Postfix), gunShootPars);
            Patcher.Finalizer(typeof(Gun), nameof(Gun.Shoot), nameof(Gun_Shoot_Finalizer), gunShootPars);

            Patcher.Postfix(typeof(Gun), nameof(Gun.ShowGun));
            Patcher.Prefix(typeof(Gun), nameof(Gun.HideGun));

            Patcher.Postfix(typeof(Gun), nameof(Gun.GunUpdate));

            Patcher.Postfix(typeof(MainGUI), nameof(MainGUI.HideEverything));
        }

        public static bool preventQuestMarkerDestruction;
        public static bool QuestMarker_OnDisable() => !preventQuestMarkerDestruction;

        private static bool gunShooting;
        private static InvItem? gunLastItem;
        public static void Gun_Shoot_Prefix() => gunShooting = true;
        public static void Gun_Shoot_Postfix(Gun __instance)
        {
            Flashlight? flashlight = gunLastItem?.GetHook<Flashlight>();
            if (flashlight is not null)
            {
                __instance.gunContainerAnim.Play("", -1, 0f);
            }

        }
        public static void Gun_Shoot_Finalizer() => gunShooting = false;

        public static void Gun_ShowGun(Gun __instance, InvItem myGun)
        {
            gunLastItem = myGun;
            if (!gunShooting) return;
            myGun.GetHook<Flashlight>()?.TurnOn(__instance);
        }
        public static void Gun_AimGun(Gun __instance)
        {
            __instance.visibleGun?.GetHook<Flashlight>()?.AimLight(__instance);
        }
        public static void Gun_HideGun(Gun __instance)
        {
            __instance.visibleGun?.GetHook<Flashlight>()?.TurnOff(__instance);
        }
        public static void Gun_GunUpdate(Gun __instance)
        {
            if (!__instance.holdingAttack)
            {
                Flashlight? flashlight = __instance.visibleGun?.GetHook<Flashlight>();
                if (flashlight is not null) __instance.HideGun();
            }
        }

        public static PhotoUI GetPhotoUI(MainGUI gui)
        {
            Transform? tr = gui.transform.Find(nameof(PhotoUI));
            if (!tr)
            {
                tr = new GameObject(nameof(PhotoUI), typeof(RectTransform)).transform;
                tr.SetParent(gui.transform, true);
                tr.localPosition = Vector3.zero;
                tr.localScale = Vector3.one;
                tr.gameObject.AddComponent<PhotoUI>();
            }
            return tr.GetComponent<PhotoUI>();
        }

        public static void MainGUI_HideEverything(MainGUI __instance)
            => GetPhotoUI(__instance).Hide();

    }
}
