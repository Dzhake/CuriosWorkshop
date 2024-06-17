using RogueLibsCore;

namespace CuriosWorkshop
{
    public class WeightedShoes : CustomTrait
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomTrait<WeightedShoes>()
                     .WithName(new CustomNameInfo
                     {
                         English = "[CW] Weighted Shoes",
                         Russian = @"[CW] Утяжелённые ботинки",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "You can take wider shots (+25% to both width and height).",
                     })
                     .WithUnlock(new TraitUnlock
                     {
                         IsAvailableInCC = false,
                         LeadingItems = { nameof(PhotoCamera) },
                         Recommendations = { nameof(PhotoCamera), nameof(DisposableCamera), nameof(Streamcorder), nameof(SoulStealer) },
                     });
        }

        public override void OnAdded() { }
        public override void OnRemoved() { }

    }
}
