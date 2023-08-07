using System.Reflection;
using HarmonyLib;
using Light2D;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CuriosWorkshop
{
    public static class LightingUtilities
    {
        private static readonly FieldInfo ambientLightTextureField = AccessTools.Field(typeof(LightingSystem), "_ambientTexture");
        private static readonly FieldInfo lightSourcesTextureField = AccessTools.Field(typeof(LightingSystem), "_lightSourcesTexture");

        public static Color GetLighting(Vector2 position, CameraScript? cameraScript = null)
        {
            LightingSystem system = (cameraScript ?? GameController.gameController.cameraScript).lightingSystem;
            Vector2 cameraPos = system.LightCamera.WorldToScreenPoint(position);
            RenderTexture ambientLight = (RenderTexture)ambientLightTextureField.GetValue(system);
            RenderTexture lightSources = (RenderTexture)lightSourcesTextureField.GetValue(system);
            RenderTexture darkSources = LightingPatches.GetDarkTexture(system);

            Vector2 percentPos = cameraPos / system.LightCamera.pixelRect.size;
            percentPos.y = 1f - percentPos.y;

            Vector3 ambient = GetPixel(ambientLight, percentPos);
            Vector3 lightSource = GetPixel(lightSources, percentPos);
            Vector3 darkSource = GetPixel(darkSources, percentPos);
            Vector3 result = ambient + lightSource - darkSource;
            return new Color(result.x, result.y, result.z, 1f);
        }
        public static float GetLightingLevel(Vector2 position, CameraScript? cameraScript = null)
        {
            Color color = GetLighting(position, cameraScript);
            const float invSqrt3 = 0.57735027f;
            return ((Vector3)(Vector4)color).magnitude * invSqrt3;
        }

        private static Texture2D? tempTex;
        private static Vector3 GetPixel(RenderTexture texture, Vector2 pos)
        {
            tempTex ??= new Texture2D(1, 1, texture.graphicsFormat, 0, TextureCreationFlags.None);
            tempTex.filterMode = FilterMode.Point;

            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = texture;
            tempTex.ReadPixels(new Rect(pos.x * texture.width, pos.y * texture.height, 1, 1), 0, 0, false);
            tempTex.Apply();

            RenderTexture.active = prevRT;
            return (Vector4)tempTex.GetPixel(0, 0);
        }

    }
}
