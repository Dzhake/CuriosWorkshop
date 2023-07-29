using System.Reflection;
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

            Texture2D src = new Texture2D(15, 9);
            src.LoadImage(Properties.Resources.FlashlightLight);
            src.filterMode = FilterMode.Bilinear;
            Sprite sprite = Sprite.Create(src, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 64, 0u, SpriteMeshType.FullRect, Vector4.zero, false);
            FlashlightSprite = sprite;

            Shader lightShader = Shader.Find("Light2D/Light 60 Points");
            Material material = new Material(lightShader);
            material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.RealtimeEmissive;

            FlashlightMaterial = material;
        }

        public static Material FlashlightMaterial = null!;
        public static Sprite FlashlightSprite = null!;

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
            Vector2 mousePos = Owner!.agentCamera.actualCamera.ScreenCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 agentPos = Owner.tr.position;
            Vector2 direction = (mousePos - agentPos).normalized;

            Transform? tr = gun.gunContainerTr.Find(nameof(FlashlightLight));
            bool justCreated = !tr;
            if (!tr)
            {
                tr = new GameObject(nameof(FlashlightLight)).transform;
                tr.SetParent(gun.gunContainerTr);
                tr.localPosition = Vector3.zero;
                tr.gameObject.AddComponent<FlashlightLight>();
            }
            if (!tr.gameObject.activeSelf || justCreated)
            {
                tr.gameObject.SetActive(true);
                gc.audioHandler.Play(Owner!, "FlashlightOn");
            }

            tr.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            tr.localPosition = new Vector3(0.32f, 0f, 0f);
            tr.localScale = gun.gunContainerTr.localScale * 8f;
        }
        public void TurnOff(Gun gun)
        {
            Transform? tr = gun.gunContainerTr.Find(nameof(FlashlightLight));
            if (tr && tr.gameObject.activeSelf)
            {
                tr.gameObject.SetActive(false);
                gc.audioHandler.Play(Owner!, "FlashlightOff");
            }
        }

    }
    public class FlashlightLight : MonoBehaviour
    {
        private LightSprite[] lightSprites = null!;
        private MeshRenderer meshRenderer = null!;

        private static readonly MethodInfo forceUpdateMesh
            = typeof(LightSprite).GetMethod("UpdateMeshData", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public void Awake()
        {
            static LightSprite CreateLight(GameObject go)
            {
                LightSprite lightSprite = go.AddComponent<LightSprite>();
                lightSprite.Sprite = Flashlight.FlashlightSprite;
                lightSprite.Material = Flashlight.FlashlightMaterial;
                lightSprite.Color = new Color32(199, 174, 120, 255);
                return lightSprite;
            }

            const int length = 8;
            lightSprites = new LightSprite[length];
            for (int i = 0; i < length; i++)
                lightSprites[i] = CreateLight(gameObject);

            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.renderingLayerMask = 4294967295u;
            transform.localScale = new Vector3(12f, 12f, 0f);
            tag = "Light";
            gameObject.layer = 11;
        }
        public void Update()
        {
            for (int i = 0, length = lightSprites.Length; i < length; i++)
                forceUpdateMesh.Invoke(lightSprites[i], new object?[] { true });
        }
    }
}
