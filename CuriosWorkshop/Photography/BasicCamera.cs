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

        public abstract bool OnOverlay(Rect rect, Vector2Int size);
        public abstract bool TakePhoto(Rect rect, Vector2Int size);

        private Rect GetRectSize(Vector2 center, out Vector2Int size)
        {
            size = PhotoSize;
            tk2dCamera tk2dCamera = GameController.gameController.cameraScript.actualCamera;
            Vector2 worldSize = (Vector2)size / (100f * tk2dCamera.ZoomFactor);
            if (Owner!.HasTrait<WeightedShoes>())
            {
                size += size / 4;
                worldSize += worldSize / 4f;
            }
            return new Rect(center - 0.5f * worldSize, worldSize);
        }

        public bool TargetFilter(Vector2 position)
        {
            Rect rect = GetRectSize(position, out Vector2Int size);
            return OnOverlay(rect, size) && Owner!.movement.HasLOSPosition(position, "360");
        }
        public bool TargetPosition(Vector2 position)
        {
            if (!Owner!.movement.HasLOSPosition(position, "360"))
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
            return TakePhoto(rect, size);
        }
        public CustomTooltip TargetCursorText(Vector2 position) => default;

    }
}
