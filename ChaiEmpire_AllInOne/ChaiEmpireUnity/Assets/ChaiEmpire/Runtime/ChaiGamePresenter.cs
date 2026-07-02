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

        private static readonly Color Background = new Color(0.05f, 0.16f, 0.17f);
        private static readonly Color Panel = new Color(0.96f, 0.93f, 0.84f);
        private static readonly Color Ink = new Color(0.08f, 0.11f, 0.12f);
        private static readonly Color Saffron = new Color(0.93f, 0.43f, 0.16f);
        private static readonly Color Teal = new Color(0.07f, 0.43f, 0.43f);
        private static readonly Color Leaf = new Color(0.18f, 0.50f, 0.31f);
        private static readonly Color Rose = new Color(0.65f, 0.17f, 0.27f);
        private static readonly Color Disabled = new Color(0.45f, 0.48f, 0.48f);

        private readonly List<UpgradeRow> upgradeRows = new List<UpgradeRow>();
        private readonly List<LocationRow> locationRows = new List<LocationRow>();

        private ChaiContent content;
        private ChaiGame game;
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
        private GameObject offlineRewardModal;
        private Text offlineRewardAmountText;
        private Text offlineRewardDetailText;
        private Text offlineRewardCapText;
        private Text rushText;
        private Text statusText;
        private Text prestigeText;
        private Button rushButton;
        private Button resetSaveButton;
        private Text resetSaveLabel;
        private float refreshTimer;
        private float saveTimer;
        private float statusTimer;
        private float resetConfirmTimer;
        private bool resetSaveArmed;

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
            claim.onClick.AddListener(HideOfflineReward);

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
                game.TapKettle();
                SetStatus("Fresh cutting chai");
                RefreshAll();
            });

            Button customers = CreateButton("Customer Queue", actions.transform, "Serve Queue", 34, Leaf, 76);
            customers.onClick.AddListener(() =>
            {
                game.TapCustomerQueue();
                SetStatus("Queue served");
                RefreshAll();
            });

            rushButton = CreateButton("Rush Hour", actions.transform, "Rush Hour", 34, Rose, 76);
            rushButton.onClick.AddListener(() =>
            {
                if (game.TryTriggerRushHour())
                {
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
                    if (game.TryBuyUpgrade(captured.Id))
                    {
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
                    if (game.TryUnlockLocation(captured.Id))
                    {
                        SetStatus(captured.DisplayName + " unlocked");
                        RefreshAll();
                    }
                });
                locationRows.Add(new LocationRow(captured, button, label));
            }
        }

        private void BuildPrestige(Transform parent)
        {
            GameObject section = CreatePanel("Prestige", parent, new Color(0.96f, 0.92f, 0.97f), 180);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 8;

            Text title = CreateText("Prestige Title", section.transform, "Secret Masala", 34, Rose, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            prestigeText = CreateText("Prestige Preview", section.transform, string.Empty, 24, Ink, TextAnchor.MiddleLeft);
            statusText = CreateText("Status", section.transform, string.Empty, 24, Rose, TextAnchor.MiddleLeft);
        }

        private void BuildSettings(Transform parent)
        {
            GameObject section = CreatePanel("Settings", parent, new Color(0.93f, 0.94f, 0.89f), 170);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 12;

            Text title = CreateText("Settings Title", section.transform, "Settings", 34, Ink, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;

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
            locationText.text = GetCurrentLocationName() + "  |  Demand x" + game.GetDemandMultiplier().ToString("0.##");

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
                BigDouble cost = row.Definition.GetCost(level);
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
                bool canUnlock = !unlocked && game.State.Rupees >= row.Definition.UnlockCost;
                row.Label.text = row.Definition.DisplayName + "\n" +
                    "Demand x" + row.Definition.DemandMultiplier.ToString("0.##") + "  |  " +
                    (unlocked ? "Unlocked" : "Cost " + ChaiNumberFormatter.Rupees(row.Definition.UnlockCost));
                row.Button.interactable = canUnlock;
                SetButtonColor(row.Button, unlocked ? Saffron : canUnlock ? Leaf : Disabled);
            }

            PrestigePreview preview = game.GetPrestigePreview();
            prestigeText.text = preview.Message + "\nProjected legacy: " + ChaiNumberFormatter.Compact(preview.ProjectedMasalaLegacy);
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

            resetSaveLabel.text = resetSaveArmed ? "Confirm Reset" : "Reset Save";
            SetButtonColor(resetSaveButton, resetSaveArmed ? Rose : Teal);
        }

        private void HandleTutorialPrimary()
        {
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
                        SetStatus("Strong Tea Leaves upgraded");
                    }
                    break;
            }

            RefreshAll();
        }

        private void HandleResetSave()
        {
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

        private void ShowOfflineReward(OfflineReward reward)
        {
            if (offlineRewardModal == null)
            {
                return;
            }

            offlineRewardAmountText.text = ChaiNumberFormatter.Rupees(reward.RupeesEarned);
            offlineRewardDetailText.text = "Away " + FormatDuration(reward.RawSeconds) + "  |  Efficiency " + FormatPercent(content.OfflineEfficiency);
            offlineRewardCapText.text = reward.WasCapped ?
                "Capped at " + FormatDuration(reward.CappedSeconds) :
                "Cap " + FormatDuration(content.OfflineCapSeconds);
            offlineRewardModal.SetActive(true);
        }

        private void HideOfflineReward()
        {
            if (offlineRewardModal != null)
            {
                offlineRewardModal.SetActive(false);
            }
        }

        private string GetCurrentLocationName()
        {
            LocationDefinition current = content.Locations[0];
            foreach (LocationDefinition location in content.Locations)
            {
                if (game.State.IsLocationUnlocked(location.Id) && location.DemandMultiplier >= current.DemandMultiplier)
                {
                    current = location;
                }
            }

            return current.DisplayName;
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
    }
}
