using RogueLibsCore;

namespace CuriosWorkshop
{
    public class XRayLens : CustomTrait
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomTrait<XRayLens>()
                     .WithName(new CustomNameInfo
                     {
                         English = "X-Ray Lens",
                         Russian = @"Рентгеновская линза",
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
                         Recommendations = { nameof(PhotoCamera), nameof(DisposableCamera), nameof(Streamcorder), nameof(SoulStealer) },
                     });
        }

        public override void OnAdded() { }
        public override void OnRemoved() { }

    }
}
