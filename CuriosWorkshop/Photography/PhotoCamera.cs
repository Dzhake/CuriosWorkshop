using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.Technology)]
    public class PhotoCamera : BasicCamera
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<PhotoCamera>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Photo Camera",
                         Russian = @"Фотоаппарат",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithSprite(Properties.Resources.PhotoCamera)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 0,
                         // TODO: Prerequisites = { nameof(Photographer) },
                         CharacterCreationCost = 5,
                         LoadoutCost = 5,
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.initCount = 10;
            Item.rewardCount = 10;
            Item.itemValue = 40;
            Item.stackable = true;
            Item.hasCharges = true;
            Item.goesInToolbar = true;
        }

        public override Vector2Int PhotoSize => new Vector2Int(400, 300);

        public override bool OnOverlay(Rect rect, Vector2Int size)
        {
            PhotoUtils.SetCameraOverlay(Owner!.mainGUI, rect.center, size, CameraOverlayType.Normal);
            return true;
        }
        public override bool TakePhoto(Rect rect, Vector2Int size)
        {
            gc.audioHandler.Play(Owner, "TakePhoto");
            Texture2D screenshot = PhotoUtils.TakeScreenshot(rect.center, size);

            Photo photo = Inventory!.AddItem<Photo>(1)!;
            photo.genTexture = screenshot;
            photo.capturedFeatures = PhotoFeature.Create(rect);

            Item.invInterface.HideTarget();
            Count--;
            return true;
        }

    }
}
