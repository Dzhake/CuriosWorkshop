using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;
using UnityEngine.UI;

namespace CuriosWorkshop
{
    public class PhotoUI : CustomUserInterface
    {
        private Image picture = null!;
        private readonly List<FeatureUI> features = new();

        public Photo? Photo;

        public override void Setup()
        {
            gameObject.AddComponent<GraphicRaycaster>();

            Image window = CreateElement<Image>("PhotoWindow", new Rect(0, 0, 1920, 1080));
            window.sprite = RogueUtilities.ConvertToSprite(Properties.Resources.PhotoWindow);
            window.alphaHitTestMinimumThreshold = 0.1f;

            picture = CreateElement<Image>("PhotoImage", new Rect(555, 291, 400, 300));

            GameObject summary = CreateElement("PhotoSummary", new Vector2(1132, 172));

            const int count = 15;
            for (int i = 0; i < count; i++)
            {
                FeatureUI feature = CreateElement<FeatureUI>(summary.transform, $"PhotoFeature {i + 1}", new Rect(4, 4 + 24 * i, 248, 24));
                features.Add(feature);
            }

        }

        public override void OnOpened()
        {
            Texture2D photo = Photo!.genTexture!;
            Rect photoRect = new Rect(0f, 0f, photo.width, photo.height);
            Sprite sprite = Sprite.Create(photo, photoRect, new Vector2(0.5f, 0.5f), 64f, 0u, SpriteMeshType.FullRect, Vector4.zero);
            picture.sprite = sprite;

            PhotoFeature[]? captured = Photo.capturedFeatures;
            for (int i = 0, count = features.Count; i < count; i++)
                features[i].Set(null);
                // features[i].Set(captured is null || i >= captured.Length ? null : captured[i]);
        }
        public override void OnClosed()
        {
        }

        public class FeatureUI : CustomUiElement
        {
            private Text text = null!;
            private Image moneyIcon = null!;
            private Text moneyText = null!;

            public override void Awake()
            {
                text = CreateElement<Text>("Description", new Rect(0f, 0f, 376 - 8, 48));
                text.font = gc.munroFont;
                text.fontSize = 40;
                text.alignment = TextAnchor.MiddleLeft;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;

                moneyIcon = CreateElement<Image>("MoneyIcon", new Rect(376, 0, 48, 48));

                moneyText = CreateElement<Text>("MoneyText", new Rect(416, 0, 80, 48));
                moneyText.font = gc.munroFont;
                moneyText.fontSize = 40;
                moneyText.color = Color.green;
                moneyText.alignment = TextAnchor.MiddleLeft;
                moneyText.horizontalOverflow = HorizontalWrapMode.Overflow;
                moneyText.verticalOverflow = VerticalWrapMode.Overflow;

            }
            public void Set(PhotoFeature? feature)
            {
                if (feature is null)
                {
                    text.text = "";
                    moneyIcon.gameObject.SetActive(false);
                    moneyText.text = "";
                    return;
                }
                text.text = $"{feature.Name}";

                if (feature.CostOffset > 0)
                {
                    moneyIcon.gameObject.SetActive(true);
                    GameResources gr = GameController.gameController.gameResources;
                    moneyIcon.sprite = feature.CostOffset switch
                    {
                        0f => null,
                        < 10f => gr.itemDic["MoneyA"],
                        < 25f => gr.itemDic["MoneyB"],
                        < 50f => gr.itemDic["MoneyC"],
                        _ => gr.itemDic["Money"],
                    };
                }
                else moneyIcon.gameObject.SetActive(false);

                moneyText.text = $"+{feature.CostOffset}";

            }

        }

    }
}
