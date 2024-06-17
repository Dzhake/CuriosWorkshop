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
                         English = "[CW] X-Ray Lens",
                         Russian = @"[CW] Рентгеновская линза",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "You can take shots through walls.",
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
