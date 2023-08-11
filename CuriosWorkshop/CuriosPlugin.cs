using BepInEx;
using BepInEx.Logging;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RogueLibs.GUID, RogueLibs.CompiledVersion)]
    public class CuriosPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "abbysssal.streetsofrogue.curiosworkshop";
        public const string PluginName = "Curio's Workshop";
        public const string PluginVersion = "0.1.0";

        public static CuriosPlugin Instance = null!;
        public new static ManualLogSource Logger = null!;
        public static RoguePatcher Patcher = null!;

        public static bool IsDebug
#if DEBUG
            => true;
#else
            => false;
#endif

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            Patcher = new RoguePatcher(this);
            RogueLibs.LoadFromAssembly();

            CompatPatches.Apply();
            PhotographyPatches.Apply();
            LightingPatches.Apply();
            HomeBasePatches.Apply();
        }

        public static RogueSprite[] CreateOctoSprite(string name, SpriteScope scope, byte[] rawData, float rectSize, float ppu = 64f)
        {
            Rect Area(int x, int y) => new Rect(x * rectSize, y * rectSize, rectSize, rectSize);

            return new RogueSprite[]
            {
                RogueLibs.CreateCustomSprite(name + "N", scope, rawData, Area(1, 0), ppu),
                RogueLibs.CreateCustomSprite(name + "NE", scope, rawData, Area(2, 0), ppu),
                RogueLibs.CreateCustomSprite(name + "E", scope, rawData, Area(2, 1), ppu),
                RogueLibs.CreateCustomSprite(name + "SE", scope, rawData, Area(2, 2), ppu),
                RogueLibs.CreateCustomSprite(name + "S", scope, rawData, Area(1, 2), ppu),
                RogueLibs.CreateCustomSprite(name + "SW", scope, rawData, Area(0, 2), ppu),
                RogueLibs.CreateCustomSprite(name + "W", scope, rawData, Area(0, 1), ppu),
                RogueLibs.CreateCustomSprite(name + "NW", scope, rawData, Area(0, 0), ppu),
            };
        }

    }
}
