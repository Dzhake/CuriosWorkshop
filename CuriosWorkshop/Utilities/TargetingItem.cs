#if DEBUG
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.NPCsCantPickUp)]
    public class TargetingItem : CustomItem, IItemTargetableAnywhere
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<TargetingItem>()
                     .WithName(new CustomNameInfo("@ Targeting Item"))
                     .WithDescription(new CustomNameInfo("Displays tile coordinates and lighting level."))
                     .WithSprite(Properties.Resources.TargetingItem)
                     .WithUnlock(new ItemUnlock());
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.initCount = 1;
            Item.stackable = true;
            Item.goesInToolbar = true;
            Item.noCountText = true;
            Item.itemValue = 1_000_000;
        }

        public bool TargetFilter(Vector2 position) => true;
        public bool TargetPosition(Vector2 position) => true;

        public CustomTooltip TargetCursorText(Vector2 position)
        {
            string coordinates = $"{position / new Vector2(0.64f, 0.64f)}";
            Color color = LightingUtilities.GetLighting(position, Owner!.agentCamera);
            //float brightness = LightingUtilities.GetLightingLevel(position, Owner.agentCamera);
            float brightness = ((Vector3)(Vector4)color).magnitude / Mathf.Sqrt(3f);
            return new CustomTooltip(coordinates + $" {Mathf.Round(brightness * 100) / 100f}", color);
        }

    }
}
#endif
