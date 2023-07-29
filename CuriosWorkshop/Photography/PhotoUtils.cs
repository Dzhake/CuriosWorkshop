using Light2D;
using RogueLibsCore;
using UnityEngine;
using UnityEngine.UI;

namespace CuriosWorkshop
{
    public static class PhotoUtils
    {
        public static Texture2D TakeScreenshot(Vector2 center, Vector2Int size)
        {
            int width = size.x;
            int height = size.y;
            float screenMultiplier = Screen.width / 1920f;
            float widthF = width * screenMultiplier;
            float heightF = height * screenMultiplier;

            tk2dCamera tk2dCamera = GameController.gameController.cameraScript.actualCamera;
            Camera camera = tk2dCamera.ScreenCamera;
            Transform tr = camera.transform;

            Vector3 prevPos = tr.position;
            float prevZoom = tk2dCamera.ZoomFactor;
            Texture2D? screenshot = null;

            try
            {
                const int detailLevel = 1;
                width = Mathf.RoundToInt(widthF * detailLevel);
                height = Mathf.RoundToInt(heightF * detailLevel);

                tr.position = new Vector3(center.x, center.y, prevPos.z);
                tk2dCamera.ZoomFactor *= detailLevel * (Screen.width / widthF);

                CuriosPlugin.WithHiddenInterface(() =>
                {
                    RenderTexture prevRender = camera.targetTexture;

                    // create a render texture and render on it
                    RenderTexture render = new RenderTexture(width, height, 24);
                    camera.targetTexture = render;
                    camera.Render();

                    // set the render texture and read pixels from it
                    RenderTexture.active = render;
                    screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
                    screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

                    // return previous values
                    camera.targetTexture = prevRender;
                    RenderTexture.active = null;
                });
            }
            finally
            {
                tr.position = prevPos;
                tk2dCamera.ZoomFactor = prevZoom;
            }
            return screenshot!;
        }

        public static void SetCameraOverlay(MainGUI mainGUI, Vector2 center, Vector2Int size, CameraOverlayType overlayType)
        {
            CameraOverlay overlay = GetCameraOverlay(mainGUI);

            overlay.Set(overlayType, center, size);
        }
        private static CameraOverlay GetCameraOverlay(MainGUI mainGUI)
        {
            Transform? existing = mainGUI.transform.Find("CameraFrameOverlay");
            if (existing) return existing.GetComponent<CameraOverlay>();

            GameObject go = new GameObject("CameraFrameOverlay", typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(mainGUI.transform, true);
            return go.AddComponent<CameraOverlay>();
        }

    }
    public class CameraOverlay : MonoBehaviour
    {
        public RectTransform rect = null!;
        public Image img = null!;

        private float prevAlpha;
        private uint activeCounterPrev;
        private uint activeCounter;

        private Vector2 moveSpeed;

        public void Awake()
        {
            rect = gameObject.GetComponent<RectTransform>();
            img = gameObject.AddComponent<Image>();
        }

        public void Set(CameraOverlayType type, Vector2 center, Vector2Int size)
        {
            Camera screenCamera = GameController.gameController.cameraScript.actualCamera.ScreenCamera;
            Vector2 targetPos = screenCamera.WorldToScreenPoint(center);
            CuriosPlugin.Logger.LogDebug($"{center} - {screenCamera.transform.position} >> {targetPos}");

            transform.position = Vector2.SmoothDamp(transform.position, targetPos, ref moveSpeed, 0.1f);

            img.sprite = type.Sprite;
            float screenMultiplier = Screen.width / 1920f;
            Vector2 sizeMultiplier = size / type.Size;
            rect.sizeDelta = type.Sprite.rect.size * sizeMultiplier * screenMultiplier;

            float newAlpha = Mathf.Min(prevAlpha + 2f * Time.deltaTime, 1.5f);
            img.color = img.color.WithAlpha(Mathf.Clamp01(newAlpha));
            prevAlpha = newAlpha;

            activeCounter++;
        }

        public void Update()
        {
            if (activeCounter == activeCounterPrev)
            {
                activeCounter = 0;
                float newAlpha = Mathf.Clamp01(prevAlpha - 2f * Time.deltaTime);
                img.color = img.color.WithAlpha(newAlpha);
                prevAlpha = newAlpha;
            }
            activeCounterPrev = activeCounter;
        }

    }
    public readonly struct CameraOverlayType
    {
        public Sprite Sprite { get; }
        public Vector2 Size { get; }

        public CameraOverlayType(byte[] rawData, int width, int height)
        {
            Sprite = RogueUtilities.ConvertToSprite(rawData);
            Size = new Vector2(width, height);
        }

        public static CameraOverlayType Disposable { get; } = new(Properties.Resources.PhotoFrame, 400, 300);
        public static CameraOverlayType Normal { get; } = new(Properties.Resources.PhotoFrame, 400, 300);
        public static CameraOverlayType Streamcorder { get; } = new(Properties.Resources.PhotoFrame, 400, 300);
        public static CameraOverlayType SoulStealer { get; } = new(Properties.Resources.PhotoFrame, 400, 300);
    }
}
