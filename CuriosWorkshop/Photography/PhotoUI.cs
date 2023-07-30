using System.Collections.Generic;
using System.Linq;
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

            Image window = Create<Image>(transform, "PhotoWindow", new Vector2(260, 140), Properties.Resources.PhotoWindow);

            Sprite frameSprite = RogueUtilities.ConvertToSprite(Properties.Resources.PhotoFrame);
            Image frame = Create<Image>(transform, "PhotoFrame", new Vector2(284, 224), Properties.Resources.PhotoFrame, 2f);
            frame.sprite = frameSprite;

            picture = Create<Image>(transform, "PhotoImage", new Rect(300, 240, 800, 600));

            Image summaryBackground = Create<Image>(transform, "PhotoSummary", new Vector2(1132, 172), Properties.Resources.PhotoSummaryBackground);

            const int count = 15;
            for (int i = 0; i < count; i++)
            {
                FeatureUI feature = Create<FeatureUI>(summaryBackground.transform, $"PhotoFeature {i + 1}", new Rect(8, 8 + 48 * i, 496, 48));
                features.Add(feature);
            }

        }

        public override void OnOpened()
        {
            gc.audioHandler.Play(MainGUI.agent, "ShowInterface");

            Texture2D photo = Photo!.genTexture!;
            Rect photoRect = new Rect(0f, 0f, photo.width, photo.height);
            Sprite sprite = Sprite.Create(photo, photoRect, new Vector2(0.5f, 0.5f), 64f, 0u, SpriteMeshType.FullRect, Vector4.zero);
            picture.sprite = sprite;

            PhotoFeature[]? captured = Photo.capturedFeatures;
            for (int i = 0, count = features.Count; i < count; i++)
                features[i].Set(captured is null || i >= captured.Length ? null : captured[i]);
        }
        public override void OnClosed()
        {
            gc.audioHandler.Play(MainGUI.agent, "HideInterface");
        }

        public class FeatureUI : CustomUiElement
        {
            private Text text = null!;
            private Image moneyIcon = null!;
            private Text moneyText = null!;

            public override void Awake()
            {
                text = Create<Text>(transform, "Description", new Rect(0f, 0f, 376 - 8, 48));
                text.font = gc.munroFont;
                text.fontSize = 40;
                text.alignment = TextAnchor.MiddleLeft;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;

                moneyIcon = Create<Image>(transform, "MoneyIcon", new Rect(376, 0, 48, 48));

                moneyText = Create<Text>(transform, "MoneyText", new Rect(416, 0, 80, 48));
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
    public class PhotoFeature
    {
        public string Type { get; }
        public string Name { get; }

        public float CostOffset { get; }
        public float? CostMultiplier { get; }

        public PhotoFeature(string type, string name, float offset, float? multiplier = null)
        {
            Type = type;
            Name = name;
            CostOffset = offset;
            CostMultiplier = multiplier;
        }

        public static PhotoFeature[] Create(Rect area)
        {
            Vector2 padding = new Vector2(0.32f, 0.32f);
            area = new Rect(area.min + padding, area.size - 2 * padding);

            List<PhotoFeature> list = new();

            GameController gc = GameController.gameController;
            foreach (Agent agent in gc.agentList.Where(a => area.Contains(a.tr.position)))
            {
                if (agent.agentRealName?.StartsWith("E_") is not false) continue;
                list.Add(new PhotoFeature("Agent", agent.agentRealName, 5f));
            }
            foreach (ObjectReal obj in gc.objectRealList.Where(o => area.Contains(o.tr.position)))
            {
                if (obj.objectRealRealName?.StartsWith("E_") is not false) continue;
                list.Add(new PhotoFeature("Object", obj.objectRealRealName, 5f));
            }

            return list.ToArray();
        }

    }
}
