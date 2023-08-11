using System.Collections;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Technology, RogueCategories.NonViolent, RogueCategories.NonStandardWeapons2, RogueCategories.NotRealWeapons)]
    public class Flashlight : CustomItem, IFlashlight
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Flashlight>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Flashlight",
                         Russian = @"Фонарик",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithSprite(Properties.Resources.Flashlight)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 0,
                         CharacterCreationCost = 0,
                         LoadoutCost = 0,
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponProjectile;
            Item.weaponCode = weaponType.WeaponProjectile;
            Item.initCount = 180 * 100;
            Item.initCountAI = 30 * 100;
            Item.rewardCount = 180 * 100;
            Item.itemValue = 10;
            Item.stackable = true;
            Item.hasCharges = true;

            Item.isWeapon = true;
            Item.rapidFire = true;
            Item.thiefCantSteal = true;
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
            if (Owner!.isPlayer > 0) gun.SubtractBullets(1, Item);
            AimLight(gun);
        }
        public void AimLight(Gun gun)
        {
            MoveableLightSource source = MoveableLightSource.Get(gun);
            source.gameObject.layer = LightingPatches.LightSourceLayer;
            source.TurnOn(() => gc.audioHandler.Play(Owner!, "FlashlightOn"));
            source.UpdateLight(new Color32(199, 174, 120, 255), Owner!.isPlayer > 0 ? 8f : 4f);
        }
        public void TurnOff(Gun gun)
        {
            MoveableLightSource source = MoveableLightSource.Get(gun);
            source.gameObject.layer = LightingPatches.LightSourceLayer;
            source.TurnOff(() => gc.audioHandler.Play(Owner!, "FlashlightOff"));
        }

    }
}
