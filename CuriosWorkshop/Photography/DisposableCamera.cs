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
                         English = "",
                         Russian = @"",
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

        public override Vector2Int PhotoSize => new Vector2Int(400, 300);

        public override bool OnOverlay(Rect rect, Vector2Int size)
        {
            PhotoUtils.SetCameraOverlay(Owner!.mainGUI, rect.center, size, CameraOverlayType.Disposable);
            return true;
        }
        public override bool TakePhoto(Rect rect, Vector2Int size)
        {
            gc.audioHandler.Play(Owner, "TakePhoto");
            Texture2D screenshot = PhotoUtils.TakeScreenshot(rect.center, size);

            Photo photo = Inventory!.AddItem<Photo>(1)!;
            photo.genTexture = screenshot;

            Item.invInterface.HideTarget();
            Count--;
            return true;
        }

    }
}
