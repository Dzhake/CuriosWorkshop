using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    public abstract class BasicCamera : CustomItem, IItemTargetableAnywhere
    {
        [RLSetup]
        private static void Setup()
        {
            RogueLibs.CreateCustomAudio("TakePhoto", Properties.Resources.TakePhoto);

            RogueLibs.CreateCustomName("TakePhoto", NameTypes.Interface, new CustomNameInfo
            {
                English = "Take a photo",
                Russian = @"Снять фото",
            });
        }

        public abstract CameraOverlayType Type { get; }

        private Rect GetRectSize(Vector2 center, out Vector2Int size)
        {
            size = Type.Size;
            tk2dCamera tk2dCamera = GameController.gameController.cameraScript.actualCamera;
            Vector2 worldSize = (Vector2)size / (100f * tk2dCamera.ZoomFactor);
            if (Owner!.HasTrait<WeightedShoes>())
            {
                size += size / 4;
                worldSize += worldSize / 4f;
            }
            return new Rect(center - 0.5f * worldSize, worldSize);
        }

        public abstract bool OnOverlay(Rect area, Vector2Int size);
        public abstract bool TakePhoto(Rect area, Vector2Int size);

        public bool TargetFilter(Vector2 position)
        {
            Rect rect = GetRectSize(position, out Vector2Int size);
            bool canCapture = OnOverlay(rect, size);
            bool canSee = Owner!.HasTrait<XRayLens>() || Owner!.movement.HasLOSPosition(position, "360");
            return  canCapture && canSee;
        }
        public bool TargetPosition(Vector2 position)
        {
            if (!Owner!.movement.HasLOSPosition(position, "360") && !Owner.HasTrait<XRayLens>())
            {
                gc.audioHandler.Play(Owner, "CantDo");
                return false;
            }
            if (!Inventory!.hasEmptySlot())
            {
                gc.audioHandler.Play(Owner, "CantDo");
                Owner!.SayDialogue("InventoryFull");
                return false;
            }
            Rect rect = GetRectSize(position, out Vector2Int size);
            bool res = TakePhoto(rect, size);
            if (res)
            {
                Item.invInterface.HideTarget();
                Inventory.invInterface.UpdateInvInterface();
                Count--;
            }
            return res;
        }
        public CustomTooltip TargetCursorText(Vector2 position) => default;

    }
}
