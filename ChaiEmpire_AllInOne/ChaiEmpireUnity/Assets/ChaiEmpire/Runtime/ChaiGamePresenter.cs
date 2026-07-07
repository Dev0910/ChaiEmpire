using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChaiEmpire
{
    public sealed class ChaiGamePresenter : MonoBehaviour
    {
        private const string CanvasName = "Chai Empire Canvas";
        private const string SafeAreaRootName = "SafeAreaRoot";
        private const string MainScrollViewName = "MainScrollView";
        private const string ViewportName = "Viewport";
        private const string ContentColumnName = "ContentColumn";
        private const string ModalLayerName = "ModalLayer";
        private const float ResetConfirmSeconds = 6f;
        private const float PrestigeConfirmSeconds = 8f;

        private static readonly Color Background = new Color(0.04f, 0.12f, 0.14f);
        private static readonly Color Panel = new Color(1.00f, 0.94f, 0.78f);
        private static readonly Color Card = new Color(1.00f, 0.98f, 0.89f);
        private static readonly Color Ink = new Color(0.12f, 0.08f, 0.05f);
        private static readonly Color MutedInk = new Color(0.39f, 0.30f, 0.23f);
        private static readonly Color Cream = new Color(1.00f, 0.88f, 0.58f);
        private static readonly Color Saffron = new Color(0.95f, 0.42f, 0.12f);
        private static readonly Color Gold = new Color(1.00f, 0.72f, 0.20f);
        private static readonly Color Teal = new Color(0.04f, 0.45f, 0.43f);
        private static readonly Color Leaf = new Color(0.15f, 0.56f, 0.30f);
        private static readonly Color Rose = new Color(0.69f, 0.14f, 0.24f);
        private static readonly Color Purple = new Color(0.39f, 0.20f, 0.62f);
        private static readonly Color Disabled = new Color(0.48f, 0.47f, 0.43f);
        private static Sprite circleSprite;
        private static AudioClip buttonPressClip;
        private static AudioClip purchaseClip;
        private static AudioClip unlockClip;

        private readonly List<UpgradeRow> upgradeRows = new List<UpgradeRow>();
        private readonly List<LocationRow> locationRows = new List<LocationRow>();
        private readonly List<PrestigeSkillRow> prestigeSkillRows = new List<PrestigeSkillRow>();
        private readonly List<SteamWisp> steamWisps = new List<SteamWisp>();
        private readonly List<FloatingReward> floatingRewards = new List<FloatingReward>();

        private ChaiContent content;
        private ChaiGame game;
        private AudioSource audioSource;
        private Text demandPillText;
        private Text stallSignText;
        private Text kettleButtonLabel;
        private RectTransform kettleButtonRect;
        private RectTransform tapPulseRect;
        private Image tapPulseImage;
        private Image rushBurstImage;
        private GameObject rushBanner;
        private RectTransform feedbackLayer;
        private Text rupeesText;
        private Text rateText;
        private Text tapText;
        private Text servedText;
        private Text locationText;
        private Text legacyText;
        private GameObject tutorialPanel;
        private Text tutorialTitleText;
        private Text tutorialBodyText;
        private Text tutorialProgressText;
        private Button tutorialPrimaryButton;
        private Text tutorialPrimaryLabel;
        private Image stallArtBackground;
        private Image stallArtGlow;
        private Image stallCounterTop;
        private Image stallCounterFront;
        private GameObject offlineRewardModal;
        private Text offlineRewardAmountText;
        private Text offlineRewardDetailText;
        private Text offlineRewardCapText;
        private Button offlineSponsorButton;
        private Text offlineSponsorLabel;
        private Text eventText;
        private Button eventButton;
        private Text eventButtonLabel;
        private Text monetizationText;
        private Button sponsorBoostButton;
        private Text sponsorBoostLabel;
        private Button noAdsButton;
        private Text noAdsLabel;
        private Text productionText;
        private Button privacyPolicyButton;
        private Text privacyPolicyLabel;
        private Button analyticsConsentButton;
        private Text analyticsConsentLabel;
        private Button adsConsentButton;
        private Text adsConsentLabel;
        private Button crashConsentButton;
        private Text crashConsentLabel;
        private Button cloudSaveExportButton;
        private Text cloudSaveExportLabel;
        private Button stallThemeButton;
        private Text stallThemeLabel;
        private Button cupPackButton;
        private Text cupPackLabel;
        private Button signboardPackButton;
        private Text signboardPackLabel;
        private Text rushText;
        private Text statusText;
        private Text prestigeText;
        private Button prestigeButton;
        private Text prestigeButtonLabel;
        private Button rushButton;
        private Button hapticsToggleButton;
        private Text hapticsToggleLabel;
        private Button resetSaveButton;
        private Text resetSaveLabel;
        private GameObject upgradeDetailModal;
        private Text upgradeDetailTitleText;
        private Text upgradeDetailCategoryText;
        private Text upgradeDetailLevelText;
        private Text upgradeDetailEffectText;
        private Text upgradeDetailCostText;
        private Text upgradeDetailDescriptionText;
        private Text upgradeDetailBuyLabel;
        private Button upgradeDetailBuyButton;
        private UpgradeDefinition selectedUpgrade;
        private float refreshTimer;
        private float saveTimer;
        private float statusTimer;
        private float resetConfirmTimer;
        private float prestigeConfirmTimer;
        private float steamTimer;
        private float tapFeedbackTimer;
        private int floatingRewardSeed;
        private bool hapticsEnabled = true;
        private bool resetSaveArmed;
        private bool prestigeArmed;
        private bool offlineSponsorClaimed;
        private OfflineReward pendingOfflineReward;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            content = ChaiContent.CreateDefault();
            LoadResult loadResult = ChaiSaveRepository.LoadOrCreate(content);
            game = ChaiGame.FromState(content, loadResult.State);

            ClearGeneratedUi();
            BuildUi();

            if (loadResult.HasOfflineReward)
            {
                ShowOfflineReward(loadResult.OfflineReward);
                SetStatus("Welcome back: " + ChaiNumberFormatter.Rupees(loadResult.OfflineReward.RupeesEarned));
            }
            else
            {
                SetStatus("Gali Tapri is open");
            }

            RefreshAll();
        }

        private void Update()
        {
            if (game == null)
            {
                return;
            }

            game.Tick(Time.deltaTime);
            AnimateSteam(Time.deltaTime);
            AnimateTapFeedback(Time.deltaTime);
            AnimateFloatingRewards(Time.deltaTime);
            saveTimer += Time.deltaTime;
            refreshTimer += Time.deltaTime;

            if (statusTimer > 0)
            {
                statusTimer -= Time.deltaTime;
                if (statusTimer <= 0)
                {
                    statusText.text = string.Empty;
                }
            }

            if (resetSaveArmed)
            {
                resetConfirmTimer -= Time.deltaTime;
                if (resetConfirmTimer <= 0)
                {
                    resetSaveArmed = false;
                    RefreshSettings();
                }
            }

            if (prestigeArmed)
            {
                prestigeConfirmTimer -= Time.deltaTime;
                if (prestigeConfirmTimer <= 0)
                {
                    prestigeArmed = false;
                    RefreshPrestige();
                }
            }

            if (saveTimer >= 10)
            {
                saveTimer = 0;
                ChaiSaveRepository.Save(game.State);
            }

            if (refreshTimer >= 0.2f)
            {
                refreshTimer = 0;
                RefreshAll();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && game != null)
            {
                ChaiSaveRepository.Save(game.State);
            }
        }

        private void OnApplicationQuit()
        {
            if (game != null)
            {
                ChaiSaveRepository.Save(game.State);
            }
        }

        private void BuildUi()
        {
            EnsureEventSystem();
            ResetGeneratedUiReferences();

            Canvas canvas = CreateCanvas(transform);
            audioSource = canvas.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;

            GameObject safeAreaRoot = CreateChild(SafeAreaRootName, canvas.transform);
            StretchToParent(safeAreaRoot.GetComponent<RectTransform>());

            GameObject scrollObject = CreateChild(MainScrollViewName, safeAreaRoot.transform);
            RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(26, 26);
            scrollRect.offsetMax = new Vector2(-26, -26);

            Image scrollImage = scrollObject.AddComponent<Image>();
            scrollImage.color = new Color(1, 1, 1, 0);

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 32;

            GameObject viewport = CreateChild(ViewportName, scrollObject.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            GameObject contentObject = CreateChild(ContentColumnName, viewport.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLayout = contentObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(0, 0, 0, 36);
            rootLayout.spacing = 18;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            contentObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            BuildHeader(contentObject.transform);
            BuildStallArt(contentObject.transform);
            BuildActions(contentObject.transform);
            BuildStats(contentObject.transform);
            BuildTutorial(contentObject.transform);
            BuildUpgradeList(contentObject.transform);
            BuildLocationList(contentObject.transform);
            BuildEventPanel(contentObject.transform);
            BuildMonetizationPanel(contentObject.transform);
            BuildPrestige(contentObject.transform);
            BuildProductionPanel(contentObject.transform);
            BuildSettings(contentObject.transform);

            GameObject modalLayer = CreateChild(ModalLayerName, safeAreaRoot.transform);
            StretchToParent(modalLayer.GetComponent<RectTransform>());
            modalLayer.transform.SetAsLastSibling();
            BuildOfflineRewardModal(modalLayer.transform);
            BuildUpgradeDetailModal(modalLayer.transform);
        }

        public void RebuildPersistentPreview()
        {
            content = ChaiContent.CreateDefault();
            game = ChaiGame.NewGame(content);
            ClearGeneratedUi();
            BuildUi();
            RefreshAll();
            Canvas.ForceUpdateCanvases();
        }

        private void ResetGeneratedUiReferences()
        {
            upgradeRows.Clear();
            locationRows.Clear();
            prestigeSkillRows.Clear();
            steamWisps.Clear();
            floatingRewards.Clear();
            selectedUpgrade = null;
            feedbackLayer = null;
            tapFeedbackTimer = 0;
        }

        private void ClearGeneratedUi()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name != CanvasName)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(CanvasName);
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            Image background = canvasObject.AddComponent<Image>();
            background.color = Background;

            RectTransform rect = canvasObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                StretchToParent(rect);
            }

            return canvas;
        }

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, new Color(0.06f, 0.31f, 0.34f), 180);
            AddShadow(header, new Vector2(0, -6), 0.32f);
            AddOutline(header, new Color(1f, 0.86f, 0.36f), new Vector2(3, 3));

            CreateArtShape("Header Sunburst", header.transform, new Vector2(-350, 14), new Vector2(420, 180), new Color(1f, 0.72f, 0.18f, 0.28f), GetCircleSprite(), 0);
            CreateArtShape("Header Tea Steam A", header.transform, new Vector2(430, 34), new Vector2(24, 94), new Color(1f, 0.96f, 0.76f, 0.34f), GetCircleSprite(), 0);
            CreateArtShape("Header Tea Steam B", header.transform, new Vector2(474, 18), new Vector2(20, 78), new Color(1f, 0.96f, 0.76f, 0.26f), GetCircleSprite(), 0);

            GameObject sign = CreateChild("Stall Signboard Header", header.transform);
            RectTransform signRect = sign.GetComponent<RectTransform>();
            signRect.anchorMin = new Vector2(0, 0.5f);
            signRect.anchorMax = new Vector2(1, 0.5f);
            signRect.offsetMin = new Vector2(28, -54);
            signRect.offsetMax = new Vector2(-310, 54);
            Image signImage = sign.AddComponent<Image>();
            signImage.color = new Color(1f, 0.70f, 0.20f);
            AddOutline(sign, new Color(0.38f, 0.15f, 0.07f), new Vector2(4, 4));

            Text title = CreateText("Title", sign.transform, "Chai Empire", 54, Ink, TextAnchor.MiddleLeft);
            SetStretchRect(title.GetComponent<RectTransform>(), 24, 42, -24, -8);
            title.fontStyle = FontStyle.Bold;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 32;

            locationText = CreateText("Location", sign.transform, string.Empty, 25, new Color(0.44f, 0.24f, 0.10f), TextAnchor.MiddleLeft);
            SetStretchRect(locationText.GetComponent<RectTransform>(), 26, 8, -24, -54);

            GameObject demand = CreateChild("Demand Badge", header.transform);
            RectTransform demandRect = demand.GetComponent<RectTransform>();
            demandRect.anchorMin = new Vector2(1, 0.5f);
            demandRect.anchorMax = new Vector2(1, 0.5f);
            demandRect.pivot = new Vector2(1, 0.5f);
            demandRect.anchoredPosition = new Vector2(-26, 0);
            demandRect.sizeDelta = new Vector2(258, 116);
            Image demandImage = demand.AddComponent<Image>();
            demandImage.color = new Color(0.91f, 0.19f, 0.21f);
            AddShadow(demand, new Vector2(0, -5), 0.3f);
            AddOutline(demand, Color.white, new Vector2(3, 3));

            demandPillText = CreateText("Demand Multiplier", demand.transform, string.Empty, 31, Color.white, TextAnchor.MiddleCenter);
            demandPillText.fontStyle = FontStyle.Bold;
            StretchToParent(demandPillText.GetComponent<RectTransform>());

            CreateArtShape("Header Cup Icon", header.transform, new Vector2(434, -58), new Vector2(72, 46), new Color(0.92f, 0.36f, 0.12f), GetCircleSprite(), 0);
            CreateArtShape("Header Cup Foam", header.transform, new Vector2(434, -38), new Vector2(58, 18), Cream, GetCircleSprite(), 0);
        }

        private void BuildStallArt(Transform parent)
        {
            Color backdrop = new Color(0.98f, 0.77f, 0.38f);
            Color counter = new Color(0.36f, 0.18f, 0.10f);
            Color stove = new Color(0.16f, 0.18f, 0.18f);
            Color stoveFace = new Color(0.28f, 0.30f, 0.28f);
            Color brass = new Color(0.90f, 0.58f, 0.22f);
            Color kettle = new Color(0.78f, 0.86f, 0.83f);
            Color kettleShade = new Color(0.12f, 0.42f, 0.42f);

            steamWisps.Clear();

            GameObject art = CreatePanel("Stall Art", parent, backdrop, 620);
            AddShadow(art, new Vector2(0, -8), 0.28f);
            stallArtBackground = art.GetComponent<Image>();
            stallArtGlow = CreateArtShape("Back Wall Glow", art.transform, new Vector2(-60, 72), new Vector2(930, 430), new Color(1f, 0.93f, 0.55f, 0.78f), GetCircleSprite(), 0);
            CreateArtShape("Street Sky Band", art.transform, new Vector2(0, 246), new Vector2(1020, 168), new Color(0.38f, 0.78f, 0.90f, 0.55f), null, 0);
            CreateArtShape("Street Ground Band", art.transform, new Vector2(0, -264), new Vector2(1040, 150), new Color(0.37f, 0.22f, 0.13f, 0.45f), null, 0);
            CreateArtShape("Awning Shadow", art.transform, new Vector2(0, 194), new Vector2(940, 76), new Color(0.23f, 0.11f, 0.08f, 0.24f), null, 0);
            CreateArtShape("Awning Stripe Saffron", art.transform, new Vector2(-300, 212), new Vector2(172, 108), Saffron, null, 0);
            CreateArtShape("Awning Stripe Cream", art.transform, new Vector2(-120, 212), new Vector2(172, 108), Cream, null, 0);
            CreateArtShape("Awning Stripe Teal", art.transform, new Vector2(60, 212), new Vector2(172, 108), Teal, null, 0);
            CreateArtShape("Awning Stripe Gold", art.transform, new Vector2(240, 212), new Vector2(172, 108), Gold, null, 0);
            CreateArtShape("Awning Front Lip", art.transform, new Vector2(0, 152), new Vector2(940, 34), new Color(0.35f, 0.16f, 0.08f), null, 0);

            GameObject sign = CreateChild("Painted Stall Signboard", art.transform);
            RectTransform signRect = sign.GetComponent<RectTransform>();
            signRect.anchorMin = new Vector2(0.5f, 0.5f);
            signRect.anchorMax = new Vector2(0.5f, 0.5f);
            signRect.pivot = new Vector2(0.5f, 0.5f);
            signRect.anchoredPosition = new Vector2(0, 92);
            signRect.sizeDelta = new Vector2(420, 78);
            Image signImage = sign.AddComponent<Image>();
            signImage.color = new Color(0.98f, 0.60f, 0.16f);
            AddShadow(sign, new Vector2(0, -4), 0.25f);
            AddOutline(sign, new Color(0.36f, 0.15f, 0.06f), new Vector2(4, 4));
            stallSignText = CreateText("Stall Signboard Text", sign.transform, "GALI TAPRI", 32, Color.white, TextAnchor.MiddleCenter);
            stallSignText.fontStyle = FontStyle.Bold;
            StretchToParent(stallSignText.GetComponent<RectTransform>());

            CreateStringLights(art.transform);

            stallCounterTop = CreateArtShape("Counter Top", art.transform, new Vector2(0, -188), new Vector2(980, 82), counter, null, 0);
            stallCounterFront = CreateArtShape("Counter Front", art.transform, new Vector2(0, -248), new Vector2(936, 96), new Color(0.50f, 0.25f, 0.13f), null, 0);
            CreateArtShape("Counter Highlight", art.transform, new Vector2(-6, -157), new Vector2(910, 18), new Color(1f, 0.70f, 0.34f, 0.55f), null, 0);
            CreateCustomerQueue(art.transform);
            CreateUpiQrProp(art.transform);
            CreateChaiCupTray(art.transform);

            CreateArtShape("Stove Shadow", art.transform, new Vector2(0, -98), new Vector2(440, 66), new Color(0.09f, 0.10f, 0.10f, 0.36f), GetCircleSprite(), 0);
            CreateArtShape("Stove Base", art.transform, new Vector2(0, -80), new Vector2(430, 138), stove, null, 0);
            CreateArtShape("Stove Front", art.transform, new Vector2(0, -112), new Vector2(338, 58), stoveFace, null, 0);
            CreateArtShape("Burner Ring", art.transform, new Vector2(0, -8), new Vector2(258, 56), new Color(0.07f, 0.08f, 0.08f), GetCircleSprite(), 0);
            CreateArtShape("Flame Outer", art.transform, new Vector2(-10, 28), new Vector2(146, 104), Saffron, GetCircleSprite(), 0);
            CreateArtShape("Flame Inner", art.transform, new Vector2(12, 30), new Vector2(84, 70), Gold, GetCircleSprite(), 0);

            tapPulseImage = CreateArtShape("Tap Feedback Pulse", art.transform, new Vector2(0, 48), new Vector2(430, 278), new Color(1f, 0.93f, 0.20f, 0f), GetCircleSprite(), 0);
            tapPulseRect = tapPulseImage.GetComponent<RectTransform>();

            CreateArtShape("Kettle Handle Outer", art.transform, new Vector2(-258, 104), new Vector2(204, 204), kettleShade, GetCircleSprite(), 0);
            CreateArtShape("Kettle Handle Hole", art.transform, new Vector2(-258, 104), new Vector2(126, 126), backdrop, GetCircleSprite(), 0);
            CreateArtShape("Kettle Spout", art.transform, new Vector2(262, 106), new Vector2(210, 52), kettleShade, null, -12);
            CreateArtShape("Kettle Spout Tip", art.transform, new Vector2(368, 126), new Vector2(74, 40), kettle, GetCircleSprite(), 0);
            CreateArtShape("Kettle Body", art.transform, new Vector2(0, 68), new Vector2(410, 224), kettle, GetCircleSprite(), 0);
            CreateArtShape("Kettle Belly", art.transform, new Vector2(0, 52), new Vector2(306, 148), kettleShade, GetCircleSprite(), 0);
            CreateArtShape("Kettle Highlight", art.transform, new Vector2(-88, 106), new Vector2(96, 50), new Color(1f, 0.96f, 0.82f, 0.78f), GetCircleSprite(), 0);
            CreateArtShape("Kettle Lid", art.transform, new Vector2(0, 182), new Vector2(214, 52), brass, GetCircleSprite(), 0);
            CreateArtShape("Kettle Knob", art.transform, new Vector2(0, 218), new Vector2(56, 40), counter, GetCircleSprite(), 0);
            CreateSteamWisp("Steam Wisp A", art.transform, new Vector2(-68, 268), new Vector2(40, 96), 0f);
            CreateSteamWisp("Steam Wisp B", art.transform, new Vector2(2, 284), new Vector2(34, 110), 0.85f);
            CreateSteamWisp("Steam Wisp C", art.transform, new Vector2(76, 264), new Vector2(38, 88), 1.7f);

            rushBurstImage = CreateArtShape("Rush Hour Burst Overlay", art.transform, new Vector2(0, 40), new Vector2(900, 560), new Color(1f, 0.24f, 0.08f, 0f), GetCircleSprite(), 0);
            rushBurstImage.raycastTarget = false;

            feedbackLayer = CreateChild("Floating Feedback Layer", art.transform).GetComponent<RectTransform>();
            feedbackLayer.anchorMin = Vector2.zero;
            feedbackLayer.anchorMax = Vector2.one;
            feedbackLayer.offsetMin = Vector2.zero;
            feedbackLayer.offsetMax = Vector2.zero;
            CreateLocationBackdropVariants(art.transform);
        }

        private void CreateLocationBackdropVariants(Transform parent)
        {
            GameObject variants = CreateChild("Location Backdrop Variants", parent);
            RectTransform rect = variants.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-22, -18);
            rect.sizeDelta = new Vector2(330, 44);

            HorizontalLayoutGroup layout = variants.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            CreatePaletteSwatch(variants.transform, "Gali Tapri");
            CreatePaletteSwatch(variants.transform, "Bus Stand");
            CreatePaletteSwatch(variants.transform, "Railway Platform");
            CreatePaletteSwatch(variants.transform, "College Canteen");
            CreatePaletteSwatch(variants.transform, "IT Park");
            CreatePaletteSwatch(variants.transform, "Airport Lounge");
        }

        private void CreatePaletteSwatch(Transform parent, string displayName)
        {
            string locationId = displayName.ToLowerInvariant().Replace(" ", "-");
            LocationVisualPalette palette = GetLocationVisualPalette(locationId);
            Image swatch = CreateArtShape("Backdrop Variant - " + displayName, parent, Vector2.zero, new Vector2(44, 44), palette.Backdrop, null, 0);
            swatch.raycastTarget = false;
        }

        private void CreateStringLights(Transform parent)
        {
            Color wire = new Color(0.24f, 0.13f, 0.07f, 0.75f);
            CreateArtShape("String Light Wire", parent, new Vector2(0, 138), new Vector2(820, 8), wire, null, 0);
            Color[] colors =
            {
                Gold,
                Saffron,
                new Color(0.18f, 0.70f, 0.86f),
                Leaf,
                Rose,
                Cream
            };

            for (int i = 0; i < 10; i++)
            {
                float x = -360 + (i * 80);
                CreateArtShape("String Light Bulb " + (i + 1).ToString("00"), parent, new Vector2(x, 122), new Vector2(28, 38), colors[i % colors.Length], GetCircleSprite(), 0);
            }
        }

        private void CreateChaiCupTray(Transform parent)
        {
            CreateArtShape("Chai Cup Tray Shadow", parent, new Vector2(334, -184), new Vector2(248, 34), new Color(0.12f, 0.07f, 0.04f, 0.26f), GetCircleSprite(), 0);
            CreateChaiCup(parent, "Chai Cup A", new Vector2(240, -132), Saffron);
            CreateChaiCup(parent, "Chai Cup B", new Vector2(318, -128), Gold);
            CreateChaiCup(parent, "Chai Cup C", new Vector2(396, -138), Rose);
        }

        private void CreateChaiCup(Transform parent, string prefix, Vector2 position, Color sleeve)
        {
            CreateArtShape(prefix + " Steam", parent, position + new Vector2(0, 54), new Vector2(18, 48), new Color(1f, 0.96f, 0.78f, 0.3f), GetCircleSprite(), 0);
            CreateArtShape(prefix + " Rim", parent, position + new Vector2(0, 24), new Vector2(62, 26), new Color(0.32f, 0.14f, 0.07f), GetCircleSprite(), 0);
            CreateArtShape(prefix + " Tea", parent, position + new Vector2(0, 28), new Vector2(48, 14), new Color(0.78f, 0.34f, 0.10f), GetCircleSprite(), 0);
            CreateArtShape(prefix + " Body", parent, position + new Vector2(0, -8), new Vector2(54, 78), sleeve, GetCircleSprite(), 0);
        }

        private void CreateCustomerQueue(Transform parent)
        {
            CreateArtShape("Customer Queue Shadow", parent, new Vector2(360, -214), new Vector2(292, 34), new Color(0.16f, 0.08f, 0.04f, 0.24f), GetCircleSprite(), 0);
            CreateCustomer("Queue Customer A", parent, new Vector2(286, -142), new Color(0.18f, 0.50f, 0.31f), 0.96f);
            CreateCustomer("Queue Customer B", parent, new Vector2(366, -132), Saffron, 1.04f);
            CreateCustomer("Queue Customer C", parent, new Vector2(446, -152), Rose, 0.88f);
        }

        private void CreateCustomer(string prefix, Transform parent, Vector2 basePosition, Color shirt, float scale)
        {
            Color skin = new Color(0.67f, 0.42f, 0.25f);
            Color hair = new Color(0.10f, 0.07f, 0.05f);
            CreateArtShape(prefix + " Body", parent, basePosition + new Vector2(0, -38 * scale), new Vector2(58 * scale, 88 * scale), shirt, GetCircleSprite(), 0);
            CreateArtShape(prefix + " Head", parent, basePosition + new Vector2(0, 20 * scale), new Vector2(46 * scale, 46 * scale), skin, GetCircleSprite(), 0);
            CreateArtShape(prefix + " Hair", parent, basePosition + new Vector2(0, 36 * scale), new Vector2(50 * scale, 22 * scale), hair, GetCircleSprite(), 0);
        }

        private void CreateUpiQrProp(Transform parent)
        {
            Vector2 origin = new Vector2(-388, -132);
            Color paper = new Color(0.97f, 0.95f, 0.88f);
            Color ink = new Color(0.06f, 0.08f, 0.08f);

            CreateArtShape("UPI QR Prop Backplate", parent, origin, new Vector2(124, 148), paper, null, 0);
            CreateArtShape("UPI QR Prop Header", parent, origin + new Vector2(0, 54), new Vector2(104, 24), Teal, null, 0);
            CreateArtShape("UPI QR Prop Code Field", parent, origin + new Vector2(0, -10), new Vector2(88, 88), Color.white, null, 0);
            CreateQrFinder("UPI QR Finder A", parent, origin + new Vector2(-28, 18), ink);
            CreateQrFinder("UPI QR Finder B", parent, origin + new Vector2(28, 18), ink);
            CreateQrFinder("UPI QR Finder C", parent, origin + new Vector2(-28, -38), ink);

            CreateQrDot("UPI QR Dot 01", parent, origin, -6, 8, ink);
            CreateQrDot("UPI QR Dot 02", parent, origin, 10, 8, ink);
            CreateQrDot("UPI QR Dot 03", parent, origin, 26, -6, ink);
            CreateQrDot("UPI QR Dot 04", parent, origin, -8, -12, ink);
            CreateQrDot("UPI QR Dot 05", parent, origin, 12, -26, ink);
            CreateQrDot("UPI QR Dot 06", parent, origin, 32, -34, ink);
        }

        private void CreateQrFinder(string prefix, Transform parent, Vector2 center, Color ink)
        {
            CreateArtShape(prefix + " Outer", parent, center, new Vector2(24, 24), ink, null, 0);
            CreateArtShape(prefix + " Inner", parent, center, new Vector2(12, 12), Color.white, null, 0);
            CreateArtShape(prefix + " Core", parent, center, new Vector2(6, 6), ink, null, 0);
        }

        private void CreateQrDot(string name, Transform parent, Vector2 origin, float x, float y, Color ink)
        {
            CreateArtShape(name, parent, origin + new Vector2(x, y), new Vector2(8, 8), ink, null, 0);
        }

        private void BuildStats(Transform parent)
        {
            GameObject stats = CreatePanel("Stats", parent, new Color(0.12f, 0.22f, 0.19f), 280);
            AddShadow(stats, new Vector2(0, -6), 0.25f);

            GridLayoutGroup grid = stats.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.spacing = new Vector2(14, 14);
            grid.cellSize = new Vector2(486, 112);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            rupeesText = CreateStatCard(stats.transform, "Rupees", "Rs", Gold, 44);
            rateText = CreateStatCard(stats.transform, "Per Second", "/s", Leaf, 28);
            tapText = CreateStatCard(stats.transform, "Tap Value", "TAP", Saffron, 28);
            servedText = CreateStatCard(stats.transform, "Chai Served", "CUP", Teal, 28);
        }

        private Text CreateStatCard(Transform parent, string title, string icon, Color accent, int valueSize)
        {
            GameObject card = CreateChild(title, parent);
            Image image = card.AddComponent<Image>();
            image.color = Card;
            AddOutline(card, new Color(0.94f, 0.65f, 0.20f), new Vector2(2, 2));
            AddShadow(card, new Vector2(0, -4), 0.16f);

            CreateGameIcon(card.transform, title + " Icon", icon, accent, new Vector2(58, 0), new Vector2(72, 72), 24);

            Text label = CreateText(title + " Label", card.transform, title.ToUpperInvariant(), 20, MutedInk, TextAnchor.MiddleLeft);
            SetStretchRect(label.GetComponent<RectTransform>(), 112, 72, -18, -12);
            label.fontStyle = FontStyle.Bold;

            Text value = CreateText(title + " Value", card.transform, string.Empty, valueSize, Ink, TextAnchor.MiddleLeft);
            SetStretchRect(value.GetComponent<RectTransform>(), 112, 12, -18, -54);
            value.fontStyle = FontStyle.Bold;
            value.resizeTextForBestFit = true;
            value.resizeTextMinSize = 21;
            return value;
        }

        private void BuildTutorial(Transform parent)
        {
            tutorialPanel = CreatePanel("Tutorial", parent, new Color(0.98f, 0.81f, 0.30f), 220);
            AddShadow(tutorialPanel, new Vector2(0, -6), 0.24f);
            AddOutline(tutorialPanel, Color.white, new Vector2(3, 3));

            CreateGameIcon(tutorialPanel.transform, "Tutorial Mission Icon", "GO", Rose, new Vector2(72, 0), new Vector2(92, 92), 24);

            tutorialTitleText = CreateText("Tutorial Title", tutorialPanel.transform, string.Empty, 30, Ink, TextAnchor.MiddleLeft);
            SetStretchRect(tutorialTitleText.GetComponent<RectTransform>(), 140, 148, -30, -20);
            tutorialTitleText.fontStyle = FontStyle.Bold;
            tutorialBodyText = CreateText("Tutorial Body", tutorialPanel.transform, string.Empty, 24, Ink, TextAnchor.MiddleLeft);
            SetStretchRect(tutorialBodyText.GetComponent<RectTransform>(), 140, 84, -30, -86);
            tutorialProgressText = CreateText("Tutorial Progress", tutorialPanel.transform, string.Empty, 22, Rose, TextAnchor.MiddleLeft);
            SetStretchRect(tutorialProgressText.GetComponent<RectTransform>(), 140, 32, -326, -144);
            tutorialProgressText.fontStyle = FontStyle.Bold;

            tutorialPrimaryButton = CreateButton("Tutorial Primary", tutorialPanel.transform, string.Empty, 24, Saffron, 74);
            RectTransform buttonRect = tutorialPrimaryButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 0);
            buttonRect.anchorMax = new Vector2(1, 0);
            buttonRect.pivot = new Vector2(1, 0);
            buttonRect.anchoredPosition = new Vector2(-28, 24);
            buttonRect.sizeDelta = new Vector2(284, 74);
            AddShadow(tutorialPrimaryButton.gameObject, new Vector2(0, -4), 0.22f);
            tutorialPrimaryLabel = tutorialPrimaryButton.GetComponentInChildren<Text>();
            tutorialPrimaryButton.onClick.AddListener(HandleTutorialPrimary);
        }

        private void BuildOfflineRewardModal(Transform parent)
        {
            offlineRewardModal = CreateChild("Offline Reward Modal", parent);
            RectTransform modalRect = offlineRewardModal.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image scrim = offlineRewardModal.AddComponent<Image>();
            scrim.color = new Color(0.03f, 0.02f, 0.01f, 0.68f);

            GameObject card = CreateChild("Offline Reward Card", offlineRewardModal.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(880, 850);

            Image cardImage = card.AddComponent<Image>();
            cardImage.color = new Color(1f, 0.90f, 0.55f);
            AddShadow(card, new Vector2(0, -10), 0.4f);
            AddOutline(card, Color.white, new Vector2(4, 4));

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(46, 46, 34, 42);
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("Offline Reward Title", card.transform, "Welcome Back, Boss", 42, Rose, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            GameObject art = CreatePanel("Offline Reward Art", card.transform, new Color(1f, 0.78f, 0.25f), 150);
            CreateGameIcon(art.transform, "Offline Thermos Icon", "Rs", Gold, new Vector2(0, 0), new Vector2(118, 118), 34);
            CreateArtShape("Offline Steam A", art.transform, new Vector2(-94, 22), new Vector2(24, 84), new Color(1f, 0.96f, 0.82f, 0.52f), GetCircleSprite(), 0);
            CreateArtShape("Offline Steam B", art.transform, new Vector2(94, 24), new Vector2(22, 76), new Color(1f, 0.96f, 0.82f, 0.42f), GetCircleSprite(), 0);
            offlineRewardAmountText = CreateText("Offline Reward Amount", card.transform, string.Empty, 54, Ink, TextAnchor.MiddleCenter);
            offlineRewardAmountText.fontStyle = FontStyle.Bold;
            offlineRewardDetailText = CreateText("Offline Reward Detail", card.transform, string.Empty, 28, Ink, TextAnchor.MiddleCenter);
            offlineRewardCapText = CreateText("Offline Reward Cap", card.transform, string.Empty, 24, Rose, TextAnchor.MiddleCenter);

            offlineSponsorButton = CreateButton("Sponsor Offline Bonus", card.transform, "Optional Sponsor x2", 28, Leaf, 78);
            offlineSponsorLabel = offlineSponsorButton.GetComponentInChildren<Text>();
            offlineSponsorButton.onClick.AddListener(HandleOfflineSponsorBonus);

            Button claim = CreateButton("Claim Offline Reward", card.transform, "Claim", 30, Saffron, 82);
            claim.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                HideOfflineReward();
            });

            offlineRewardModal.SetActive(false);
        }

        private void BuildUpgradeDetailModal(Transform parent)
        {
            upgradeDetailModal = CreateChild("Upgrade Detail Modal", parent);
            RectTransform modalRect = upgradeDetailModal.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image scrim = upgradeDetailModal.AddComponent<Image>();
            scrim.color = new Color(0.03f, 0.02f, 0.01f, 0.66f);

            GameObject card = CreateChild("Upgrade Detail Card", upgradeDetailModal.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(900, 860);
            Image cardImage = card.AddComponent<Image>();
            cardImage.color = Card;
            AddShadow(card, new Vector2(0, -10), 0.38f);
            AddOutline(card, Gold, new Vector2(4, 4));

            CreateGameIcon(card.transform, "Upgrade Detail Icon", "UP", Saffron, new Vector2(80, 314), new Vector2(104, 104), 30);

            upgradeDetailTitleText = CreateText("Upgrade Detail Title", card.transform, string.Empty, 40, Ink, TextAnchor.MiddleLeft);
            upgradeDetailTitleText.fontStyle = FontStyle.Bold;
            SetStretchRect(upgradeDetailTitleText.GetComponent<RectTransform>(), 154, 676, -86, -42);

            Button close = CreateButton("Close Upgrade Detail", card.transform, "X", 34, Rose, 66);
            RectTransform closeRect = close.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-28, -28);
            closeRect.sizeDelta = new Vector2(70, 66);
            close.onClick.AddListener(HideUpgradeDetail);

            upgradeDetailCategoryText = CreateDetailLine(card.transform, "Upgrade Detail Category", "Category", 560);
            upgradeDetailLevelText = CreateDetailLine(card.transform, "Upgrade Detail Level", "Level", 460);
            upgradeDetailEffectText = CreateDetailLine(card.transform, "Upgrade Detail Effect", "Effect", 360);
            upgradeDetailCostText = CreateDetailLine(card.transform, "Upgrade Detail Cost", "Cost", 260);

            upgradeDetailDescriptionText = CreateText("Upgrade Detail Description", card.transform, string.Empty, 25, MutedInk, TextAnchor.MiddleLeft);
            SetStretchRect(upgradeDetailDescriptionText.GetComponent<RectTransform>(), 50, 108, -50, -676);

            upgradeDetailBuyButton = CreateButton("Buy Upgrade", card.transform, "Buy Upgrade", 30, Saffron, 90);
            RectTransform buyRect = upgradeDetailBuyButton.GetComponent<RectTransform>();
            buyRect.anchorMin = new Vector2(0, 0);
            buyRect.anchorMax = new Vector2(1, 0);
            buyRect.offsetMin = new Vector2(50, 42);
            buyRect.offsetMax = new Vector2(-50, 132);
            AddShadow(upgradeDetailBuyButton.gameObject, new Vector2(0, -5), 0.25f);
            upgradeDetailBuyLabel = upgradeDetailBuyButton.GetComponentInChildren<Text>();
            upgradeDetailBuyButton.onClick.AddListener(HandleUpgradeDetailBuy);

            upgradeDetailModal.SetActive(false);
        }

        private Text CreateDetailLine(Transform parent, string objectName, string label, float y)
        {
            GameObject row = CreatePanel(objectName + " Row", parent, new Color(1f, 0.96f, 0.84f), 72);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.offsetMin = new Vector2(50, y - 72);
            rowRect.offsetMax = new Vector2(-50, y);
            Text left = CreateText(objectName + " Label", row.transform, label, 23, MutedInk, TextAnchor.MiddleLeft);
            SetStretchRect(left.GetComponent<RectTransform>(), 22, 0, -450, 0);
            left.fontStyle = FontStyle.Bold;

            Text value = CreateText(objectName, row.transform, string.Empty, 24, Ink, TextAnchor.MiddleRight);
            SetStretchRect(value.GetComponent<RectTransform>(), 260, 0, -22, 0);
            value.fontStyle = FontStyle.Bold;
            return value;
        }

        private void BuildActions(Transform parent)
        {
            GameObject actions = CreatePanel("Actions", parent, new Color(0.08f, 0.31f, 0.31f), 330);
            AddShadow(actions, new Vector2(0, -7), 0.3f);

            Text title = CreateText("Action Dock Title", actions.transform, "Brew faster, serve hotter", 28, Cream, TextAnchor.MiddleLeft);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.offsetMin = new Vector2(28, 302);
            titleRect.offsetMax = new Vector2(-28, -18);
            title.fontStyle = FontStyle.Bold;

            Button kettle = CreateButton("Brew Kettle", actions.transform, "TAP KETTLE", 50, Saffron, 208);
            kettleButtonRect = kettle.GetComponent<RectTransform>();
            kettleButtonRect.anchorMin = new Vector2(0, 0);
            kettleButtonRect.anchorMax = new Vector2(1, 0);
            kettleButtonRect.pivot = new Vector2(0.5f, 0);
            kettleButtonRect.offsetMin = new Vector2(26, 34);
            kettleButtonRect.offsetMax = new Vector2(-376, 242);
            AddShadow(kettle.gameObject, new Vector2(0, -8), 0.34f);
            AddOutline(kettle.gameObject, new Color(1f, 0.91f, 0.45f), new Vector2(4, 4));
            CreateGameIcon(kettle.transform, "Tap Kettle Icon", "TAP", Gold, new Vector2(92, 0), new Vector2(116, 116), 30);
            kettleButtonLabel = kettle.GetComponentInChildren<Text>();
            RectTransform kettleLabelRect = kettleButtonLabel.GetComponent<RectTransform>();
            kettleLabelRect.offsetMin = new Vector2(166, 24);
            kettleLabelRect.offsetMax = new Vector2(-24, -24);
            kettleButtonLabel.alignment = TextAnchor.MiddleLeft;
            kettle.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                game.TapKettle();
                PulseTapButton();
                SpawnFloatingReward("+" + ChaiNumberFormatter.Rupees(game.GetTapValue()), Gold, new Vector2(0, 120));
                SetStatus("Fresh cutting chai");
                RefreshAll();
            });

            Button customers = CreateButton("Customer Queue", actions.transform, "Serve Queue", 28, Leaf, 96);
            RectTransform customersRect = customers.GetComponent<RectTransform>();
            customersRect.anchorMin = new Vector2(1, 0);
            customersRect.anchorMax = new Vector2(1, 0);
            customersRect.pivot = new Vector2(1, 0);
            customersRect.anchoredPosition = new Vector2(-26, 146);
            customersRect.sizeDelta = new Vector2(318, 96);
            AddCardIcon(customers, "CUP", Cream);
            customers.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                game.TapCustomerQueue();
                SpawnFloatingReward("Queue +" + ChaiNumberFormatter.Rupees(game.GetTapValue() * 3), Cream, new Vector2(210, 72));
                SetStatus("Queue served");
                RefreshAll();
            });

            rushButton = CreateButton("Rush Hour", actions.transform, "Rush Hour", 28, Rose, 96);
            RectTransform rushRect = rushButton.GetComponent<RectTransform>();
            rushRect.anchorMin = new Vector2(1, 0);
            rushRect.anchorMax = new Vector2(1, 0);
            rushRect.pivot = new Vector2(1, 0);
            rushRect.anchoredPosition = new Vector2(-26, 34);
            rushRect.sizeDelta = new Vector2(318, 96);
            AddCardIcon(rushButton, "2X", Gold);
            rushButton.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                if (game.TryTriggerRushHour())
                {
                    TriggerHaptic();
                    SpawnFloatingReward("RUSH x2", Saffron, new Vector2(0, 190));
                    SetStatus("Rush hour: 2x for 20 sec");
                    RefreshAll();
                }
            });

            rushBanner = CreateChild("Rush Banner", actions.transform);
            RectTransform bannerRect = rushBanner.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 0);
            bannerRect.anchorMax = new Vector2(1, 0);
            bannerRect.offsetMin = new Vector2(26, 252);
            bannerRect.offsetMax = new Vector2(-26, 302);
            Image bannerImage = rushBanner.AddComponent<Image>();
            bannerImage.color = new Color(1f, 0.78f, 0.13f, 0.92f);
            AddOutline(rushBanner, new Color(1f, 0.95f, 0.68f), new Vector2(2, 2));

            rushText = CreateText("Rush Status", rushBanner.transform, string.Empty, 23, Ink, TextAnchor.MiddleCenter);
            rushText.fontStyle = FontStyle.Bold;
            StretchToParent(rushText.GetComponent<RectTransform>());

            statusText = CreateText("Status Toast", actions.transform, string.Empty, 24, Cream, TextAnchor.MiddleCenter);
            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0);
            statusRect.offsetMin = new Vector2(30, 0);
            statusRect.offsetMax = new Vector2(-30, 34);
            statusText.fontStyle = FontStyle.Bold;
        }

        private void BuildUpgradeList(Transform parent)
        {
            GameObject section = CreatePanel("Upgrades", parent, new Color(0.98f, 0.88f, 0.55f), 0);
            AddShadow(section, new Vector2(0, -6), 0.24f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 18, 22);
            layout.spacing = 12;

            Text title = CreateText("Upgrades Title", section.transform, "Upgrade Bazaar", 36, Ink, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

            foreach (UpgradeDefinition upgrade in content.Upgrades)
            {
                UpgradeDefinition captured = upgrade;
                Button button = CreateButton(upgrade.Id, section.transform, upgrade.DisplayName, 22, Teal, 132);
                AddShadow(button.gameObject, new Vector2(0, -4), 0.18f);
                AddOutline(button.gameObject, Color.white, new Vector2(2, 2));
                AddCardIcon(button, GetUpgradeIcon(upgrade), GetUpgradeAccent(upgrade));
                Text label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() =>
                {
                    PlayButtonPressSound();
                    ShowUpgradeDetail(captured);
                });
                upgradeRows.Add(new UpgradeRow(captured, button, label));
            }
        }

        private void BuildLocationList(Transform parent)
        {
            GameObject section = CreatePanel("Locations", parent, new Color(0.63f, 0.87f, 0.88f), 0);
            AddShadow(section, new Vector2(0, -6), 0.24f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 18, 22);
            layout.spacing = 12;

            Text title = CreateText("Locations Title", section.transform, "Street Map", 36, Ink, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

            foreach (LocationDefinition location in content.Locations)
            {
                if (location.UnlockedByDefault)
                {
                    continue;
                }

                LocationDefinition captured = location;
                Button button = CreateButton(location.Id, section.transform, location.DisplayName, 22, Leaf, 124);
                AddShadow(button.gameObject, new Vector2(0, -4), 0.18f);
                AddOutline(button.gameObject, Color.white, new Vector2(2, 2));
                AddCardIcon(button, "MAP", GetLocationVisualPalette(location.Id).Glow);
                Text label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() =>
                {
                    PlayButtonPressSound();
                    if (game.TryUnlockLocation(captured.Id))
                    {
                        PlayUnlockSound();
                        TriggerHaptic();
                        SpawnFloatingReward("UNLOCKED", Gold, new Vector2(0, 170));
                        SetStatus(captured.DisplayName + " unlocked");
                        RefreshAll();
                        return;
                    }

                    SetStatus("Need " + ChaiNumberFormatter.Rupees(game.GetLocationUnlockCost(captured.Id)) + " for " + captured.DisplayName);
                });
                locationRows.Add(new LocationRow(captured, button, label));
            }
        }

        private void BuildPrestige(Transform parent)
        {
            GameObject section = CreatePanel("Prestige", parent, new Color(0.36f, 0.18f, 0.46f), 1540);
            AddShadow(section, new Vector2(0, -7), 0.32f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 24);
            layout.spacing = 10;

            Text title = CreateText("Prestige Title", section.transform, "Secret Masala Vault", 36, Cream, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

            GameObject previewCard = CreatePanel("Prestige Preview Card", section.transform, new Color(0.94f, 0.86f, 1f), 220);
            AddCardIconToTransform(previewCard.transform, "Masala Jar Icon", "M", Gold, new Vector2(62, 0), new Vector2(86, 86), 34);
            prestigeText = CreateText("Prestige Preview", previewCard.transform, string.Empty, 23, Ink, TextAnchor.MiddleLeft);
            RectTransform previewRect = prestigeText.GetComponent<RectTransform>();
            SetStretchRect(previewRect, 132, 20, -28, -20);

            prestigeButton = CreateButton("Prestige Button", section.transform, "Preserve Secret Masala", 25, Rose, 82);
            AddShadow(prestigeButton.gameObject, new Vector2(0, -4), 0.22f);
            prestigeButtonLabel = prestigeButton.GetComponentInChildren<Text>();
            prestigeButton.onClick.AddListener(HandlePrestige);

            legacyText = CreateText("Legacy", section.transform, string.Empty, 24, Cream, TextAnchor.MiddleLeft);
            legacyText.fontStyle = FontStyle.Bold;

            Text skillTitle = CreateText("Skill Tree Title", section.transform, "Masala Skill Cards", 30, Cream, TextAnchor.MiddleLeft);
            skillTitle.fontStyle = FontStyle.Bold;

            prestigeSkillRows.Clear();
            foreach (PrestigeSkillDefinition skill in ChaiPrestigeSkills.All)
            {
                PrestigeSkillDefinition captured = skill;
                Button button = CreateButton(skill.Id, section.transform, skill.DisplayName, 19, Teal, 112);
                AddCardIcon(button, "SK", Purple);
                Text label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => HandlePrestigeSkill(captured));
                prestigeSkillRows.Add(new PrestigeSkillRow(captured, button, label));
            }

            Text prestigeStatus = CreateText("Prestige Status", section.transform, "Prestige resets rupees, upgrades, and locations after the vault opens.", 22, Gold, TextAnchor.MiddleLeft);
            prestigeStatus.fontStyle = FontStyle.Bold;
        }

        private void BuildEventPanel(Transform parent)
        {
            GameObject section = CreatePanel("Events", parent, new Color(0.86f, 0.95f, 0.62f), 304);
            AddShadow(section, new Vector2(0, -6), 0.22f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 20);
            layout.spacing = 10;

            Text title = CreateText("Events Title", section.transform, "Live Street Events", 34, Leaf, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            CreateArtShape("Events Spark Icon", section.transform, new Vector2(450, 112), new Vector2(62, 62), Gold, GetCircleSprite(), 0);
            eventText = CreateText("Event Status", section.transform, string.Empty, 23, Ink, TextAnchor.MiddleLeft);
            eventButton = CreateButton("Event Button", section.transform, "Start Event", 24, Leaf, 78);
            AddShadow(eventButton.gameObject, new Vector2(0, -4), 0.18f);
            eventButtonLabel = eventButton.GetComponentInChildren<Text>();
            eventButton.onClick.AddListener(HandleStartEvent);
        }

        private void BuildMonetizationPanel(Transform parent)
        {
            GameObject section = CreatePanel("Optional Rewards", parent, new Color(0.98f, 0.78f, 0.70f), 570);
            AddShadow(section, new Vector2(0, -6), 0.22f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 8;

            Text title = CreateText("Optional Rewards Title", section.transform, "Boosts & Stall Style", 34, Rose, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            monetizationText = CreateText("Optional Rewards Status", section.transform, string.Empty, 23, Ink, TextAnchor.MiddleLeft);

            sponsorBoostButton = CreateButton("Sponsor Boost", section.transform, "Sponsor Boost", 23, Leaf, 70);
            AddCardIcon(sponsorBoostButton, "2X", Gold);
            sponsorBoostLabel = sponsorBoostButton.GetComponentInChildren<Text>();
            sponsorBoostButton.onClick.AddListener(HandleSponsorBoost);

            noAdsButton = CreateButton("No Ads", section.transform, "No Ads", 23, Teal, 66);
            AddCardIcon(noAdsButton, "AD", Teal);
            noAdsLabel = noAdsButton.GetComponentInChildren<Text>();
            noAdsButton.onClick.AddListener(HandleNoAdsPurchase);

            stallThemeButton = CreateButton("Stall Theme", section.transform, "Theme", 22, Saffron, 64);
            AddCardIcon(stallThemeButton, "ST", Saffron);
            stallThemeLabel = stallThemeButton.GetComponentInChildren<Text>();
            stallThemeButton.onClick.AddListener(HandleCycleStallTheme);

            cupPackButton = CreateButton("Cup Pack", section.transform, "Cup Pack", 22, Saffron, 64);
            AddCardIcon(cupPackButton, "CUP", Gold);
            cupPackLabel = cupPackButton.GetComponentInChildren<Text>();
            cupPackButton.onClick.AddListener(HandleCycleCupPack);

            signboardPackButton = CreateButton("Signboard Pack", section.transform, "Signboard", 22, Saffron, 64);
            AddCardIcon(signboardPackButton, "SGN", Rose);
            signboardPackLabel = signboardPackButton.GetComponentInChildren<Text>();
            signboardPackButton.onClick.AddListener(HandleCycleSignboardPack);
        }

        private void BuildProductionPanel(Transform parent)
        {
            GameObject section = CreatePanel("Privacy And Services", parent, new Color(0.77f, 0.91f, 1f), 590);
            AddShadow(section, new Vector2(0, -6), 0.22f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 8;

            Text title = CreateText("Production Title", section.transform, "Services & Privacy", 34, Teal, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            productionText = CreateText("Production Status", section.transform, string.Empty, 23, Ink, TextAnchor.MiddleLeft);

            privacyPolicyButton = CreateButton("Privacy Policy", section.transform, "Privacy Policy", 22, Teal, 64);
            privacyPolicyLabel = privacyPolicyButton.GetComponentInChildren<Text>();
            privacyPolicyButton.onClick.AddListener(HandlePrivacyPolicy);

            analyticsConsentButton = CreateButton("Analytics Consent", section.transform, "Analytics Off", 22, Leaf, 62);
            analyticsConsentLabel = analyticsConsentButton.GetComponentInChildren<Text>();
            analyticsConsentButton.onClick.AddListener(HandleAnalyticsConsentToggle);

            adsConsentButton = CreateButton("Ads Consent", section.transform, "Ads Consent Off", 22, Leaf, 62);
            adsConsentLabel = adsConsentButton.GetComponentInChildren<Text>();
            adsConsentButton.onClick.AddListener(HandleAdsConsentToggle);

            crashConsentButton = CreateButton("Crash Consent", section.transform, "Crash Reports Off", 22, Rose, 62);
            crashConsentLabel = crashConsentButton.GetComponentInChildren<Text>();
            crashConsentButton.onClick.AddListener(HandleCrashConsentToggle);

            cloudSaveExportButton = CreateButton("Cloud Save Export", section.transform, "Export Cloud Save", 22, Saffron, 66);
            cloudSaveExportLabel = cloudSaveExportButton.GetComponentInChildren<Text>();
            cloudSaveExportButton.onClick.AddListener(HandleCloudSaveExport);
        }

        private void BuildSettings(Transform parent)
        {
            GameObject section = CreatePanel("Settings", parent, new Color(0.88f, 0.92f, 0.78f), 278);
            AddShadow(section, new Vector2(0, -6), 0.2f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 12;

            Text title = CreateText("Settings Title", section.transform, "Tapri Settings", 34, Ink, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

            hapticsToggleButton = CreateButton("Haptics Toggle", section.transform, "Haptics On", 24, Leaf, 66);
            hapticsToggleLabel = hapticsToggleButton.GetComponentInChildren<Text>();
            hapticsToggleButton.onClick.AddListener(HandleHapticsToggle);

            resetSaveButton = CreateButton("Reset Save", section.transform, "Reset Save", 24, Rose, 76);
            resetSaveLabel = resetSaveButton.GetComponentInChildren<Text>();
            resetSaveButton.onClick.AddListener(HandleResetSave);
        }

        private void RefreshAll()
        {
            RefreshTutorial();
            RefreshSettings();

            rupeesText.text = ChaiNumberFormatter.Rupees(game.State.Rupees);
            rateText.text = ChaiNumberFormatter.PerSecond(game.GetPassiveRupeesPerSecond());
            tapText.text = ChaiNumberFormatter.Rupees(game.GetTapValue());
            servedText.text = ChaiNumberFormatter.Compact(game.State.ChaiServed);
            if (legacyText != null)
            {
                legacyText.text = "Masala Legacy  " + ChaiNumberFormatter.Compact(game.State.Prestige.MasalaLegacy);
            }

            LocationDefinition currentLocation = GetCurrentLocation();
            locationText.text = currentLocation.DisplayName;
            if (demandPillText != null)
            {
                demandPillText.text = "Demand\nx" + game.GetDemandMultiplier().ToString("0.##");
            }

            if (stallSignText != null)
            {
                stallSignText.text = currentLocation.DisplayName.ToUpperInvariant();
            }

            if (kettleButtonLabel != null)
            {
                kettleButtonLabel.text = "TAP KETTLE\n+" + ChaiNumberFormatter.Rupees(game.GetTapValue());
            }

            ApplyLocationBackdrop(currentLocation.Id);

            if (game.State.RushRemainingSeconds > 0)
            {
                rushText.text = "Rush active  " + Mathf.CeilToInt((float)game.State.RushRemainingSeconds) + " sec";
            }
            else if (game.State.RushCooldownSeconds > 0)
            {
                rushText.text = "Rush ready in  " + Mathf.CeilToInt((float)game.State.RushCooldownSeconds) + " sec";
            }
            else
            {
                rushText.text = "Rush ready";
            }

            rushButton.interactable = game.State.RushCooldownSeconds <= 0;
            SetButtonColor(rushButton, rushButton.interactable ? Rose : Disabled);
            if (rushBanner != null)
            {
                rushBanner.SetActive(game.State.RushRemainingSeconds > 0 || game.State.RushCooldownSeconds <= 0);
            }

            if (rushBurstImage != null)
            {
                Color rushColor = game.State.RushRemainingSeconds > 0 ?
                    new Color(1f, 0.31f, 0.02f, 0.16f) :
                    new Color(1f, 0.24f, 0.08f, 0f);
                rushBurstImage.color = rushColor;
            }

            RefreshEvents();
            RefreshMonetization();
            RefreshProduction();

            foreach (UpgradeRow row in upgradeRows)
            {
                int level = game.State.GetUpgradeLevel(row.Definition.Id);
                BigDouble cost = game.GetUpgradeCost(row.Definition.Id);
                bool canBuy = game.State.Rupees >= cost;
                bool isMaxed = row.Definition.MaxLevel > 0 && level >= row.Definition.MaxLevel;
                string badge = isMaxed ? "MAXED" : canBuy ? "READY TO BUY" : "SAVE UP";
                row.Label.text = row.Definition.DisplayName + "  Lv " + level + "\n" +
                    row.Definition.Category + " | " + DescribeUpgrade(row.Definition) + "\n" +
                    badge + " | " + ChaiNumberFormatter.Rupees(cost);
                row.Button.interactable = true;
                SetButtonColor(row.Button, isMaxed ? Purple : canBuy ? Leaf : new Color(0.47f, 0.42f, 0.33f));
            }

            foreach (LocationRow row in locationRows)
            {
                bool unlocked = game.State.IsLocationUnlocked(row.Definition.Id);
                BigDouble unlockCost = game.GetLocationUnlockCost(row.Definition.Id);
                bool canUnlock = !unlocked && game.State.Rupees >= unlockCost;
                row.Label.text = row.Definition.DisplayName + "\n" +
                    "Demand x" + row.Definition.DemandMultiplier.ToString("0.##") + " | " +
                    (unlocked ? "Unlocked" : canUnlock ? "Unlock now" : "Needs " + ChaiNumberFormatter.Rupees(unlockCost));
                row.Button.interactable = true;
                SetButtonColor(row.Button, unlocked ? Saffron : canUnlock ? Leaf : new Color(0.43f, 0.49f, 0.45f));
            }

            RefreshPrestige();
            RefreshUpgradeDetail();
        }

        private void RefreshTutorial()
        {
            if (tutorialPanel == null)
            {
                return;
            }

            ChaiTutorialPrompt prompt = ChaiTutorial.GetPrompt(game.State, content);
            tutorialPanel.SetActive(prompt.ShouldShow);
            if (!prompt.ShouldShow)
            {
                return;
            }

            tutorialTitleText.text = prompt.Title;
            tutorialBodyText.text = prompt.Body;
            tutorialProgressText.text = prompt.Progress;
            tutorialPrimaryLabel.text = prompt.PrimaryButtonLabel;
        }

        private void RefreshSettings()
        {
            if (resetSaveButton == null)
            {
                return;
            }

            hapticsToggleLabel.text = hapticsEnabled ? "Haptics On" : "Haptics Off";
            SetButtonColor(hapticsToggleButton, hapticsEnabled ? Leaf : Disabled);
            resetSaveLabel.text = resetSaveArmed ? "Confirm Reset" : "Reset Save";
            SetButtonColor(resetSaveButton, resetSaveArmed ? Rose : Teal);
        }

        private void RefreshPrestige()
        {
            if (prestigeText == null || prestigeButton == null)
            {
                return;
            }

            PrestigePreview preview = game.GetPrestigePreview();
            prestigeText.text = preview.Message + "\n" +
                "Current legacy: " + ChaiNumberFormatter.Compact(game.State.Prestige.MasalaLegacy) +
                "  |  Skill points: " + game.State.Prestige.UnspentSkillPoints + "\n" +
                "Projected gain: " + ChaiNumberFormatter.Compact(preview.ProjectedMasalaLegacy) +
                "  |  Reset: rupees, upgrades, locations";

            prestigeButton.interactable = preview.CanPrestige;
            prestigeButtonLabel.text = prestigeArmed ? "Confirm Prestige" : preview.CanPrestige ? "Preserve Secret Masala" : "Locked";
            SetButtonColor(prestigeButton, preview.CanPrestige ? prestigeArmed ? Rose : Teal : Disabled);

            foreach (PrestigeSkillRow row in prestigeSkillRows)
            {
                int level = game.State.Prestige.GetSkillLevel(row.Definition.Id);
                bool isMaxed = level >= row.Definition.MaxLevel;
                bool canSpend = !isMaxed && game.State.Prestige.UnspentSkillPoints > 0;
                row.Label.text = row.Definition.DisplayName + "  Lv " + level + "/" + row.Definition.MaxLevel + "\n" +
                    row.Definition.Branch + " | " + row.Definition.EffectLabel;
                row.Button.interactable = canSpend;
                SetButtonColor(row.Button, isMaxed ? Saffron : canSpend ? Leaf : Disabled);
            }
        }

        private void ShowUpgradeDetail(UpgradeDefinition definition)
        {
            selectedUpgrade = definition;
            if (upgradeDetailModal != null)
            {
                upgradeDetailModal.SetActive(true);
                upgradeDetailModal.transform.SetAsLastSibling();
            }

            RefreshUpgradeDetail();
        }

        private void RefreshUpgradeDetail()
        {
            if (upgradeDetailModal == null || !upgradeDetailModal.activeSelf || selectedUpgrade == null || game == null)
            {
                return;
            }

            int level = game.State.GetUpgradeLevel(selectedUpgrade.Id);
            BigDouble cost = game.GetUpgradeCost(selectedUpgrade.Id);
            bool canBuy = game.State.Rupees >= cost;
            bool isMaxed = selectedUpgrade.MaxLevel > 0 && level >= selectedUpgrade.MaxLevel;

            upgradeDetailTitleText.text = selectedUpgrade.DisplayName;
            upgradeDetailCategoryText.text = selectedUpgrade.Category;
            upgradeDetailLevelText.text = "Lv " + level + (selectedUpgrade.MaxLevel > 0 ? "/" + selectedUpgrade.MaxLevel : string.Empty);
            upgradeDetailEffectText.text = DescribeUpgrade(selectedUpgrade);
            upgradeDetailCostText.text = ChaiNumberFormatter.Rupees(cost);
            upgradeDetailDescriptionText.text = selectedUpgrade.Description + "\n\n" +
                (selectedUpgrade.IsAutomation ? "Automation keeps the stall earning while you plan the next expansion." : "Tap power upgrades make every kettle press feel better.");
            upgradeDetailBuyButton.interactable = canBuy && !isMaxed;
            upgradeDetailBuyLabel.text = isMaxed ? "MAXED" : canBuy ? "BUY UPGRADE" : "NEED " + ChaiNumberFormatter.Rupees(cost);
            SetButtonColor(upgradeDetailBuyButton, upgradeDetailBuyButton.interactable ? Saffron : Disabled);
        }

        private void HideUpgradeDetail()
        {
            PlayButtonPressSound();
            if (upgradeDetailModal != null)
            {
                upgradeDetailModal.SetActive(false);
            }
        }

        private void HandleUpgradeDetailBuy()
        {
            PlayButtonPressSound();
            if (selectedUpgrade == null)
            {
                return;
            }

            if (game.TryBuyUpgrade(selectedUpgrade.Id))
            {
                PlayPurchaseSound();
                TriggerHaptic();
                PulseTapButton();
                SpawnFloatingReward("UPGRADE", Gold, new Vector2(0, 164));
                SetStatus(selectedUpgrade.DisplayName + " upgraded");
                RefreshAll();
                return;
            }

            SetStatus("Need " + ChaiNumberFormatter.Rupees(game.GetUpgradeCost(selectedUpgrade.Id)));
            RefreshUpgradeDetail();
        }

        private void RefreshEvents()
        {
            if (eventText == null || eventButton == null)
            {
                return;
            }

            EventState eventState = game.State.Event;
            if (game.TryGetActiveEvent(out ChaiEventDefinition activeEvent))
            {
                eventText.text = activeEvent.DisplayName + "\n" +
                    activeEvent.Description + "\n" +
                    "Active " + FormatDuration(eventState.RemainingSeconds) + "  |  " + FormatEventEffects(activeEvent);
                eventButton.interactable = false;
                eventButtonLabel.text = "Event Active";
                SetButtonColor(eventButton, Saffron);
                return;
            }

            ChaiEventDefinition nextEvent = game.GetNextEventDefinition();
            if (eventState.CooldownSeconds > 0)
            {
                eventText.text = "Next: " + nextEvent.DisplayName + "\n" +
                    nextEvent.Description + "\n" +
                    "Ready in " + FormatDuration(eventState.CooldownSeconds);
                eventButton.interactable = false;
                eventButtonLabel.text = "Cooling Down";
                SetButtonColor(eventButton, Disabled);
                return;
            }

            eventText.text = "Next: " + nextEvent.DisplayName + "\n" +
                nextEvent.Description + "\n" +
                FormatEventEffects(nextEvent);
            eventButton.interactable = true;
            eventButtonLabel.text = "Start Event";
            SetButtonColor(eventButton, Leaf);
        }

        private void RefreshMonetization()
        {
            if (monetizationText == null)
            {
                return;
            }

            MonetizationState monetization = game.State.Monetization;
            CosmeticState cosmetics = game.State.Cosmetics;
            monetizationText.text = "Boost x" + game.GetProductionBoostMultiplier().ToString("0.#") +
                "  |  No Ads " + (monetization.NoAdsPurchased ? "Owned" : "Available") + "\n" +
                "Theme: " + ChaiCosmetics.GetDisplayName(ChaiCosmetics.StallThemes, cosmetics.StallThemeId) +
                "  |  Cups: " + ChaiCosmetics.GetDisplayName(ChaiCosmetics.CupPacks, cosmetics.CupPackId) + "\n" +
                "Signboard: " + ChaiCosmetics.GetDisplayName(ChaiCosmetics.SignboardPacks, cosmetics.SignboardPackId);

            if (monetization.ProductionBoostRemainingSeconds > 0)
            {
                sponsorBoostLabel.text = "Boost Active " + FormatDuration(monetization.ProductionBoostRemainingSeconds);
                sponsorBoostButton.interactable = false;
                SetButtonColor(sponsorBoostButton, Saffron);
            }
            else if (monetization.ProductionBoostCooldownSeconds > 0)
            {
                sponsorBoostLabel.text = "Boost Ready in " + FormatDuration(monetization.ProductionBoostCooldownSeconds);
                sponsorBoostButton.interactable = false;
                SetButtonColor(sponsorBoostButton, Disabled);
            }
            else
            {
                sponsorBoostLabel.text = "Optional Sponsor Boost";
                sponsorBoostButton.interactable = true;
                SetButtonColor(sponsorBoostButton, Leaf);
            }

            noAdsLabel.text = monetization.NoAdsPurchased ? "No Ads Owned" : "Optional No Ads";
            noAdsButton.interactable = !monetization.NoAdsPurchased;
            SetButtonColor(noAdsButton, monetization.NoAdsPurchased ? Disabled : Teal);

            stallThemeLabel.text = "Theme: " + ChaiCosmetics.GetDisplayName(ChaiCosmetics.StallThemes, cosmetics.StallThemeId);
            cupPackLabel.text = "Cups: " + ChaiCosmetics.GetDisplayName(ChaiCosmetics.CupPacks, cosmetics.CupPackId);
            signboardPackLabel.text = "Sign: " + ChaiCosmetics.GetDisplayName(ChaiCosmetics.SignboardPacks, cosmetics.SignboardPackId);
        }

        private void RefreshProduction()
        {
            if (productionText == null)
            {
                return;
            }

            int unlockedCount = game.GetUnlockedAchievements().Count;
            ProductionState production = game.State.Production;
            productionText.text = "Privacy " + (production.PrivacyPolicyAcknowledged ? "Seen" : "Pending") +
                "  |  Achievements " + unlockedCount + "/" + ChaiProductionServices.Achievements.Count + "\n" +
                "Analytics " + (production.AnalyticsConsent ? "On" : "Off") +
                "  |  Ads " + (production.AdsConsent ? "On" : "Off") +
                "  |  Crash " + (production.CrashReportingConsent ? "On" : "Off") + "\n" +
                "Cloud exports " + Math.Max(0, production.CloudSaveExportCount);

            privacyPolicyLabel.text = production.PrivacyPolicyAcknowledged ? "Privacy Policy Seen" : "Privacy Policy";
            SetButtonColor(privacyPolicyButton, production.PrivacyPolicyAcknowledged ? Leaf : Teal);

            analyticsConsentLabel.text = production.AnalyticsConsent ? "Analytics On" : "Analytics Off";
            SetButtonColor(analyticsConsentButton, production.AnalyticsConsent ? Leaf : Disabled);

            adsConsentLabel.text = production.AdsConsent ? "Ads Consent On" : "Ads Consent Off";
            SetButtonColor(adsConsentButton, production.AdsConsent ? Leaf : Disabled);

            crashConsentLabel.text = production.CrashReportingConsent ? "Crash Reports On" : "Crash Reports Off";
            SetButtonColor(crashConsentButton, production.CrashReportingConsent ? Leaf : Disabled);

            cloudSaveExportLabel.text = "Export Cloud Save";
            cloudSaveExportButton.interactable = true;
            SetButtonColor(cloudSaveExportButton, Saffron);
        }

        private void HandleHapticsToggle()
        {
            PlayButtonPressSound();
            hapticsEnabled = !hapticsEnabled;
            SetStatus(hapticsEnabled ? "Haptics on" : "Haptics off");
            RefreshSettings();
        }

        private void HandleTutorialPrimary()
        {
            PlayButtonPressSound();
            ChaiTutorialPrompt prompt = ChaiTutorial.GetPrompt(game.State, content);
            switch (prompt.PrimaryAction)
            {
                case ChaiTutorialAction.TapKettle:
                    game.TapKettle();
                    PulseTapButton();
                    SpawnFloatingReward("+" + ChaiNumberFormatter.Rupees(game.GetTapValue()), Gold, new Vector2(0, 120));
                    SetStatus("Fresh cutting chai");
                    break;
                case ChaiTutorialAction.BuyFirstUpgrade:
                    if (game.TryBuyUpgrade(ChaiTutorial.FirstUpgradeId))
                    {
                        PlayPurchaseSound();
                        TriggerHaptic();
                        SpawnFloatingReward("FIRST BUY", Gold, new Vector2(0, 164));
                        SetStatus("Strong Tea Leaves upgraded");
                    }
                    break;
            }

            RefreshAll();
        }

        private void HandleResetSave()
        {
            PlayButtonPressSound();
            if (!resetSaveArmed)
            {
                resetSaveArmed = true;
                resetConfirmTimer = ResetConfirmSeconds;
                SetStatus("Tap Confirm Reset to clear this save");
                RefreshSettings();
                return;
            }

            resetSaveArmed = false;
            resetConfirmTimer = 0;
            HideOfflineReward();

            if (ChaiSaveRepository.DeleteSave())
            {
                game = ChaiGame.NewGame(content);
                ChaiSaveRepository.Save(game.State);
                SetStatus("Save reset");
            }
            else
            {
                SetStatus("Save reset failed");
            }

            RefreshAll();
        }

        private void HandlePrestige()
        {
            PlayButtonPressSound();
            PrestigePreview preview = game.GetPrestigePreview();
            if (!preview.CanPrestige)
            {
                SetStatus("Reach Airport Lounge and 1B lifetime rupees first");
                RefreshPrestige();
                return;
            }

            if (!prestigeArmed)
            {
                prestigeArmed = true;
                prestigeConfirmTimer = PrestigeConfirmSeconds;
                SetStatus("Tap Confirm Prestige to reset this run");
                RefreshPrestige();
                return;
            }

            prestigeArmed = false;
            prestigeConfirmTimer = 0;
            HideOfflineReward();

            if (game.TryPrestige(out PrestigeResult result))
            {
                ChaiSaveRepository.Save(game.State);
                PlayUnlockSound();
                TriggerHaptic();
                SpawnFloatingReward("MASALA +" + ChaiNumberFormatter.Compact(result.GainedMasalaLegacy), Gold, new Vector2(0, 180));
                SetStatus("Preserved +" + ChaiNumberFormatter.Compact(result.GainedMasalaLegacy) + " legacy, +" + result.GainedSkillPoints + " points");
            }
            else
            {
                SetStatus(result.Message);
            }

            RefreshAll();
        }

        private void HandlePrestigeSkill(PrestigeSkillDefinition definition)
        {
            PlayButtonPressSound();
            if (game.TrySpendSkillPoint(definition.Id))
            {
                ChaiSaveRepository.Save(game.State);
                PlayPurchaseSound();
                TriggerHaptic();
                SpawnFloatingReward("SKILL UP", Gold, new Vector2(0, 160));
                SetStatus(definition.DisplayName + " upgraded");
            }
            else
            {
                SetStatus("Need a skill point");
            }

            RefreshAll();
        }

        private void HandleStartEvent()
        {
            PlayButtonPressSound();
            ChaiEventDefinition nextEvent = game.GetNextEventDefinition();
            if (game.TryStartEvent(nextEvent.Id))
            {
                ChaiSaveRepository.Save(game.State);
                PlayUnlockSound();
                TriggerHaptic();
                SpawnFloatingReward("EVENT LIVE", Leaf, new Vector2(0, 170));
                SetStatus(nextEvent.DisplayName + " started");
            }
            else
            {
                SetStatus("Event not ready");
            }

            RefreshAll();
        }

        private void HandleSponsorBoost()
        {
            PlayButtonPressSound();
            if (game.TryStartRewardedProductionBoost())
            {
                ChaiSaveRepository.Save(game.State);
                PlayUnlockSound();
                TriggerHaptic();
                SpawnFloatingReward("BOOST x2", Gold, new Vector2(0, 168));
                SetStatus("Sponsor boost: x2 for 2m");
            }
            else
            {
                SetStatus("Sponsor boost not ready");
            }

            RefreshAll();
        }

        private void HandleNoAdsPurchase()
        {
            PlayButtonPressSound();
            if (game.TryPurchaseNoAds())
            {
                ChaiSaveRepository.Save(game.State);
                PlayPurchaseSound();
                SpawnFloatingReward("OWNED", Gold, new Vector2(0, 146));
                SetStatus("No Ads owned");
            }
            else
            {
                SetStatus("No Ads already owned");
            }

            RefreshAll();
        }

        private void HandlePrivacyPolicy()
        {
            PlayButtonPressSound();
            game.AcknowledgePrivacyPolicy();
            ChaiSaveRepository.Save(game.State);
            Application.OpenURL(ChaiProductionServices.PrivacyPolicyUrl);
            SetStatus("Privacy policy opened");
            RefreshAll();
        }

        private void HandleAnalyticsConsentToggle()
        {
            PlayButtonPressSound();
            bool enabled = !game.State.Production.AnalyticsConsent;
            game.SetAnalyticsConsent(enabled);
            ChaiSaveRepository.Save(game.State);
            SetStatus(enabled ? "Analytics on" : "Analytics off");
            RefreshAll();
        }

        private void HandleAdsConsentToggle()
        {
            PlayButtonPressSound();
            bool enabled = !game.State.Production.AdsConsent;
            game.SetAdsConsent(enabled);
            ChaiSaveRepository.Save(game.State);
            SetStatus(enabled ? "Ads consent on" : "Ads consent off");
            RefreshAll();
        }

        private void HandleCrashConsentToggle()
        {
            PlayButtonPressSound();
            bool enabled = !game.State.Production.CrashReportingConsent;
            game.SetCrashReportingConsent(enabled);
            ChaiSaveRepository.Save(game.State);
            SetStatus(enabled ? "Crash reports on" : "Crash reports off");
            RefreshAll();
        }

        private void HandleCloudSaveExport()
        {
            PlayButtonPressSound();
            GUIUtility.systemCopyBuffer = game.ExportCloudSavePayload();
            ChaiSaveRepository.Save(game.State);
            SetStatus("Cloud save copied");
            RefreshAll();
        }

        private void HandleCycleStallTheme()
        {
            CycleCosmetic(ChaiCosmetics.StallThemes, game.State.Cosmetics.StallThemeId, game.TrySelectStallTheme, "Theme selected");
        }

        private void HandleCycleCupPack()
        {
            CycleCosmetic(ChaiCosmetics.CupPacks, game.State.Cosmetics.CupPackId, game.TrySelectCupPack, "Cup pack selected");
        }

        private void HandleCycleSignboardPack()
        {
            CycleCosmetic(ChaiCosmetics.SignboardPacks, game.State.Cosmetics.SignboardPackId, game.TrySelectSignboardPack, "Signboard selected");
        }

        private void CycleCosmetic(IReadOnlyList<CosmeticDefinition> definitions, string currentId, Func<string, bool> select, string status)
        {
            PlayButtonPressSound();
            string nextId = ChaiCosmetics.GetNextId(definitions, currentId);
            if (select(nextId))
            {
                ChaiSaveRepository.Save(game.State);
                PlayPurchaseSound();
                SpawnFloatingReward("STYLE", Cream, new Vector2(0, 145));
                SetStatus(status);
            }

            RefreshAll();
        }

        private void HandleOfflineSponsorBonus()
        {
            PlayButtonPressSound();
            if (offlineSponsorClaimed || pendingOfflineReward.RupeesEarned <= BigDouble.Zero)
            {
                SetStatus("Sponsor bonus already claimed");
                return;
            }

            if (game.TryClaimRewardedOfflineBonus(pendingOfflineReward.RupeesEarned))
            {
                offlineSponsorClaimed = true;
                ChaiSaveRepository.Save(game.State);
                offlineRewardAmountText.text = ChaiNumberFormatter.Rupees(pendingOfflineReward.RupeesEarned * 2);
                offlineSponsorLabel.text = "Sponsor Bonus Claimed";
                offlineSponsorButton.interactable = false;
                SetButtonColor(offlineSponsorButton, Disabled);
                SpawnFloatingReward("OFFLINE x2", Gold, new Vector2(0, 170));
                SetStatus("Offline reward doubled");
            }
        }

        private void PlayButtonPressSound()
        {
            PlaySound(GetButtonPressClip(), 0.42f);
        }

        private void PlayPurchaseSound()
        {
            PlaySound(GetPurchaseClip(), 0.55f);
        }

        private void PlayUnlockSound()
        {
            PlaySound(GetUnlockClip(), 0.62f);
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, volume);
        }

        private void TriggerHaptic()
        {
            if (hapticsEnabled)
            {
#if UNITY_EDITOR
                return;
#else
                Handheld.Vibrate();
#endif
            }
        }

        private void ShowOfflineReward(OfflineReward reward)
        {
            if (offlineRewardModal == null)
            {
                return;
            }

            pendingOfflineReward = reward;
            offlineSponsorClaimed = false;
            offlineRewardAmountText.text = ChaiNumberFormatter.Rupees(reward.RupeesEarned);
            offlineRewardDetailText.text = "Away " + FormatDuration(reward.RawSeconds) + "  |  Efficiency " + FormatPercent(game.GetOfflineEfficiency());
            offlineRewardCapText.text = reward.WasCapped ?
                "Capped at " + FormatDuration(reward.CappedSeconds) :
                "Cap " + FormatDuration(game.GetOfflineCapSeconds());
            offlineSponsorLabel.text = "Optional Sponsor x2";
            offlineSponsorButton.interactable = reward.RupeesEarned > BigDouble.Zero;
            SetButtonColor(offlineSponsorButton, offlineSponsorButton.interactable ? Leaf : Disabled);
            offlineRewardModal.SetActive(true);
        }

        private void AnimateSteam(float deltaSeconds)
        {
            if (steamWisps.Count == 0 || deltaSeconds <= 0)
            {
                return;
            }

            steamTimer += deltaSeconds;
            for (int i = 0; i < steamWisps.Count; i++)
            {
                SteamWisp wisp = steamWisps[i];
                float cycle = Mathf.Repeat(steamTimer + wisp.PhaseSeconds, 2.4f) / 2.4f;
                float rise = Mathf.Lerp(0, 58, cycle);
                float sway = Mathf.Sin((cycle * Mathf.PI * 2f) + wisp.PhaseSeconds) * 18f;
                float alpha = Mathf.Sin(cycle * Mathf.PI) * 0.44f;

                wisp.Rect.anchoredPosition = wisp.BasePosition + new Vector2(sway, rise);
                wisp.Rect.sizeDelta = Vector2.Lerp(wisp.BaseSize, wisp.BaseSize * 1.35f, cycle);
                wisp.Image.color = new Color(1f, 0.96f, 0.86f, alpha);
            }
        }

        private void PulseTapButton()
        {
            tapFeedbackTimer = 0.34f;
            if (kettleButtonRect != null)
            {
                kettleButtonRect.localScale = new Vector3(1.04f, 1.04f, 1f);
            }

            if (tapPulseImage != null)
            {
                tapPulseImage.color = new Color(1f, 0.88f, 0.18f, 0.32f);
            }
        }

        private void AnimateTapFeedback(float deltaSeconds)
        {
            if (tapFeedbackTimer <= 0)
            {
                if (kettleButtonRect != null)
                {
                    kettleButtonRect.localScale = Vector3.one;
                }

                if (tapPulseImage != null)
                {
                    tapPulseImage.color = new Color(1f, 0.88f, 0.18f, 0f);
                }

                return;
            }

            tapFeedbackTimer -= deltaSeconds;
            float t = Mathf.Clamp01(1f - (tapFeedbackTimer / 0.34f));
            float buttonScale = Mathf.Lerp(1.04f, 1f, t);
            if (kettleButtonRect != null)
            {
                kettleButtonRect.localScale = new Vector3(buttonScale, buttonScale, 1f);
            }

            if (tapPulseImage != null && tapPulseRect != null)
            {
                float pulseScale = Mathf.Lerp(1f, 1.38f, t);
                tapPulseRect.localScale = new Vector3(pulseScale, pulseScale, 1f);
                tapPulseImage.color = new Color(1f, 0.88f, 0.18f, Mathf.Lerp(0.32f, 0f, t));
            }
        }

        private void SpawnFloatingReward(string message, Color color, Vector2 anchoredPosition)
        {
            if (feedbackLayer == null)
            {
                return;
            }

            GameObject rewardObject = CreateChild("Floating Reward " + floatingRewardSeed.ToString("00"), feedbackLayer);
            floatingRewardSeed++;
            RectTransform rect = rewardObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360, 72);

            Text text = rewardObject.AddComponent<Text>();
            text.text = message;
            text.font = GetFont();
            text.fontSize = 34;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 20;
            AddOutline(rewardObject, Color.white, new Vector2(2, 2));
            floatingRewards.Add(new FloatingReward(rewardObject, rect, text, anchoredPosition, color, 1.05f));
        }

        private void AnimateFloatingRewards(float deltaSeconds)
        {
            for (int i = floatingRewards.Count - 1; i >= 0; i--)
            {
                FloatingReward reward = floatingRewards[i];
                reward.LifeSeconds -= deltaSeconds;
                float t = Mathf.Clamp01(1f - reward.LifeSeconds / 1.05f);
                reward.Rect.anchoredPosition = reward.StartPosition + new Vector2(Mathf.Sin(t * Mathf.PI) * 22f, Mathf.Lerp(0, 118, t));
                reward.Rect.localScale = Vector3.one * Mathf.Lerp(0.92f, 1.08f, Mathf.Sin(t * Mathf.PI));
                reward.Text.color = new Color(reward.Color.r, reward.Color.g, reward.Color.b, Mathf.Lerp(1f, 0f, t));

                if (reward.LifeSeconds > 0)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(reward.GameObject);
                }
                else
                {
                    DestroyImmediate(reward.GameObject);
                }

                floatingRewards.RemoveAt(i);
            }
        }

        private void HideOfflineReward()
        {
            if (offlineRewardModal != null)
            {
                offlineRewardModal.SetActive(false);
            }
        }

        private LocationDefinition GetCurrentLocation()
        {
            LocationDefinition current = content.Locations[0];
            foreach (LocationDefinition location in content.Locations)
            {
                if (game.State.IsLocationUnlocked(location.Id) && location.DemandMultiplier >= current.DemandMultiplier)
                {
                    current = location;
                }
            }

            return current;
        }

        private void ApplyLocationBackdrop(string locationId)
        {
            if (stallArtBackground == null)
            {
                return;
            }

            LocationVisualPalette palette = GetLocationVisualPalette(locationId);
            stallArtBackground.color = palette.Backdrop;
            stallArtGlow.color = palette.Glow;
            stallCounterTop.color = palette.CounterTop;
            stallCounterFront.color = palette.CounterFront;
            ApplyCosmeticTheme();
        }

        private void ApplyCosmeticTheme()
        {
            if (game.State.Cosmetics == null)
            {
                return;
            }

            switch (game.State.Cosmetics.StallThemeId)
            {
                case "festival-lights":
                    stallArtGlow.color = new Color(1f, 0.88f, 0.42f);
                    stallCounterTop.color = Saffron;
                    stallCounterFront.color = Rose;
                    break;
                case "monsoon-blue":
                    stallArtGlow.color = new Color(0.74f, 0.93f, 1f);
                    stallCounterTop.color = Teal;
                    stallCounterFront.color = new Color(0.12f, 0.27f, 0.34f);
                    break;
            }
        }

        private static string FormatPercent(double value)
        {
            return (value * 100d).ToString("0") + "%";
        }

        private static string FormatDuration(double seconds)
        {
            double clampedSeconds = Math.Max(0, seconds);
            if (clampedSeconds >= 3600)
            {
                return (clampedSeconds / 3600d).ToString("0.#") + "h";
            }

            if (clampedSeconds >= 60)
            {
                return (clampedSeconds / 60d).ToString("0.#") + "m";
            }

            return clampedSeconds.ToString("0") + "s";
        }

        private static string FormatEventEffects(ChaiEventDefinition definition)
        {
            return "Tap x" + (1 + definition.TapMultiplierBonus).ToString("0.##") +
                "  |  Passive x" + (1 + definition.PassiveMultiplierBonus).ToString("0.##") +
                "  |  All x" + (1 + definition.GlobalMultiplierBonus).ToString("0.##");
        }

        private static string DescribeUpgrade(UpgradeDefinition upgrade)
        {
            switch (upgrade.Kind)
            {
                case UpgradeKind.TapFlat:
                    return "+" + upgrade.ValuePerLevel.ToString("0.##") + " tap";
                case UpgradeKind.TapMultiplier:
                    return "+" + (upgrade.ValuePerLevel * 100).ToString("0") + "% tap";
                case UpgradeKind.PassiveFlat:
                    return "+" + upgrade.ValuePerLevel.ToString("0.##") + "/sec";
                case UpgradeKind.GlobalMultiplier:
                    return "+" + (upgrade.ValuePerLevel * 100).ToString("0") + "% all";
                default:
                    return string.Empty;
            }
        }

        private void SetStatus(string message)
        {
            if (statusText == null)
            {
                return;
            }

            statusText.text = message;
            statusTimer = 3.5f;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color, float height)
        {
            GameObject panel = CreateChild(name, parent);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height > 0 ? height : -1;
            return panel;
        }

        private static Image CreateArtShape(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color, Sprite sprite, float rotationDegrees)
        {
            GameObject shape = CreateChild(name, parent);
            RectTransform rect = shape.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localEulerAngles = new Vector3(0, 0, rotationDegrees);

            Image image = shape.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static LocationVisualPalette GetLocationVisualPalette(string locationId)
        {
            switch (locationId)
            {
                case "bus-stand":
                    return new LocationVisualPalette(
                        new Color(0.98f, 0.78f, 0.47f),
                        new Color(1f, 0.90f, 0.56f),
                        new Color(0.30f, 0.20f, 0.14f),
                        new Color(0.49f, 0.30f, 0.16f));
                case "railway-platform":
                    return new LocationVisualPalette(
                        new Color(0.72f, 0.78f, 0.82f),
                        new Color(0.88f, 0.92f, 0.92f),
                        new Color(0.25f, 0.25f, 0.24f),
                        new Color(0.42f, 0.34f, 0.25f));
                case "college-canteen":
                    return new LocationVisualPalette(
                        new Color(0.84f, 0.86f, 0.58f),
                        new Color(0.97f, 0.92f, 0.62f),
                        new Color(0.29f, 0.23f, 0.12f),
                        new Color(0.42f, 0.33f, 0.18f));
                case "it-park":
                    return new LocationVisualPalette(
                        new Color(0.60f, 0.82f, 0.88f),
                        new Color(0.80f, 0.95f, 1f),
                        new Color(0.18f, 0.24f, 0.27f),
                        new Color(0.30f, 0.38f, 0.42f));
                case "highway-dhaba":
                    return new LocationVisualPalette(
                        new Color(0.89f, 0.70f, 0.44f),
                        new Color(1f, 0.86f, 0.55f),
                        new Color(0.31f, 0.16f, 0.08f),
                        new Color(0.49f, 0.25f, 0.12f));
                case "mall-kiosk":
                    return new LocationVisualPalette(
                        new Color(0.84f, 0.75f, 0.90f),
                        new Color(0.98f, 0.88f, 1f),
                        new Color(0.26f, 0.19f, 0.34f),
                        new Color(0.45f, 0.31f, 0.52f));
                case "airport-lounge":
                    return new LocationVisualPalette(
                        new Color(0.69f, 0.83f, 0.93f),
                        new Color(0.92f, 0.98f, 1f),
                        new Color(0.20f, 0.23f, 0.29f),
                        new Color(0.35f, 0.39f, 0.48f));
                default:
                    return new LocationVisualPalette(
                        new Color(0.99f, 0.84f, 0.58f),
                        new Color(1f, 0.91f, 0.68f),
                        new Color(0.36f, 0.18f, 0.10f),
                        new Color(0.48f, 0.25f, 0.13f));
            }
        }

        private void CreateSteamWisp(string name, Transform parent, Vector2 basePosition, Vector2 baseSize, float phaseSeconds)
        {
            Image image = CreateArtShape(name, parent, basePosition, baseSize, new Color(1f, 0.96f, 0.86f, 0.22f), GetCircleSprite(), 0);
            RectTransform rect = image.GetComponent<RectTransform>();
            steamWisps.Add(new SteamWisp(rect, image, basePosition, baseSize, phaseSeconds));
        }

        private static void AddShadow(GameObject target, Vector2 distance, float alpha)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.04f, 0.02f, alpha);
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static Text CreateGameIcon(Transform parent, string name, string glyph, Color color, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            return AddCardIconToTransform(parent, name, glyph, color, anchoredPosition, size, fontSize);
        }

        private static void AddCardIcon(Button button, string glyph, Color color)
        {
            AddCardIconToTransform(button.transform, "Card Icon", glyph, color, new Vector2(58, 0), new Vector2(76, 76), 20);
            Text label = button.GetComponentInChildren<Text>();
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.offsetMin = new Vector2(116, 10);
            labelRect.offsetMax = new Vector2(-18, -10);
            label.alignment = TextAnchor.MiddleLeft;
            label.resizeTextMinSize = 16;
            label.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static Text AddCardIconToTransform(Transform parent, string name, string glyph, Color color, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            GameObject icon = CreateChild(name, parent);
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = icon.AddComponent<Image>();
            image.sprite = GetCircleSprite();
            image.color = color;
            image.raycastTarget = false;
            AddOutline(icon, Color.white, new Vector2(2, 2));

            Text text = CreateText(name + " Glyph", icon.transform, glyph, fontSize, Color.white, TextAnchor.MiddleCenter);
            StretchToParent(text.GetComponent<RectTransform>());
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            return text;
        }

        private static string GetUpgradeIcon(UpgradeDefinition upgrade)
        {
            if (upgrade.IsAutomation)
            {
                return "AUTO";
            }

            switch (upgrade.Kind)
            {
                case UpgradeKind.TapFlat:
                    return "TAP";
                case UpgradeKind.TapMultiplier:
                    return "X";
                case UpgradeKind.PassiveFlat:
                    return "/s";
                case UpgradeKind.GlobalMultiplier:
                    return "ALL";
                default:
                    return "UP";
            }
        }

        private static Color GetUpgradeAccent(UpgradeDefinition upgrade)
        {
            if (upgrade.IsAutomation)
            {
                return Purple;
            }

            switch (upgrade.Kind)
            {
                case UpgradeKind.TapFlat:
                    return Saffron;
                case UpgradeKind.TapMultiplier:
                    return Gold;
                case UpgradeKind.PassiveFlat:
                    return Leaf;
                case UpgradeKind.GlobalMultiplier:
                    return Teal;
                default:
                    return Rose;
            }
        }

        private static Button CreateButton(string name, Transform parent, string text, int fontSize, Color color, float height)
        {
            GameObject buttonObject = CreateChild(name, parent);
            StretchHorizontally(buttonObject.GetComponent<RectTransform>(), height);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.15f);
            colors.disabledColor = Disabled;
            button.colors = colors;

            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height;

            Text label = CreateText("Label", buttonObject.transform, text, fontSize, Color.white, TextAnchor.MiddleCenter);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16, 8);
            labelRect.offsetMax = new Vector2(-16, -8);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            return button;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject textObject = CreateChild(name, parent);
            StretchHorizontally(textObject.GetComponent<RectTransform>(), Mathf.Max(36, fontSize + 14));
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = GetFont();
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            LayoutElement layout = textObject.AddComponent<LayoutElement>();
            layout.minHeight = Mathf.Max(36, fontSize + 14);
            return text;
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.AddComponent<RectTransform>();
            return child;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetStretchRect(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static void StretchHorizontally(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.sizeDelta = new Vector2(0, height);
        }

        private static void SetButtonColor(Button button, Color color)
        {
            Image image = button.GetComponent<Image>();
            image.color = color;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.15f);
            colors.disabledColor = Disabled;
            button.colors = colors;
        }

        private static Font GetFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static AudioClip GetButtonPressClip()
        {
            if (buttonPressClip != null)
            {
                return buttonPressClip;
            }

            const int sampleRate = 44100;
            const float durationSeconds = 0.055f;
            int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-52f * t);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 780f * t) * envelope * 0.55f;
            }

            buttonPressClip = AudioClip.Create("Chai Empire Button Press", sampleCount, 1, sampleRate, false);
            buttonPressClip.SetData(samples, 0);
            return buttonPressClip;
        }

        private static AudioClip GetPurchaseClip()
        {
            if (purchaseClip == null)
            {
                purchaseClip = CreateToneClip("Chai Empire Purchase", 0.12f, 660f, 990f, 28f, 0.48f);
            }

            return purchaseClip;
        }

        private static AudioClip GetUnlockClip()
        {
            if (unlockClip == null)
            {
                unlockClip = CreateToneClip("Chai Empire Unlock", 0.18f, 520f, 780f, 18f, 0.5f);
            }

            return unlockClip;
        }

        private static AudioClip CreateToneClip(string name, float durationSeconds, float startFrequency, float endFrequency, float decay, float gain)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float progress = t / durationSeconds;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                float envelope = Mathf.Exp(-decay * t);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * gain;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Sprite GetCircleSprite()
        {
            if (circleSprite != null)
            {
                return circleSprite;
            }

            circleSprite = Resources.Load<Sprite>("ChaiEmpire/chai-circle");
            if (circleSprite != null)
            {
                return circleSprite;
            }

            const int size = 64;
            const float radius = (size - 1) * 0.5f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                name = "Chai Empire Procedural Circle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[size * size];
            Color32 clear = new Color32(255, 255, 255, 0);
            Color32 fill = new Color32(255, 255, 255, 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    pixels[y * size + x] = (dx * dx + dy * dy) <= radius * radius ? fill : clear;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            circleSprite.name = "Chai Empire Procedural Circle";
            return circleSprite;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private sealed class UpgradeRow
        {
            public UpgradeRow(UpgradeDefinition definition, Button button, Text label)
            {
                Definition = definition;
                Button = button;
                Label = label;
            }

            public UpgradeDefinition Definition { get; }
            public Button Button { get; }
            public Text Label { get; }
        }

        private sealed class LocationRow
        {
            public LocationRow(LocationDefinition definition, Button button, Text label)
            {
                Definition = definition;
                Button = button;
                Label = label;
            }

            public LocationDefinition Definition { get; }
            public Button Button { get; }
            public Text Label { get; }
        }

        private sealed class PrestigeSkillRow
        {
            public PrestigeSkillRow(PrestigeSkillDefinition definition, Button button, Text label)
            {
                Definition = definition;
                Button = button;
                Label = label;
            }

            public PrestigeSkillDefinition Definition { get; }
            public Button Button { get; }
            public Text Label { get; }
        }

        private readonly struct LocationVisualPalette
        {
            public LocationVisualPalette(Color backdrop, Color glow, Color counterTop, Color counterFront)
            {
                Backdrop = backdrop;
                Glow = glow;
                CounterTop = counterTop;
                CounterFront = counterFront;
            }

            public Color Backdrop { get; }
            public Color Glow { get; }
            public Color CounterTop { get; }
            public Color CounterFront { get; }
        }

        private sealed class SteamWisp
        {
            public SteamWisp(RectTransform rect, Image image, Vector2 basePosition, Vector2 baseSize, float phaseSeconds)
            {
                Rect = rect;
                Image = image;
                BasePosition = basePosition;
                BaseSize = baseSize;
                PhaseSeconds = phaseSeconds;
            }

            public RectTransform Rect { get; }
            public Image Image { get; }
            public Vector2 BasePosition { get; }
            public Vector2 BaseSize { get; }
            public float PhaseSeconds { get; }
        }

        private sealed class FloatingReward
        {
            public FloatingReward(GameObject gameObject, RectTransform rect, Text text, Vector2 startPosition, Color color, float lifeSeconds)
            {
                GameObject = gameObject;
                Rect = rect;
                Text = text;
                StartPosition = startPosition;
                Color = color;
                LifeSeconds = lifeSeconds;
            }

            public GameObject GameObject { get; }
            public RectTransform Rect { get; }
            public Text Text { get; }
            public Vector2 StartPosition { get; }
            public Color Color { get; }
            public float LifeSeconds { get; set; }
        }
    }
}
