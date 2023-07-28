using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    public abstract class BasicCamera : CustomItem, IItemTargetableAnywhere
    {
        [RLSetup]
        private static void Setup()
        {
            RogueLibs.CreateCustomAudio("TakePhoto", Properties.Resources.TakePhoto, AudioType.MPEG);

            RogueLibs.CreateCustomName("TakePhoto", NameTypes.Interface, new CustomNameInfo
            {
                English = "Take a photo",
                Russian = @"Снять фото",
            });
        }

        public abstract Vector2Int PhotoSize { get; }
        public virtual string SoundEffectName => "TakePhoto";

        public virtual void AugmentPhoto(Photo photo, Rect rect) { }

        public bool TargetFilter(Vector2 position)
        {
            PhotoUtils.SetCameraOverlay(Owner!.mainGUI, position, Properties.Resources.PhotoFrame);
            // TODO: move the photo frame to the cursor
            return true;
        }
        public bool TargetPosition(Vector2 position)
        {
            if (!Inventory!.hasEmptySlot())
            {
                gc.audioHandler.Play(Owner, "CantDo");
                Owner!.SayDialogue("InventoryFull");
                return false;
            }

            Vector2Int size = PhotoSize;
            if (Owner!.HasTrait<WeightedShoes>()) size += size / 4;
            int detail = Owner!.HasTrait<SteadyHand>() ? 2 : 1;

            gc.audioHandler.Play(Owner, SoundEffectName);
            Texture2D screenshot = PhotoUtils.TakeScreenshot(size, detail, position);

            Photo photo = Inventory.AddItem<Photo>(1)!;
            photo.genTexture = screenshot;
            AugmentPhoto(photo,);

            Count--;
            Item.invInterface.HideTarget();
            return true;
        }
        public CustomTooltip TargetCursorText(Vector2 position) => default;

    }
}
