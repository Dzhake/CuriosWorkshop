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
                         English = "[CW] Photo Camera",
                         Russian = @"[CW] Фотоаппарат",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "A reusable camera. Takes a screenshot of a small area of the screen, and gives you a unique Photo item (non-stackable). When you take a photo, all nearby Cops get angry (referencing the cut 7th Law of the Land). The film can be refilled at any Loadout-O-Matic.",
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
