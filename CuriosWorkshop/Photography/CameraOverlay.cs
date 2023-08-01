using System;
using Light2D;
using RogueLibsCore;
using UnityEngine;
using UnityEngine.UI;

namespace CuriosWorkshop
{
    public class CameraOverlay : CustomUserOverlay
    {
        public Agent Owner => MainGUI.agent;

        public Image frame = null!;
        public Image flash = null!;

        private float prevAlpha;
        private float prevFlashAlpha;
        private uint activityCounterPrev;
        private uint activityCounter;

        private Vector2 moveSpeed;

        public override void Setup()
        {
            graphicRaycaster.enabled = false;

            flash = CreateElement<Image>("Flash", new Rect(0f, 0f, 400f, 300f));
            SetCenterPosition(flash.gameObject, transform, new Rect(0f, 0f, 400f, 300f));

            Texture2D white = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            flash.sprite = Sprite.Create(white, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            flash.color = Color.white.WithAlpha(0f);

            frame = CreateElement<Image>("CameraFrame", new Rect(0f, 0f, 400f, 300f));
            SetCenterPosition(frame.gameObject, transform, new Rect(0f, 0f, 400f, 300f));
        }

        public void Set(CameraOverlayType type, Rect area, Vector2Int size)
        {
            Camera screenCamera = GameController.gameController.cameraScript.actualCamera.ScreenCamera;

            Vector2 targetPos = screenCamera.WorldToScreenPoint(area.center);
            Vector2 prevPos = frame.rectTransform.position;
            Vector3 newPos = prevAlpha > 0f ? Vector2.SmoothDamp(prevPos, targetPos, ref moveSpeed, 0.1f) : targetPos;

            frame.sprite = type.Frame;
            frame.rectTransform.position = newPos;
            flash.rectTransform.position = newPos;

            Vector2 sizeMultiplier = (Vector2)size / type.Size;
            frame.rectTransform.sizeDelta = type.Size * sizeMultiplier;
            flash.rectTransform.sizeDelta = type.Size * sizeMultiplier;

            prevAlpha = Mathf.Clamp01(prevAlpha + 4f * Time.deltaTime);
            frame.color = frame.color.WithAlpha(prevAlpha);

            activityCounter++;
        }

        public void Update()
        {
            if (activityCounter == activityCounterPrev)
            {
                activityCounter = 0;
                if (prevAlpha > 0f)
                {
                    float multiplier = prevFlashAlpha > 0f ? 2f : 4f;
                    prevAlpha = Mathf.Clamp01(Math.Min(prevAlpha, 1.5f) - multiplier * Time.deltaTime);
                    frame.color = frame.color.WithAlpha(prevAlpha);
                }
            }
            activityCounterPrev = activityCounter;

            if (prevFlashAlpha > 0f)
            {
                prevFlashAlpha = Mathf.Clamp01(Mathf.Min(prevFlashAlpha, 1.5f) - 2f * Time.deltaTime);
                flash.color = flash.color.WithAlpha(prevFlashAlpha);
            }
        }

        public Texture2D Capture(Action<Action>? wrapper = null)
        {
            Vector2 relativeSize = Vector2Int.RoundToInt(frame.rectTransform.sizeDelta);

            tk2dCamera tk2dCamera = GameController.gameController.cameraScript.actualCamera;
            Camera screenCamera = tk2dCamera.ScreenCamera;
            Transform tr = screenCamera.transform;

            Vector3 prevPos = tr.position;
            float prevZoom = tk2dCamera.ZoomFactor;
            Texture2D? screenshot = null;
            try
            {
                const int detailLevel = 2;
                float screenMultiplier = Screen.width / 1920f;
                float newZoomMultiplier = 1920f / relativeSize.x;
                Vector2Int renderSize = Vector2Int.RoundToInt(relativeSize * screenMultiplier * detailLevel);

                Vector2 center = screenCamera.ScreenToWorldPoint(frame.rectTransform.position);
                tr.position = new Vector3(center.x, center.y, prevPos.z);
                tk2dCamera.ZoomFactor *= newZoomMultiplier;

                bool rendered = false;

                void RenderToTexture()
                {
                    rendered = true;
                    RenderTexture prevRender = screenCamera.targetTexture;

                    // create a render texture and render on it
                    RenderTexture render = new RenderTexture(renderSize.x, renderSize.y, 24);
                    screenCamera.targetTexture = render;
                    screenCamera.Render();

                    // set the render texture and read pixels from it
                    RenderTexture.active = render;
                    screenshot = new Texture2D(renderSize.x, renderSize.y, TextureFormat.ARGB32, false);
                    screenshot.ReadPixels(new Rect(0, 0, renderSize.x, renderSize.y), 0, 0);
                    screenshot.Apply();

                    // return previous values
                    screenCamera.targetTexture = prevRender;
                    RenderTexture.active = null;
                }
                wrapper ??= static render => render();

                PhotoUtils.WithPhotoVision(() =>
                {
                    try
                    {
                        wrapper(RenderToTexture);
                    }
                    catch (Exception e)
                    {
                        if (!rendered) RenderToTexture();
                        CuriosPlugin.Logger.LogError(e);
                    }
                });
            }
            finally
            {
                tr.position = prevPos;
                tk2dCamera.ZoomFactor = prevZoom;
            }

            flash.color = flash.color.WithAlpha(1f);
            prevFlashAlpha = 2f;

            return screenshot!;
        }

    }
    public readonly struct CameraOverlayType
    {
        public Sprite Frame { get; }
        public Vector2Int Size { get; }

        private CameraOverlayType(byte[] rawData, int width, int height)
        {
            Frame = RogueUtilities.ConvertToSprite(rawData);
            Size = new Vector2Int(width, height);
        }

        public static CameraOverlayType Disposable { get; } = new(Properties.Resources.CameraFrame, 400, 300);
        public static CameraOverlayType PhotoCamera { get; } = new(Properties.Resources.CameraFrame, 400, 300);
        public static CameraOverlayType Streamcorder { get; } = new(Properties.Resources.CameraFrame, 400, 300);
        public static CameraOverlayType SoulStealer { get; } = new(Properties.Resources.CameraFrame, 400, 300);
    }
}
