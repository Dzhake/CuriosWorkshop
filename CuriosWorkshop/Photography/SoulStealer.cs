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

        public override Vector2Int PhotoSize => new Vector2Int(400, 300);

        public override bool OnOverlay(Rect rect, Vector2Int size)
        {
            PhotoUtils.SetCameraOverlay(Owner!.mainGUI, rect.center, size, CameraOverlayType.SoulStealer);
            return true; // TODO: check for presence of any souls
        }
        public override bool TakePhoto(Rect rect, Vector2Int size)
        {
            gc.audioHandler.Play(Owner, "TakePhoto");
            // TODO: make a Haunted Photo if souls were stolen; otherwise, a regular Photo
            //Texture2D screenshot = PhotoUtils.TakeScreenshot(rect);

            //Photo photo = Inventory!.AddItem<Photo>(1)!;
            //photo.genTexture = screenshot;
            //photo.capturedFeatures = PhotoFeature.Create(rect);

            Item.invInterface.HideTarget();
            Count--;
            return true;
        }

    }
}
