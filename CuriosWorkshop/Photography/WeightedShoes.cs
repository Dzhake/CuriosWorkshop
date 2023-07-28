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
                         English = "Weighted Shoes",
                         Russian = @"Утяжелённые ботинки",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithUnlock(new TraitUnlock
                     {
                         IsAvailableInCC = false,
                         LeadingItems = { nameof(PhotoCamera) },
                         Recommendations = { nameof(PhotoCamera), nameof(DisposableCamera) },
                     });
        }

        public override void OnAdded() { }
        public override void OnRemoved() { }

    }
}
