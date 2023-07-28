using RogueLibsCore;
using UnityEngine;
using UnityEngine.UI;

namespace CuriosWorkshop
{
    public static class PhotoUtils
    {
        public static Texture2D TakeScreenshot(Vector2Int size, int detailLevel, Vector2 position)
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
                width = Mathf.RoundToInt(widthF * detailLevel);
                height = Mathf.RoundToInt(heightF * detailLevel);

                tr.position = new Vector3(position.x, position.y, prevPos.z);
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

        public static void SetCameraOverlay(MainGUI mainGUI, Vector2 position, byte[] overlayImageData)
        {
            Transform? existing = mainGUI.transform.Find("CameraFrameOverlay");
            GameObject frame = existing ? existing.gameObject : CreateCameraOverlay(mainGUI);

            Camera screenCamera = GameController.gameController.cameraScript.actualCamera.ScreenCamera;
            frame.transform.position = screenCamera.WorldToScreenPoint(position);

            Image img = frame.GetComponent<Image>();
            img.sprite = RogueUtilities.ConvertToSprite(overlayImageData);

            RectTransform rect = frame.GetComponent<RectTransform>();
            float screenMultiplier = Screen.width / 1920f;
            rect.sizeDelta = img.sprite.rect.size * screenMultiplier;
        }

        private static GameObject CreateCameraOverlay(MainGUI mainGUI)
        {
            GameObject go = new GameObject("CameraFrameOverlay", typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(mainGUI.transform, true);
            return go;
        }

    }
}
