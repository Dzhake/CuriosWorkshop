using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Technology, RogueCategories.NonViolent, RogueCategories.NonStandardWeapons2, RogueCategories.NotRealWeapons, RogueCategories.Weird)]
    public class Shadowlight : CustomItem, IFlashlight
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Shadowlight>()
                     .WithName(new CustomNameInfo
                     {
                         English = "[CW] Shadowlight",
                         Russian = @"[CW] Темнарик",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "Works like a Flashlight, except it emits darkness instead of light.",
                     })
                     .WithSprite(Properties.Resources.Shadowlight)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 3,
                         CharacterCreationCost = 1,
                         LoadoutCost = 1,
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponProjectile;
            Item.weaponCode = weaponType.WeaponProjectile;
            Item.initCount = 120 * 100;
            Item.rewardCount = 120 * 100;
            Item.itemValue = 30;
            Item.stackable = true;
            Item.hasCharges = true;

            Item.isWeapon = true;
            Item.rapidFire = true;
            Item.dontSelectNPC = true;
            Item.dontAutomaticallySelect = true;
            Item.doesNoDamage = true;
            Item.gunKnockback = 0;
        }
        public override CustomTooltip GetCountString()
            => $"{Mathf.CeilToInt(100f * Count / Item.initCount)}%";

        public void TurnOn(Gun gun)
        {
            Owner!.weaponCooldown = 0.01f;
            gun.SubtractBullets(1, Item);
            AimLight(gun);
        }
        public void AimLight(Gun gun)
        {
            MoveableLightSource lightSource = MoveableLightSource.Get(gun);
            lightSource.gameObject.layer = LightingPatches.DarkSourceLayer;
            lightSource.TurnOn(() => gc.audioHandler.Play(Owner!, "FlashlightOn"));
            lightSource.UpdateLight(new Color32(160, 160, 120, 255), 8f);
        }
        public void TurnOff(Gun gun)
        {
            MoveableLightSource lightSource = MoveableLightSource.Get(gun);
            lightSource.gameObject.layer = LightingPatches.DarkSourceLayer;
            lightSource.TurnOff(() => gc.audioHandler.Play(Owner!, "FlashlightOff"));
        }

    }
}
