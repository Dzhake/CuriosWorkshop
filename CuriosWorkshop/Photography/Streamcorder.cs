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
                         English = "",
                         Russian = @"",
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

        public override Vector2Int PhotoSize => new Vector2Int(400, 300);

        public override bool OnOverlay(Rect rect, Vector2Int size)
        {
            PhotoUtils.SetCameraOverlay(Owner!.mainGUI, rect.center, size, CameraOverlayType.Streamcorder);
            return true;
        }
        public override bool TakePhoto(Rect rect, Vector2Int size)
        {
            gc.audioHandler.Play(Owner, "TakePhoto");
            // TODO: make a Live Photo instead
            //Texture2D screenshot = PhotoUtils.TakeScreenshot(rect);

            //Photo photo = Inventory!.AddItem<Photo>(1)!;
            //photo.genTexture = screenshot;

            Item.invInterface.HideTarget();
            Count--;
            return true;
        }

    }
}
