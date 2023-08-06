using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Technology, RogueCategories.NonViolent, RogueCategories.NonStandardWeapons2, RogueCategories.NotRealWeapons, RogueCategories.Weird)]
    public class Blacklight : CustomItem, IFlashlight
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Blacklight>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Blacklight",
                         Russian = @"Ультрафиолетовый фонарик",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithSprite(Properties.Resources.Blacklight)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 5,
                         CharacterCreationCost = 2,
                         LoadoutCost = 2,
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponProjectile;
            Item.weaponCode = weaponType.WeaponProjectile;
            Item.initCount = 120 * 100;
            Item.rewardCount = 120 * 100;
            Item.itemValue = 50;
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
            lightSource.gameObject.layer = LightingPatches.LightSourceLayer;
            lightSource.TurnOn(() => gc.audioHandler.Play(Owner!, "FlashlightOn"));
            lightSource.UpdateLight(new Color32(130, 0, 255, 255), 4f);
        }
        public void TurnOff(Gun gun)
        {
            MoveableLightSource lightSource = MoveableLightSource.Get(gun);
            lightSource.gameObject.layer = LightingPatches.LightSourceLayer;
            lightSource.TurnOff(() => gc.audioHandler.Play(Owner!, "FlashlightOff"));
        }

    }
}
