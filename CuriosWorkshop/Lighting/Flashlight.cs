using System;
using System.Reflection;
using JetBrains.Annotations;
using Light2D;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Technology, RogueCategories.NonViolent, RogueCategories.NonStandardWeapons2, RogueCategories.NotRealWeapons)]
    public class Flashlight : CustomItem
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Flashlight>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Flashlight",
                         Russian = @"Фонарик",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithSprite(Properties.Resources.Flashlight)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 1,
                         CharacterCreationCost = 0,
                         LoadoutCost = 0,
                     });

            RogueLibs.CreateCustomAudio("FlashlightOn", Properties.Resources.FlashlightOn, AudioType.MPEG);
            RogueLibs.CreateCustomAudio("FlashlightOff", Properties.Resources.FlashlightOff, AudioType.MPEG);

            FlashlightSprite = RogueUtilities.ConvertToSprite(Properties.Resources.FlashlightLight);
            FlashlightSprite.texture.filterMode = FilterMode.Bilinear;

            FlashlightMaterial = new Material(Shader.Find("Light2D/Light 60 Points"));
            FlashlightMaterial.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.RealtimeEmissive;
            // new Color32(255, 100, 255, 255)
        }

        public static Sprite FlashlightSprite = null!;
        public static Material FlashlightMaterial = null!;

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponProjectile;
            Item.weaponCode = weaponType.WeaponProjectile;
            Item.initCount = 300 * 100;
            Item.rewardCount = 300 * 100;
            Item.itemValue = 10;
            Item.stackable = true;
            Item.hasCharges = true;

            Item.isWeapon = true;
            Item.rapidFire = true;
            Item.dontAutomaticallySelect = true;
            Item.doesNoDamage = true;
            Item.gunKnockback = 0;
        }
        public override CustomTooltip GetCountString()
            => $"{Mathf.CeilToInt(100f * Count / Item.initCount)}%";

        public void TurnOn(Gun gun)
        {
            Owner!.weaponCooldown = 0.01f;
            gun.SubtractBullets(1, Item);
            AimLight(gun);
        }
        public void AimLight(Gun gun)
        {
            FlashlightLight light = FlashlightLight.Get(gun);
            light.TurnOn(() => gc.audioHandler.Play(Owner!, "FlashlightOn"));
            light.UpdateLight(new Color32(199, 174, 120, 255));
        }
        public void TurnOff(Gun gun)
        {
            FlashlightLight light = FlashlightLight.Get(gun);
            light.TurnOff(() => gc.audioHandler.Play(Owner!, "FlashlightOff"));
        }

    }
    public class FlashlightLight : MonoBehaviour
    {
        private LightSprite[] lightSprites = null!;
        private static readonly MethodInfo forceUpdateMesh
            = typeof(LightSprite).GetMethod("UpdateMeshData", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private void Awake()
        {
            static LightSprite CreateLight(GameObject go)
            {
                LightSprite lightSprite = go.AddComponent<LightSprite>();
                lightSprite.Sprite = Flashlight.FlashlightSprite;
                lightSprite.Material = Flashlight.FlashlightMaterial;
                return lightSprite;
            }

            const int length = 1;
            lightSprites = new LightSprite[length];
            for (int i = 0; i < length; i++)
                lightSprites[i] = CreateLight(gameObject);

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.renderingLayerMask = 4294967295u;
            transform.localScale = new Vector3(12f, 12f, 0f);
            gameObject.layer = 11;
            tag = "Light";
        }
        private void Update()
        {
            for (int i = 0, length = lightSprites.Length; i < length; i++)
                forceUpdateMesh.Invoke(lightSprites[i], new object?[] { true });
        }

        public void UpdateLight(Color color)
        {
            transform.localEulerAngles = new Vector3(0f, 0f, -90f);
            Array.ForEach(lightSprites, light => light.Color = color);
        }
        public void TurnOn([InstantHandle] Action sideEffect)
        {
            if (gameObject.activeSelf) return;
            gameObject.SetActive(true);
            sideEffect();
        }
        public void TurnOff([InstantHandle] Action sideEffect)
        {
            if (!gameObject.activeSelf) return;
            gameObject.SetActive(false);
            sideEffect();
        }

        public static FlashlightLight Get(Gun gun)
        {
            Transform? tr = gun.gunContainerTr.Find(nameof(FlashlightLight));
            if (!tr)
            {
                tr = new GameObject(nameof(FlashlightLight)).transform;
                tr.SetParent(gun.gunContainerTr);
                tr.localPosition = Vector3.zero;
                tr.gameObject.SetActive(false);
                return tr.gameObject.AddComponent<FlashlightLight>();
            }
            return tr.GetComponent<FlashlightLight>();
        }

    }
}
