using System.Collections.Generic;
using System.Linq;
using RogueLibsCore;
using UnityEngine;
using UnityEngine.UI;

namespace CuriosWorkshop
{
    public class PhotoUI : MonoBehaviour
    {
        public static PhotoUI Get(MainGUI gui)
        {
            Transform? tr = gui.transform.Find(nameof(PhotoUI));
            if (!tr)
            {
                tr = new GameObject(nameof(PhotoUI), typeof(RectTransform)).transform;
                tr.SetParent(gui.transform, true);
                tr.localPosition = Vector3.zero;
                tr.localScale = Vector3.one;
                tr.gameObject.AddComponent<PhotoUI>();
            }
            return tr.GetComponent<PhotoUI>();
        }

        private bool showingPhoto;
        public bool IsOpened => showingPhoto;

        private MainGUI mainGUI = null!;
        private RectTransform rect = null!;
        private Canvas canvas = null!;

        private Image picture = null!;
        private readonly List<FeatureUI> features = new();

        public static GameController gc => GameController.gameController;

        private T Create<T>(string goName, GameObject? parent = null) where T : Component
        {
            GameObject go = new GameObject(goName, typeof(RectTransform));
            RectTransform goRect = go.GetComponent<RectTransform>();
            goRect.SetParent((parent ?? gameObject).transform, true);
            goRect.localPosition = Vector3.zero;
            goRect.localScale = Vector3.one;
            return go.AddComponent<T>();
        }

        public void Awake()
        {
            mainGUI = gameObject.GetComponentInParent<MainGUI>();
            rect = gameObject.GetComponent<RectTransform>();
            canvas = gameObject.AddComponent<Canvas>();
            canvas.enabled = false;
            Image background = gameObject.AddComponent<Image>();

            gameObject.AddComponent<GraphicRaycaster>();

            background.sprite = RogueUtilities.ConvertToSprite(Properties.Resources.PhotoWindow);
            rect.sizeDelta = background.sprite.rect.size;

            Image frame = Create<Image>("PhotoFrame");
            frame.sprite = RogueUtilities.ConvertToSprite(Properties.Resources.PhotoFrame);
            frame.rectTransform.sizeDelta = frame.sprite.rect.size * 2f;
            frame.rectTransform.localPosition = new Vector3(-260, 0);

            picture = Create<Image>("PhotoImage");
            picture.rectTransform.sizeDelta = new Vector2(400f, 300f) * 2f;
            picture.rectTransform.localPosition = new Vector3(-260, 0);

            Image summaryBackground = Create<Image>("PhotoSummary");
            summaryBackground.sprite = RogueUtilities.ConvertToSprite(Properties.Resources.PhotoSummaryBackground);
            summaryBackground.rectTransform.sizeDelta = summaryBackground.sprite.rect.size;
            summaryBackground.rectTransform.localPosition = new Vector3(428, -12);

            Rect summaryRect = summaryBackground.rectTransform.rect;
            const int count = 15;
            for (int i = 0; i < count; i++)
            {
                FeatureUI feature = Create<FeatureUI>($"PhotoFeature {i + 1}", summaryBackground.gameObject);
                feature.rect.localPosition = new Vector3(summaryRect.center.x, summaryRect.yMax - 8f - 24f) - new Vector3(0, 48) * i;
                features.Add(feature);
            }

        }

        public void Hide()
        {
            if (!showingPhoto) return;
            gc.audioHandler.Play(mainGUI.agent, "HideInterface");
            showingPhoto = false;
            canvas.enabled = false;
        }
        public void Show(Photo photo)
        {
            gc.audioHandler.Play(mainGUI.agent, "ShowInterface");
            showingPhoto = false;
            mainGUI.HideEverything();
            mainGUI.agent.worldSpaceGUI.HideEverything2();

            showingPhoto = true;
            canvas.enabled = true;

            Rect photoRect = new Rect(0f, 0f, photo.genTexture!.width, photo.genTexture.height);
            Sprite sprite = Sprite.Create(photo.genTexture, photoRect, new Vector2(0.5f, 0.5f), 64f, 0u, SpriteMeshType.FullRect, Vector4.zero);
            picture.sprite = sprite;

            PhotoFeature[]? captured = photo.capturedFeatures;
            for (int i = 0, count = features.Count; i < count; i++)
                features[i].Set(captured is null || i >= captured.Length ? null : captured[i]);
        }

        public class FeatureUI : MonoBehaviour
        {
            public RectTransform rect = null!;
            private Text text = null!;
            private Image moneyIcon = null!;
            private Text moneyText = null!;

            private T Create<T>(string goName, GameObject? parent = null) where T : Component
            {
                GameObject go = new GameObject(goName, typeof(RectTransform));
                RectTransform goRect = go.GetComponent<RectTransform>();
                goRect.SetParent((parent ?? gameObject).transform, true);
                goRect.localPosition = Vector3.zero;
                goRect.localScale = Vector3.one;
                return go.AddComponent<T>();
            }

            public void Awake()
            {
                rect = gameObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(496, 48);

                text = Create<Text>("Description");
                text.rectTransform.sizeDelta = new Vector2(376 - 8, 48);
                text.rectTransform.localPosition = new Vector3(-60, 0);
                text.font = GameController.gameController.munroFont;
                text.fontSize = 40;
                text.alignment = TextAnchor.MiddleLeft;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;

                moneyIcon = Create<Image>("MoneyIcon");
                moneyIcon.rectTransform.sizeDelta = new Vector2(48, 48);
                moneyIcon.rectTransform.localPosition = new Vector3(148, 0);

                moneyText = Create<Text>("MoneyText");
                moneyText.rectTransform.sizeDelta = new Vector2(80 - 8, 48);
                moneyText.rectTransform.localPosition = new Vector3(208, -4);
                moneyText.font = GameController.gameController.munroFont;
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
