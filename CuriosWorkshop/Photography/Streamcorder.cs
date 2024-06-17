using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.Technology, RogueCategories.Stealth, RogueCategories.Weird)]
    public class Streamcorder : BasicCamera
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Streamcorder>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Streamcorder",
                         Russian = @"Стрим-камера",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "Works similar to a Disposable Camera, except that it creates Live Photos instead of regular ones",
                     })
                     .WithSprite(Properties.Resources.Streamcorder)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 10,
                         CharacterCreationCost = 3,
                         LoadoutCost = 3,
                         Prerequisites = { nameof(PhotoCamera) },
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.initCount = 2;
            Item.rewardCount = 3;
            Item.itemValue = 80;
            Item.stackable = true;
            Item.hasCharges = true;
            Item.goesInToolbar = true;
        }

        public override CameraOverlayType Type => CameraOverlayType.Streamcorder;

        public override bool OnOverlay(Rect area, Vector2Int size) => true;
        public override bool TakePhoto(Rect area, Vector2Int size) => true;

    }
}
