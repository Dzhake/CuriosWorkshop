using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.Technology, RogueCategories.Weird, RogueCategories.Weapons, RogueCategories.NonStandardWeapons2)]
    public class SoulStealer : BasicCamera
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<SoulStealer>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Soul Stealer",
                         Russian = @"Похититель душ",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithSprite(Properties.Resources.SoulStealer)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 10,
                         CharacterCreationCost = 3,
                         LoadoutCost = 3,
                         Prerequisites = { nameof(PhotoCamera), VanillaItems.BooUrn },
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.initCount = 1;
            Item.rewardCount = 2;
            Item.itemValue = 80;
            Item.stackable = true;
            Item.hasCharges = true;
            Item.goesInToolbar = true;
        }

        public override CameraOverlayType Type => CameraOverlayType.SoulStealer;

        public override bool OnOverlay(Rect area, Vector2Int size) => true;
        public override bool TakePhoto(Rect area, Vector2Int size) => true;

    }
}
