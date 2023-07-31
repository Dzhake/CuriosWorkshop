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

        public override CameraOverlayType Type => CameraOverlayType.PhotoCamera;

        public override bool OnOverlay(Rect area, Vector2Int size)
        {
            CameraOverlay overlay = Owner!.mainGUI.Get<CameraOverlay>();
            overlay.Set(Type, area, size);
            return true;
        }
        public override bool TakePhoto(Rect area, Vector2Int size)
        {
            gc.audioHandler.Play(Owner, "TakePhoto");
            CameraOverlay overlay = Owner!.mainGUI.Get<CameraOverlay>();
            Texture2D screenshot = overlay.Capture();

            Photo photo = Inventory!.AddItem<Photo>(1)!;
            photo.genTexture = screenshot;
            photo.capturedFeatures = PhotoUtils.GetFeatures(area);

            return true;
        }

    }
}
