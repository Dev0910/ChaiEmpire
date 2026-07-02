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
        private const float ResetConfirmSeconds = 6f;
        private const float PrestigeConfirmSeconds = 8f;

        private static readonly Color Background = new Color(0.05f, 0.16f, 0.17f);
        private static readonly Color Panel = new Color(0.96f, 0.93f, 0.84f);
        private static readonly Color Ink = new Color(0.08f, 0.11f, 0.12f);
        private static readonly Color Saffron = new Color(0.93f, 0.43f, 0.16f);
        private static readonly Color Teal = new Color(0.07f, 0.43f, 0.43f);
        private static readonly Color Leaf = new Color(0.18f, 0.50f, 0.31f);
        private static readonly Color Rose = new Color(0.65f, 0.17f, 0.27f);
        private static readonly Color Disabled = new Color(0.45f, 0.48f, 0.48f);
        private static Sprite circleSprite;
        private static AudioClip buttonPressClip;
        private static AudioClip purchaseClip;
        private static AudioClip unlockClip;

        private readonly List<UpgradeRow> upgradeRows = new List<UpgradeRow>();
        private readonly List<LocationRow> locationRows = new List<LocationRow>();
        private readonly List<PrestigeSkillRow> prestigeSkillRows = new List<PrestigeSkillRow>();
        private readonly List<SteamWisp> steamWisps = new List<SteamWisp>();

        private ChaiContent content;
        private ChaiGame game;
        private AudioSource audioSource;
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
        private float refreshTimer;
        private float saveTimer;
        private float statusTimer;
        private float resetConfirmTimer;
        private float prestigeConfirmTimer;
        private float steamTimer;
        private bool hapticsEnabled = true;
        private bool resetSaveArmed;
        private bool prestigeArmed;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            content = ChaiContent.CreateDefault();
            LoadResult loadResult = ChaiSaveRepository.LoadOrCreate(content);
            game = ChaiGame.FromState(content, loadResult.State);

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

            Canvas canvas = CreateCanvas();
            audioSource = canvas.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;

            GameObject scrollObject = CreateChild("Scroll View", canvas.transform);
            RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(24, 24);
            scrollRect.offsetMax = new Vector2(-24, -24);

            Image scrollImage = scrollObject.AddComponent<Image>();
            scrollImage.color = new Color(1, 1, 1, 0);

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 32;

            GameObject viewport = CreateChild("Viewport", scrollObject.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            GameObject contentObject = CreateChild("Content", viewport.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLayout = contentObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(0, 0, 0, 28);
            rootLayout.spacing = 16;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            contentObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            BuildHeader(contentObject.transform);
            BuildStallArt(contentObject.transform);
            BuildStats(contentObject.transform);
            BuildTutorial(contentObject.transform);
            BuildActions(contentObject.transform);
            BuildUpgradeList(contentObject.transform);
            BuildLocationList(contentObject.transform);
            BuildPrestige(contentObject.transform);
            BuildSettings(contentObject.transform);
            BuildOfflineRewardModal(canvas.transform);
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Chai Empire Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            Image background = canvasObject.AddComponent<Image>();
            background.color = Background;
            return canvas;
        }

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, Teal, 170);
            VerticalLayoutGroup layout = header.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.spacing = 6;

            Text title = CreateText("Title", header.transform, "Chai Empire", 58, Color.white, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 36;
            locationText = CreateText("Location", header.transform, string.Empty, 26, new Color(0.88f, 1f, 0.96f), TextAnchor.MiddleLeft);
        }

        private void BuildStallArt(Transform parent)
        {
            Color backdrop = new Color(0.99f, 0.84f, 0.58f);
            Color counter = new Color(0.36f, 0.18f, 0.10f);
            Color stove = new Color(0.16f, 0.18f, 0.18f);
            Color stoveFace = new Color(0.28f, 0.30f, 0.28f);
            Color brass = new Color(0.90f, 0.58f, 0.22f);
            Color kettle = new Color(0.78f, 0.86f, 0.83f);
            Color kettleShade = new Color(0.12f, 0.42f, 0.42f);

            steamWisps.Clear();

            GameObject art = CreatePanel("Stall Art", parent, backdrop, 360);
            stallArtBackground = art.GetComponent<Image>();
            stallArtGlow = CreateArtShape("Back Wall Glow", art.transform, new Vector2(0, 28), new Vector2(820, 250), new Color(1f, 0.91f, 0.68f), GetCircleSprite(), 0);
            stallCounterTop = CreateArtShape("Counter Top", art.transform, new Vector2(0, -124), new Vector2(900, 56), counter, null, 0);
            stallCounterFront = CreateArtShape("Counter Front", art.transform, new Vector2(0, -160), new Vector2(860, 58), new Color(0.48f, 0.25f, 0.13f), null, 0);
            CreateCustomerQueue(art.transform);
            CreateUpiQrProp(art.transform);

            CreateArtShape("Stove Shadow", art.transform, new Vector2(0, -82), new Vector2(360, 54), new Color(0.09f, 0.10f, 0.10f, 0.35f), GetCircleSprite(), 0);
            CreateArtShape("Stove Base", art.transform, new Vector2(0, -64), new Vector2(360, 116), stove, null, 0);
            CreateArtShape("Stove Front", art.transform, new Vector2(0, -90), new Vector2(300, 54), stoveFace, null, 0);
            CreateArtShape("Burner Ring", art.transform, new Vector2(0, 0), new Vector2(220, 48), new Color(0.07f, 0.08f, 0.08f), GetCircleSprite(), 0);
            CreateArtShape("Flame Outer", art.transform, new Vector2(-10, 16), new Vector2(122, 86), Saffron, GetCircleSprite(), 0);
            CreateArtShape("Flame Inner", art.transform, new Vector2(12, 18), new Vector2(70, 58), new Color(1f, 0.83f, 0.24f), GetCircleSprite(), 0);

            CreateArtShape("Kettle Handle Outer", art.transform, new Vector2(-214, 78), new Vector2(170, 170), kettleShade, GetCircleSprite(), 0);
            CreateArtShape("Kettle Handle Hole", art.transform, new Vector2(-214, 78), new Vector2(104, 104), backdrop, GetCircleSprite(), 0);
            CreateArtShape("Kettle Spout", art.transform, new Vector2(215, 86), new Vector2(170, 42), kettleShade, null, -12);
            CreateArtShape("Kettle Spout Tip", art.transform, new Vector2(300, 102), new Vector2(62, 34), kettle, GetCircleSprite(), 0);
            CreateArtShape("Kettle Body", art.transform, new Vector2(0, 58), new Vector2(338, 184), kettle, GetCircleSprite(), 0);
            CreateArtShape("Kettle Belly", art.transform, new Vector2(0, 42), new Vector2(256, 122), kettleShade, GetCircleSprite(), 0);
            CreateArtShape("Kettle Highlight", art.transform, new Vector2(-72, 86), new Vector2(80, 42), new Color(1f, 0.96f, 0.82f, 0.75f), GetCircleSprite(), 0);
            CreateArtShape("Kettle Lid", art.transform, new Vector2(0, 155), new Vector2(176, 42), brass, GetCircleSprite(), 0);
            CreateArtShape("Kettle Knob", art.transform, new Vector2(0, 184), new Vector2(46, 34), counter, GetCircleSprite(), 0);
            CreateSteamWisp("Steam Wisp A", art.transform, new Vector2(-52, 214), new Vector2(34, 78), 0f);
            CreateSteamWisp("Steam Wisp B", art.transform, new Vector2(6, 224), new Vector2(28, 88), 0.85f);
            CreateSteamWisp("Steam Wisp C", art.transform, new Vector2(62, 210), new Vector2(30, 72), 1.7f);
        }

        private void CreateCustomerQueue(Transform parent)
        {
            CreateArtShape("Customer Queue Shadow", parent, new Vector2(350, -118), new Vector2(248, 30), new Color(0.16f, 0.08f, 0.04f, 0.22f), GetCircleSprite(), 0);
            CreateCustomer("Queue Customer A", parent, new Vector2(274, -52), new Color(0.18f, 0.50f, 0.31f), 0.92f);
            CreateCustomer("Queue Customer B", parent, new Vector2(350, -40), new Color(0.93f, 0.43f, 0.16f), 1f);
            CreateCustomer("Queue Customer C", parent, new Vector2(424, -58), new Color(0.65f, 0.17f, 0.27f), 0.84f);
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
            Vector2 origin = new Vector2(-348, -58);
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
            GameObject stats = CreatePanel("Stats", parent, Panel, 250);
            VerticalLayoutGroup layout = stats.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 8;

            rupeesText = CreateText("Rupees", stats.transform, string.Empty, 44, Ink, TextAnchor.MiddleLeft);
            rupeesText.fontStyle = FontStyle.Bold;
            rateText = CreateText("Rate", stats.transform, string.Empty, 28, Ink, TextAnchor.MiddleLeft);
            tapText = CreateText("Tap", stats.transform, string.Empty, 28, Ink, TextAnchor.MiddleLeft);
            servedText = CreateText("Served", stats.transform, string.Empty, 24, Ink, TextAnchor.MiddleLeft);
            legacyText = CreateText("Legacy", stats.transform, string.Empty, 24, Rose, TextAnchor.MiddleLeft);
        }

        private void BuildTutorial(Transform parent)
        {
            tutorialPanel = CreatePanel("Tutorial", parent, new Color(1f, 0.91f, 0.74f), 250);
            VerticalLayoutGroup layout = tutorialPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 6;

            tutorialTitleText = CreateText("Tutorial Title", tutorialPanel.transform, string.Empty, 30, Ink, TextAnchor.MiddleLeft);
            tutorialTitleText.fontStyle = FontStyle.Bold;
            tutorialBodyText = CreateText("Tutorial Body", tutorialPanel.transform, string.Empty, 24, Ink, TextAnchor.MiddleLeft);
            tutorialProgressText = CreateText("Tutorial Progress", tutorialPanel.transform, string.Empty, 22, Rose, TextAnchor.MiddleLeft);

            tutorialPrimaryButton = CreateButton("Tutorial Primary", tutorialPanel.transform, string.Empty, 24, Saffron, 64);
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
            scrim.color = new Color(0, 0, 0, 0.58f);

            GameObject card = CreateChild("Offline Reward Card", offlineRewardModal.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(860, 620);

            Image cardImage = card.AddComponent<Image>();
            cardImage.color = Panel;

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(42, 42, 38, 38);
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("Offline Reward Title", card.transform, "Welcome Back", 42, Rose, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            offlineRewardAmountText = CreateText("Offline Reward Amount", card.transform, string.Empty, 54, Ink, TextAnchor.MiddleCenter);
            offlineRewardAmountText.fontStyle = FontStyle.Bold;
            offlineRewardDetailText = CreateText("Offline Reward Detail", card.transform, string.Empty, 28, Ink, TextAnchor.MiddleCenter);
            offlineRewardCapText = CreateText("Offline Reward Cap", card.transform, string.Empty, 24, Rose, TextAnchor.MiddleCenter);

            Button claim = CreateButton("Claim Offline Reward", card.transform, "Claim", 30, Saffron, 82);
            claim.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                HideOfflineReward();
            });

            offlineRewardModal.SetActive(false);
        }

        private void BuildActions(Transform parent)
        {
            GameObject actions = CreatePanel("Actions", parent, new Color(0.91f, 0.96f, 0.91f), 320);
            VerticalLayoutGroup layout = actions.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 12;

            Button kettle = CreateButton("Brew Kettle", actions.transform, "Tap Kettle", 52, Saffron, 96);
            kettle.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                game.TapKettle();
                SetStatus("Fresh cutting chai");
                RefreshAll();
            });

            Button customers = CreateButton("Customer Queue", actions.transform, "Serve Queue", 34, Leaf, 76);
            customers.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                game.TapCustomerQueue();
                SetStatus("Queue served");
                RefreshAll();
            });

            rushButton = CreateButton("Rush Hour", actions.transform, "Rush Hour", 34, Rose, 76);
            rushButton.onClick.AddListener(() =>
            {
                PlayButtonPressSound();
                if (game.TryTriggerRushHour())
                {
                    TriggerHaptic();
                    SetStatus("Rush hour: 2x for 20 sec");
                    RefreshAll();
                }
            });

            rushText = CreateText("Rush Status", actions.transform, string.Empty, 24, Ink, TextAnchor.MiddleCenter);
        }

        private void BuildUpgradeList(Transform parent)
        {
            GameObject section = CreatePanel("Upgrades", parent, Panel, 0);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10;

            Text title = CreateText("Upgrades Title", section.transform, "Upgrades", 34, Ink, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

            foreach (UpgradeDefinition upgrade in content.Upgrades)
            {
                UpgradeDefinition captured = upgrade;
                Button button = CreateButton(upgrade.Id, section.transform, upgrade.DisplayName, 24, Teal, 108);
                Text label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() =>
                {
                    PlayButtonPressSound();
                    if (game.TryBuyUpgrade(captured.Id))
                    {
                        PlayPurchaseSound();
                        TriggerHaptic();
                        SetStatus(captured.DisplayName + " upgraded");
                        RefreshAll();
                    }
                });
                upgradeRows.Add(new UpgradeRow(captured, button, label));
            }
        }

        private void BuildLocationList(Transform parent)
        {
            GameObject section = CreatePanel("Locations", parent, new Color(0.92f, 0.95f, 0.99f), 0);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10;

            Text title = CreateText("Locations Title", section.transform, "Locations", 34, Ink, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

            foreach (LocationDefinition location in content.Locations)
            {
                if (location.UnlockedByDefault)
                {
                    continue;
                }

                LocationDefinition captured = location;
                Button button = CreateButton(location.Id, section.transform, location.DisplayName, 24, Leaf, 98);
                Text label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() =>
                {
                    PlayButtonPressSound();
                    if (game.TryUnlockLocation(captured.Id))
                    {
                        PlayUnlockSound();
                        TriggerHaptic();
                        SetStatus(captured.DisplayName + " unlocked");
                        RefreshAll();
                    }
                });
                locationRows.Add(new LocationRow(captured, button, label));
            }
        }

        private void BuildPrestige(Transform parent)
        {
            GameObject section = CreatePanel("Prestige", parent, new Color(0.96f, 0.92f, 0.97f), 1240);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 8;

            Text title = CreateText("Prestige Title", section.transform, "Secret Masala", 34, Rose, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            prestigeText = CreateText("Prestige Preview", section.transform, string.Empty, 24, Ink, TextAnchor.MiddleLeft);
            prestigeButton = CreateButton("Prestige Button", section.transform, "Preserve Secret Masala", 24, Rose, 76);
            prestigeButtonLabel = prestigeButton.GetComponentInChildren<Text>();
            prestigeButton.onClick.AddListener(HandlePrestige);

            Text skillTitle = CreateText("Skill Tree Title", section.transform, "Skill Tree", 30, Ink, TextAnchor.MiddleLeft);
            skillTitle.fontStyle = FontStyle.Bold;

            prestigeSkillRows.Clear();
            foreach (PrestigeSkillDefinition skill in ChaiPrestigeSkills.All)
            {
                PrestigeSkillDefinition captured = skill;
                Button button = CreateButton(skill.Id, section.transform, skill.DisplayName, 21, Teal, 86);
                Text label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => HandlePrestigeSkill(captured));
                prestigeSkillRows.Add(new PrestigeSkillRow(captured, button, label));
            }

            statusText = CreateText("Status", section.transform, string.Empty, 24, Rose, TextAnchor.MiddleLeft);
        }

        private void BuildSettings(Transform parent)
        {
            GameObject section = CreatePanel("Settings", parent, new Color(0.93f, 0.94f, 0.89f), 258);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 12;

            Text title = CreateText("Settings Title", section.transform, "Settings", 34, Ink, TextAnchor.MiddleLeft);
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
            rateText.text = "Production  " + ChaiNumberFormatter.PerSecond(game.GetPassiveRupeesPerSecond());
            tapText.text = "Kettle tap  " + ChaiNumberFormatter.Rupees(game.GetTapValue());
            servedText.text = "Chai served  " + ChaiNumberFormatter.Compact(game.State.ChaiServed);
            legacyText.text = "Masala Legacy  " + ChaiNumberFormatter.Compact(game.State.Prestige.MasalaLegacy);
            LocationDefinition currentLocation = GetCurrentLocation();
            locationText.text = currentLocation.DisplayName + "  |  Demand x" + game.GetDemandMultiplier().ToString("0.##");
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

            foreach (UpgradeRow row in upgradeRows)
            {
                int level = game.State.GetUpgradeLevel(row.Definition.Id);
                BigDouble cost = game.GetUpgradeCost(row.Definition.Id);
                bool canBuy = game.State.Rupees >= cost;
                row.Label.text = row.Definition.DisplayName + "  Lv " + level + "\n" +
                    row.Definition.Category + "  |  " + DescribeUpgrade(row.Definition) + "\n" +
                    "Cost " + ChaiNumberFormatter.Rupees(cost);
                row.Button.interactable = canBuy;
                SetButtonColor(row.Button, canBuy ? Teal : Disabled);
            }

            foreach (LocationRow row in locationRows)
            {
                bool unlocked = game.State.IsLocationUnlocked(row.Definition.Id);
                BigDouble unlockCost = game.GetLocationUnlockCost(row.Definition.Id);
                bool canUnlock = !unlocked && game.State.Rupees >= unlockCost;
                row.Label.text = row.Definition.DisplayName + "\n" +
                    "Demand x" + row.Definition.DemandMultiplier.ToString("0.##") + "  |  " +
                    (unlocked ? "Unlocked" : "Cost " + ChaiNumberFormatter.Rupees(unlockCost));
                row.Button.interactable = canUnlock;
                SetButtonColor(row.Button, unlocked ? Saffron : canUnlock ? Leaf : Disabled);
            }

            RefreshPrestige();
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
                    row.Definition.Branch + "  |  " + row.Definition.EffectLabel + "\n" +
                    row.Definition.Description;
                row.Button.interactable = canSpend;
                SetButtonColor(row.Button, isMaxed ? Saffron : canSpend ? Leaf : Disabled);
            }
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
                    SetStatus("Fresh cutting chai");
                    break;
                case ChaiTutorialAction.BuyFirstUpgrade:
                    if (game.TryBuyUpgrade(ChaiTutorial.FirstUpgradeId))
                    {
                        PlayPurchaseSound();
                        TriggerHaptic();
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
                SetStatus(definition.DisplayName + " upgraded");
            }
            else
            {
                SetStatus("Need a skill point");
            }

            RefreshAll();
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
                Handheld.Vibrate();
            }
        }

        private void ShowOfflineReward(OfflineReward reward)
        {
            if (offlineRewardModal == null)
            {
                return;
            }

            offlineRewardAmountText.text = ChaiNumberFormatter.Rupees(reward.RupeesEarned);
            offlineRewardDetailText.text = "Away " + FormatDuration(reward.RawSeconds) + "  |  Efficiency " + FormatPercent(game.GetOfflineEfficiency());
            offlineRewardCapText.text = reward.WasCapped ?
                "Capped at " + FormatDuration(reward.CappedSeconds) :
                "Cap " + FormatDuration(game.GetOfflineCapSeconds());
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

        private static Button CreateButton(string name, Transform parent, string text, int fontSize, Color color, float height)
        {
            GameObject buttonObject = CreateChild(name, parent);
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
    }
}
