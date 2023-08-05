using System;
using System.Reflection;
using JetBrains.Annotations;
using Light2D;
using UnityEngine;

namespace CuriosWorkshop
{
    public class MoveableLightSource : MonoBehaviour
    {
        private LightSprite[] lightSprites = null!;
        private static readonly MethodInfo forceUpdateMesh
            = typeof(LightSprite).GetMethod("UpdateMeshData", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private void Awake()
        {
            static LightSprite CreateLight(GameObject go)
            {
                LightSprite lightSprite = go.AddComponent<LightSprite>();
                lightSprite.Sprite = LightingPatches.FlashlightSprite;
                lightSprite.Material = LightingPatches.FlashlightMaterial;
                lightSprite.Material.SetFloat("_ObstacleMul", 2000f);
                lightSprite.Material.SetFloat("_EmissionColorMul", 1f);
                return lightSprite;
            }

            const int length = 1;
            lightSprites = new LightSprite[length];
            for (int i = 0; i < length; i++)
                lightSprites[i] = CreateLight(gameObject);

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.renderingLayerMask = 4294967295u;
            transform.localScale = new Vector3(12f, 12f, 0f);
            gameObject.layer = LightingPatches.LightSourceLayer;
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

        public static MoveableLightSource Get(Gun gun)
        {
            Transform? tr = gun.gunContainerTr.Find(nameof(MoveableLightSource));
            if (!tr)
            {
                tr = new GameObject(nameof(MoveableLightSource)).transform;
                tr.SetParent(gun.gunContainerTr);
                tr.localPosition = Vector3.zero;
                tr.gameObject.SetActive(false);
                return tr.gameObject.AddComponent<MoveableLightSource>();
            }
            return tr.GetComponent<MoveableLightSource>();
        }

    }
}
