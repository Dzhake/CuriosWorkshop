using RogueLibsCore;

namespace CuriosWorkshop
{
    public class SteadyHand : CustomTrait
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomTrait<SteadyHand>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Steady Hand",
                         Russian = @"Твёрдая рука",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "Photos never turn out blurry.",
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
