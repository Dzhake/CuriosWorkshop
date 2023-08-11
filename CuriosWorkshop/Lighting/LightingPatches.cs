using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Light2D;
using RogueLibsCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CuriosWorkshop
{
    public static class LightingPatches
    {
        public static RoguePatcher Patcher => CuriosPlugin.Patcher;
        public static GameController gc => GameController.gameController;

        public static MutatorUnlock CompleteDarknessMutator = null!;

        public static Sprite FlashlightSprite = null!;
        public static Material FlashlightMaterial = null!;

        public static bool DeadlyUltraViolet => CuriosPlugin.IsDebug;

        public static void Apply()
        {
            Patcher.TypeWithPatches = typeof(LightingPatches);

            RogueLibs.CreateCustomAudio("FlashlightOn", Properties.Resources.FlashlightOn);
            RogueLibs.CreateCustomAudio("FlashlightOff", Properties.Resources.FlashlightOff);

            FlashlightSprite = RogueUtilities.ConvertToSprite(Properties.Resources.FlashlightLight);
            FlashlightSprite.texture.filterMode = FilterMode.Bilinear;

            FlashlightMaterial = new Material(Shader.Find("Light2D/Light 60 Points"));
            FlashlightMaterial.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.RealtimeEmissive;


            RogueLibs.CreateCustomSprite("Safe", SpriteScope.Objects, Properties.Resources.SafeWithKeypad);


            RogueLibs.CreateCustomUnlock(CompleteDarknessMutator = new MutatorUnlock("CompleteDarkness")
                     {
                         Recommendations = { "RogueVision" },
                     })
                     .WithName(new CustomNameInfo
                     {
                         English = "Complete Darkness",
                         Russian = @"Полная темнота",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "Turns off ambient lighting on the levels.",
                         Russian = @"Отключает рассеянное освещение на уровнях.",
                     });

            Type[] gunShootPars = { typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(string) };
            Patcher.Prefix(typeof(Gun), nameof(Gun.Shoot), nameof(Gun_Shoot_Prefix), gunShootPars);
            Patcher.Postfix(typeof(Gun), nameof(Gun.Shoot), nameof(Gun_Shoot_Postfix), gunShootPars);
            Patcher.Finalizer(typeof(Gun), nameof(Gun.Shoot), nameof(Gun_Shoot_Finalizer), gunShootPars);

            Patcher.Postfix(typeof(Gun), nameof(Gun.ShowGun));
            //Patcher.Prefix(typeof(Gun), nameof(Gun.AimGun));
            Patcher.Prefix(typeof(Gun), nameof(Gun.HideGun));
            Patcher.Postfix(typeof(Gun), nameof(Gun.GunUpdate));

            Patcher.Postfix(typeof(InvDatabase), nameof(InvDatabase.FillAgent));

            Patcher.Postfix(typeof(Relationships), nameof(Relationships.AssessFlee));

            Patcher.Postfix(typeof(LoadLevel), nameof(LoadLevel.SetupMore5));
            Patcher.Postfix(typeof(CameraScript), nameof(CameraScript.SetLighting));
            Patcher.Prefix(typeof(SpawnerMain), nameof(SpawnerMain.SpawnLightTemp));
            Patcher.Postfix(typeof(SpawnerMain), nameof(SpawnerMain.SpawnLightReal));

            Patcher.Postfix(typeof(SpawnerMain), nameof(SpawnerMain.SetLighting2));
            Patcher.Postfix(typeof(SpawnerMain), nameof(SpawnerMain.GetLightColor));

            Patcher.Postfix(typeof(Agent), "Awake");

            Patcher.Postfix(typeof(LightingSystem), "Start");
            Patcher.Postfix(typeof(LightingSystem), "RenderLightSources");
            Patcher.Transpiler(typeof(LightingSystem), "RenderLightOverlay");

            CuriosPlugin.Instance.StartCoroutine(LoadDarkShaderCoroutine());
        }

        private static bool gunShooting;
        private static InvItem? gunLastItem;
        public static void Gun_Shoot_Prefix() => gunShooting = true;
        public static void Gun_Shoot_Postfix(Gun __instance)
        {
            IFlashlight? flashlight = gunLastItem?.GetHook<IFlashlight>();
            if (flashlight is not null) __instance.gunContainerAnim.Play("", -1, 0f);
        }
        public static void Gun_Shoot_Finalizer() => gunShooting = false;

        public static void Gun_ShowGun(Gun __instance, InvItem myGun)
        {
            gunLastItem = myGun;
            if (!gunShooting) return;
            myGun.GetHook<IFlashlight>()?.TurnOn(__instance);
        }
        public static void Gun_AimGun(Gun __instance)
            => __instance.visibleGun?.GetHook<IFlashlight>()?.AimLight(__instance);
        public static void Gun_HideGun(Gun __instance)
            => __instance.visibleGun?.GetHook<IFlashlight>()?.TurnOff(__instance);
        public static void Gun_GunUpdate(Gun __instance)
        {
            Agent agent = __instance.agent;
            if (!__instance.holdingAttack && agent.isPlayer > 0)
            {
                IFlashlight? flashlight = __instance.visibleGun?.GetHook<IFlashlight>();
                if (flashlight is not null) __instance.HideGun();
            }
            InvItem? weapon = agent.inventory.equippedWeapon;
            if (agent.isPlayer == 0 && agent.opponent == null && !agent.inCombat && IsCompleteDarkness)
            {
                if (LightingUtilities.GetLightingLevel(agent.curPosition) > 0.4f)
                    return;

                if (__instance.agent.HasTrait(VanillaTraits.StubbyFingers)
                 || __instance.agent.HasTrait(VanillaTraits.SausageFingers)
                 || __instance.agent.HasTrait(VanillaTraits.PeaBrained)) return;

                Flashlight? flashlight = agent.inventory.GetItem<Flashlight>();
                if (flashlight is not null)
                {
                    agent.inventory.equippedWeapon = flashlight.Item;
                    __instance.ShowGun(flashlight.Item);
                    flashlight.AimLight(__instance);
                }
            }
            if (agent.isPlayer == 0 && (agent.opponent != null || agent.inCombat) && weapon.HasHook<IFlashlight>())
                agent.inventory.ChooseWeapon();
            //if (agent.isPlayer == 0)
            //weapon.GetHook<IFlashlight>()?.AimLight(__instance);
        }

        public static void InvDatabase_FillAgent(InvDatabase __instance)
        {
            if (__instance.agent.isPlayer == 0 && IsCompleteDarkness && !__instance.HasItem<Flashlight>() && Random.Range(0, 5) != 0)
            {
                if (__instance.agent.HasTrait(VanillaTraits.StubbyFingers)
                 || __instance.agent.HasTrait(VanillaTraits.SausageFingers)
                 || __instance.agent.HasTrait(VanillaTraits.PeaBrained)) return;

                __instance.AddItem<Flashlight>(30*100);
            }
        }

        public static void Relationships_AssessFlee(Relationships __instance, Agent ___agent, Agent otherAgent, ref float __result)
        {
            bool isAffected = DeadlyUltraViolet || ___agent.specialAbility == VanillaAbilities.Bite || ___agent.zombified;
            if (isAffected && otherAgent.inventory.equippedWeapon?.HasHook<Blacklight>() is true)
                __result = 999999f;
        }

        public static bool IsCompleteDarkness
            => gc.levelType != "HomeBase" && gc.challenges.Contains(CompleteDarknessMutator.Name);

        public static void LoadLevel_SetupMore5()
        {
            if (IsCompleteDarkness)
                gc.cameraScript.SetLighting();
        }
        public static void CameraScript_SetLighting(CameraScript __instance)
            => __instance.lightingSystem.EnableAmbientLight = !IsCompleteDarkness;
        public static bool SpawnerMain_SpawnLightTemp(string lightType)
            => !(lightType == "Item" && IsCompleteDarkness);
        public static void SpawnerMain_SpawnLightReal(LightReal __result)
        {
            if (IsCompleteDarkness && __result.fancyLight)
            {
                __result.fancyLight.Color = Color.black;
                __result.fancyLight.gameObject.SetActive(false);
            }
        }

        public static void SpawnerMain_SetLighting2(PlayfieldObject myObject)
        {
            if (myObject is Agent agent && IsCompleteDarkness)
            {
                if (!agent.agentSpriteTransform) return;
                LightSprite? light = agent.agentSpriteTransform.Find("LightRealAgent")?.GetComponent<LightSprite>();
                if (!light || light is null) return;
                if (agent.isPlayer > 0)
                {
                    light.Color.a = Mathf.Min(1f, light.Color.a * 1.5f);
                    light.gameObject.SetActive(true);
                    return;
                }
                light.Color = Color.black;
                light.gameObject.SetActive(false);
            }
        }
        public static void SpawnerMain_GetLightColor(ref Color __result)
        {
            if (IsCompleteDarkness)
                __result.a = Mathf.Min(1f, __result.a * 1.5f);
        }

        public static void Agent_Awake(Agent __instance)
        {
            if (IsCompleteDarkness && __instance.rogueLight is not null)
                __instance.rogueLight.Color = Color.black;
        }

        public static void LightingSystem_Start(LightingSystem __instance)
        {
            DarkTextures.Remove(__instance);
            DarkTextures.Add(__instance, CreateDarkTexture(__instance));
        }
        public static void LightingSystem_RenderLightSources(LightingSystem __instance)
            => RenderDarkTexture(__instance);

        public static IEnumerable<CodeInstruction> LightingSystem_RenderLightOverlay(IEnumerable<CodeInstruction> code)
        {
            IEnumerable<CodeInstruction> modified = code.AddRegionAfter(new Func<CodeInstruction, bool>[]
            {
                static i => i.opcode == OpCodes.Ldstr && (string)i.operand == "_Scale",
                static _ => true,
                static _ => true,
                static i => i.opcode == OpCodes.Callvirt,
            }, new Func<CodeInstruction[], CodeInstruction>[]
            {
                static _ => new CodeInstruction(OpCodes.Ldarg_0),
                static _ => new CodeInstruction(OpCodes.Call, typeof(LightingPatches).GetMethod(nameof(SetDarkUniforms))),
            });
            CodeInstruction[] prepend =
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, typeof(LightingPatches).GetMethod(nameof(InitializeDarkShader))),
            };
            return prepend.Concat(modified);
        }

        private static Shader? DarkShader;
        public const int DarkSourceLayer = 7;
        public const int LightSourceLayer = 11;

        private static readonly ConditionalWeakTable<LightingSystem, RenderTexture> DarkTextures = new();
        public static RenderTexture GetDarkTexture(LightingSystem system)
        {
            FieldInfo sizeField = AccessTools.Field(typeof(LightingSystem), "_lightTextureSize");
            Point2 size = (Point2)sizeField.GetValue(system);
            RenderTexture tex = DarkTextures.GetValue(system, CreateDarkTexture);
            if (tex.width != size.x || tex.height != size.y)
            {
                DarkTextures.Remove(system);
                DarkTextures.Add(system, tex = CreateDarkTexture(system));
            }
            return tex;
        }
        private static RenderTexture CreateDarkTexture(LightingSystem system)
        {
            FieldInfo sizeField = AccessTools.Field(typeof(LightingSystem), "_lightTextureSize");
            FieldInfo formatField = AccessTools.Field(typeof(LightingSystem), "_texFormat");
            Point2 size = (Point2)sizeField.GetValue(system);
            RenderTextureFormat format = (RenderTextureFormat)formatField.GetValue(system);
            return new RenderTexture(size.x, size.y, 0, format);
        }

        private static IEnumerator LoadDarkShaderCoroutine()
        {
            AssetBundleCreateRequest req = AssetBundle.LoadFromMemoryAsync(Properties.Resources.DarkLightOverlayShader);
            yield return req;
            AssetBundle bundle = req.assetBundle;
            DarkShader = bundle.LoadAsset<Shader>("DarkLightOverlay");
        }

        public static void InitializeDarkShader(LightingSystem __instance)
        {
            if (DarkShader is null || __instance.LightOverlayMaterial.shader == DarkShader) return;
            __instance.LightOverlayMaterial.shader = DarkShader;
        }
        public static void RenderDarkTexture(LightingSystem __instance)
        {
            __instance.LightCamera.targetTexture = GetDarkTexture(__instance);
            __instance.LightCamera.cullingMask = 1 << DarkSourceLayer;
            __instance.LightCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            __instance.LightCamera.Render();
            __instance.LightCamera.targetTexture = null;
            __instance.LightCamera.cullingMask = 0;
        }
        public static void SetDarkUniforms(LightingSystem __instance)
        {
            if (__instance.LightOverlayMaterial.shader != DarkShader) return;
            __instance.LightOverlayMaterial.SetTexture("_DarkSourcesTex", GetDarkTexture(__instance));
        }

    }
    public interface IFlashlight
    {
        void TurnOn(Gun gun);
        void TurnOff(Gun gun);
        void AimLight(Gun gun);
    }
}
