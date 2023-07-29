using BepInEx;
using BepInEx.Logging;
using RogueLibsCore;

namespace CuriosWorkshop
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RogueLibs.GUID, RogueLibs.CompiledVersion)]
    public class CuriosPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "abbysssal.streetsofrogue.curiosworkshop";
        public const string PluginName = "Curio's Workshop";
        public const string PluginVersion = "0.1.0";

        public new static ManualLogSource Logger = null!;
        public static RoguePatcher Patcher = null!;

        public void Awake()
        {
            Logger = base.Logger;
            Patcher = new RoguePatcher(this);
            RogueLibs.LoadFromAssembly();

            PhotographyPatches.Apply();
        }

    }
}
