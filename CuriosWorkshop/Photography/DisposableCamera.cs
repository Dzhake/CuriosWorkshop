using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.Technology)]
    public class DisposableCamera : BasicCamera
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<DisposableCamera>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Disposable Camera",
                         Russian = @"Одноразовая камера",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "Works the same way as a Photo Camera, but disappears on use.",
                     })
                     .WithSprite(Properties.Resources.DisposableCamera)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 3,
                         CharacterCreationCost = 2,
                         LoadoutCost = 2,
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.initCount = 2;
            Item.rewardCount = 3;
            Item.itemValue = 40;
            Item.stackable = true;
            Item.goesInToolbar = true;
        }

        public override CameraOverlayType Type => CameraOverlayType.Disposable;

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
