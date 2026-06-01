using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LostInAForgottenCity.Controls;
using LostInAForgottenCity.Engine;

namespace LostInAForgottenCity.Views
{
    public partial class GameView : UserControl
    {
        // ── Fields ───────────────────────────────
        private GameState _state = GameState.Instance;
        private DialogueEngine? _activeTutorialDialogue;
        private DispatcherTimer _configuringTimer = new();
        private int _configuringDots = 0;
        private const int MaxConfiguringDots = 5;
        private List<(Grid overlay, TextBlock text)> _overlays = new();

        private static readonly Dictionary<string, string>
            _deathCheckpoints = new()
        {
            { "intro_death_bridge",     "intro_towards_city" },
            { "intro_death_exhaustion", "intro_towards_city" },
        };

        // ── Constructor ──────────────────────────
        public GameView()
        {
            InitializeComponent();
            _state.OnStatsChanged += UpdateStats;
            _state.OnTimeChanged += UpdateTime;
            _state.OnInventoryChanged += UpdateInventory;
            UpdateStats();
            UpdateTime();
            UpdateInventory();
            LoadTestConsole();
        }

        // ── Stat updates ─────────────────────────
        private void UpdateStats()
        {
            var p = _state.Player;
            PlayerSanityBar.Value = p.Sanity;
            PlayerSubconsciousBar.Value = p.Subconscious;
            PlayerSubconsciousBar.MaxSlots = p.MaxSubconscious;
            PlayerDangerIndicator.Level = (DangerLevel)p.Danger;
            PlayerDangerIndicator.StatusEffect = p.StatusEffect;
            PlayerSoulBar.Value = p.Soul;
            PlayerSoulBar.MaxValue = p.MaxSoul;
            PlayerResistanceBar.Value = p.Resistance;
            PlayerResistanceBar.MaxValue = p.MaxResistance;
            PlayerHpBar.Value = p.HP;
            PlayerHpBar.MaxSegments = p.MaxHP;
            PlayerStaminaSleepBar.Value = p.Stamina;
            PlayerStaminaSleepBar.MaxSegments = p.MaxStamina;
            PlayerStaminaSleepBar.SleepValue = p.Sleep;
            PlayerStaminaSleepBar.IsSleepVisible = !p.IsSunnyDay;
        }

        private void UpdateTime()
        {
            var p = _state.Player;
            TimeDisplay.Day = p.Day;
            TimeDisplay.Hour = p.Hour;
            TimeDisplay.Minute = p.Minute;
            TimeDisplay.Date = p.Date;
            TimeDisplay.Weather = p.Weather;
            TimeDisplay.Temperature = p.Temperature;
            TimeDisplay.FeelsLike = p.FeelsLike;
            TimeDisplay.Hazard = p.Hazard;
            TimeDisplay.IsCurseVisible = p.IsHardMode
                && !string.IsNullOrEmpty(p.CurseText);
            TimeDisplay.CurseTextContent = p.CurseText;
        }

        private void UpdateInventory()
        {
            var p = _state.Player;
            RelicCount.Text = p.Relics.ToString();
            Inventory.SlotCount = p.MaxInventorySlots;
        }

        // ── Menu buttons ─────────────────────────
        private void ObjectivesBtn_Click(object sender, RoutedEventArgs e)
            => ShowOverlay("OBJECTIVES", BuildObjectivesContent());
        private void CollectionsBtn_Click(object sender, RoutedEventArgs e)
            => ShowOverlay("COLLECTIONS", BuildCollectionsContent());
        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
            => ShowOverlay("HISTORY", BuildHistoryContent());
        private void PauseBtn_Click(object sender, RoutedEventArgs e)
            => ShowOverlay("PAUSED", BuildPauseContent(), showClose: false);
        private void ClockBtn_Click(object sender, RoutedEventArgs e) { }
        private void WeatherBtn_Click(object sender, RoutedEventArgs e) { }

        // ── Overlay system ───────────────────────
        private void ShowOverlay(string title, UIElement content,
            bool showClose = true)
        {
            OverlayTitle.Text = title;
            OverlayContent.Content = content;
            OverlayCloseBtn.Visibility = showClose
                ? Visibility.Visible : Visibility.Collapsed;
            GameOverlay.Visibility = Visibility.Visible;
        }

        private void CloseOverlay_Click(object sender, RoutedEventArgs e)
            => GameOverlay.Visibility = Visibility.Collapsed;

        private UIElement BuildObjectivesContent()
        {
            var panel = new StackPanel();
            var p = _state.Player;
            if (p.ActiveQuests.Count == 0)
                panel.Children.Add(MakeOverlayText(
                    "No active objectives.", "#6a6a5a"));
            else
            {
                panel.Children.Add(MakeOverlayText("ACTIVE", "#c8a840"));
                foreach (var q in p.ActiveQuests)
                    panel.Children.Add(MakeOverlayText($"  ★ {q}", "#c8c8b0"));
            }
            if (p.CompletedQuests.Count > 0)
            {
                panel.Children.Add(MakeOverlayText("\nCOMPLETED", "#6a8a6a"));
                foreach (var q in p.CompletedQuests)
                    panel.Children.Add(MakeOverlayText($"  ✓ {q}", "#4a6a4a"));
            }
            return panel;
        }

        private UIElement BuildCollectionsContent()
        {
            var panel = new StackPanel();
            var p = _state.Player;
            if (p.Collections.Count == 0)
                panel.Children.Add(MakeOverlayText(
                    "No collections found yet.", "#6a6a5a"));
            else
                foreach (var c in p.Collections)
                    panel.Children.Add(MakeOverlayText($"  ◆ {c}", "#c8a840"));
            return panel;
        }

        private UIElement BuildHistoryContent()
        {
            var panel = new StackPanel();
            var p = _state.Player;
            if (p.NarrativeHistory.Count == 0)
                panel.Children.Add(MakeOverlayText("No history yet.", "#6a6a5a"));
            else
            {
                int start = Math.Max(0, p.NarrativeHistory.Count - 20);
                for (int i = start; i < p.NarrativeHistory.Count; i++)
                    panel.Children.Add(MakeOverlayText(
                        p.NarrativeHistory[i], "#8a8a7a"));
            }
            return panel;
        }

        private UIElement BuildPauseContent()
        {
            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var resumeBtn = new Button
            {
                Content = "RESUME",
                Style = (Style)Application.Current.Resources["ConsoleButton"],
                Margin = new Thickness(0, 4, 0, 4)
            };
            resumeBtn.Click += CloseOverlay_Click;
            var menuBtn = new Button
            {
                Content = "MAIN MENU",
                Style = (Style)Application.Current.Resources["ConsoleButton"],
                Margin = new Thickness(0, 4, 0, 4)
            };
            menuBtn.Click += (s, e) =>
            {
                GameOverlay.Visibility = Visibility.Collapsed;
                MainWindow.Instance?.NavigateTo(new MenuView());
            };
            panel.Children.Add(resumeBtn);
            panel.Children.Add(menuBtn);
            return panel;
        }

        private TextBlock MakeOverlayText(string text, string color)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new System.Windows.Media
                    .FontFamily("Courier New"),
                FontSize = 13,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media
                    .ColorConverter.ConvertFromString(color)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            };
        }

        // ── Configuring animation ─────────────────
        public void StartConfiguringAnimation()
        {
            _overlays = new List<(Grid, TextBlock)>
            {
                (MindOverlay,      MindOverlayText),
                (SpiritOverlay,    SpiritOverlayText),
                (BodyOverlay,      BodyOverlayText),
                (TimeOverlay,      TimeOverlayText),
                (InventoryOverlay, InventoryOverlayText),
                (MapOverlay,       MapOverlayText)
            };

            foreach (var (overlay, _) in _overlays)
                overlay.Visibility = Visibility.Visible;

            _configuringDots = 0;
            _configuringTimer.Interval = TimeSpan.FromMilliseconds(400);
            _configuringTimer.Tick += ConfiguringTick;
            _configuringTimer.Start();
        }

        private void ConfiguringTick(object? sender, EventArgs e)
        {
            _configuringDots++;
            string dots = string.Concat(
                Enumerable.Repeat(" .", _configuringDots));
            string text = $"[ configuring{dots} ]";
            foreach (var (_, textBlock) in _overlays)
                textBlock.Text = text;
            if (_configuringDots >= MaxConfiguringDots)
                _configuringTimer.Stop();
        }

        // ── Panel reveal ─────────────────────────
        public enum UIPanel
        {
            // Box-level (initial configuring state)
            Mind, Spirit, Body, Map, Inventory,
            // Time — progressive
            Clock,    // removes TimeOverlay, WeatherOverlay stays
            Weather,  // removes WeatherOverlay inside TimeBox
            // Sub-elements within MIND
            Sanity, Subconscious, Danger,
            // Sub-elements within SPIRIT
            Soul, Resistance,
            // Sub-elements within BODY
            Hp, Stamina
        }

        public void RevealPanel(UIPanel panel)
        {
            switch (panel)
            {
                // ── Box level ──
                case UIPanel.Mind:
                    MindOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Spirit:
                    SpiritOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Body:
                    // Reveal everything in body
                    BodyOverlay.Visibility = Visibility.Collapsed;
                    HpBarOverlay.Visibility = Visibility.Collapsed;
                    StaminaBarOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Map:
                    MapOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Inventory:
                    InventoryOverlay.Visibility = Visibility.Collapsed;
                    break;

                // ── Time progressive ──
                case UIPanel.Clock:
                    // Remove full time overlay
                    // WeatherOverlay inside TimeBox stays visible
                    TimeOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Weather:
                    TimeDisplay.RevealWeather();
                    break;

                // ── MIND sub-elements ──
                case UIPanel.Sanity:
                    SanityOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Subconscious:
                    SubconsciousOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Danger:
                    DangerOverlay.Visibility = Visibility.Collapsed;
                    break;

                // ── SPIRIT sub-elements ──
                case UIPanel.Soul:
                    SoulBarOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Resistance:
                    ResistanceBarOverlay.Visibility = Visibility.Collapsed;
                    break;

                // ── BODY sub-elements ──
                case UIPanel.Stamina:
                    // Stamina revealed first:
                    // Remove full body overlay
                    BodyOverlay.Visibility = Visibility.Collapsed;
                    // HP still hidden
                    HpBarOverlay.Visibility = Visibility.Visible;
                    // Stamina visible
                    StaminaBarOverlay.Visibility = Visibility.Collapsed;
                    break;
                case UIPanel.Hp:
                    HpBarOverlay.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        // ── Map disable (for main game) ───────────
        public void SetMapDisabled(bool disabled)
        {
            MapOverlay.Visibility = disabled
                ? Visibility.Visible : Visibility.Collapsed;
            MapOverlayText.Text = disabled
                ? "[ disabled ]" : "[ configuring ]";
        }

        // ── Tutorial death ────────────────────────
        private void ShowTutorialDeath(string comment,
            string retrySceneId, bool slow = false)
        {
            if (slow)
            {
                var pauseTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(2000)
                };
                pauseTimer.Tick += (s, e) =>
                {
                    pauseTimer.Stop();
                    GameConsole.SetTypewriterSpeed(18);
                    ShowOverlay("YOU FAILED",
                        BuildDeathContent(comment, retrySceneId),
                        showClose: false);
                };
                pauseTimer.Start();
            }
            else
            {
                ShowOverlay("YOU FAILED",
                    BuildDeathContent(comment, retrySceneId),
                    showClose: false);
            }
        }

        private UIElement BuildDeathContent(string comment,
            string retrySceneId)
        {
            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(MakeOverlayText(
                $"\"{comment},", "#c8a8d0"));
            panel.Children.Add(MakeOverlayText(
                "but unfortunately this is not how it happened.\"",
                "#c8a8d0"));
            panel.Children.Add(MakeOverlayText(
                "— Fortuneteller", "#8a6a8a"));
            var tryAgainBtn = new Button
            {
                Content = "TRY AGAIN",
                Style = (Style)Application.Current.Resources["ConsoleButton"],
                Margin = new Thickness(0, 16, 0, 4)
            };
            tryAgainBtn.Click += (s, e) =>
            {
                GameOverlay.Visibility = Visibility.Collapsed;
                GameConsole.ClearConsole();
                GameConsole.SetTypewriterSpeed(18);
                _activeTutorialDialogue?.GoToScene(retrySceneId);
            };
            panel.Children.Add(tryAgainBtn);
            return panel;
        }

        // ── Tutorial start ────────────────────────
        public void StartTutorial(string firstSceneId)
        {
            GameConsole.ClearConsole();
            StartConfiguringAnimation();

            foreach (var loc in TutorialData.GetSimplifiedLocations())
                _state.Engine.Locations[loc.Key] = loc.Value;
            foreach (var item in TutorialData.GetSimplifiedItems())
                _state.Engine.Items[item.Key] = item.Value;
            foreach (var npc in TutorialData.GetSimplifiedNPCs())
                _state.Engine.NPCs[npc.Key] = npc.Value;

            _state.Engine.CurrentPlayer.CurrentLocationId =
                "mountain_edge";
            TimeDisplay.IsDayCounterVisible = false;

            // ── Build Unknown region map ──────────────
            var regionNodes = new List<Controls.MapNode>
    {
        // South (index 0)
        new() {
            Id = "mountain_edge",
            Name = "Mountain Edge",
            BaseIcon = "⛰",
            State = Controls.LocationState.Visited,
            Special = Controls.SpecialMarker.CurrentLocation,
            Type = Controls.LocationType.Normal
        },
        // Middle (index 1)
        new() {
            Id = "random_ruins",
            Name = "Random Ruins",
            BaseIcon = "🏚",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        // North (index 2)
        new() {
            Id = "extravagant_palace",
            Name = "Extravagant Palace",
            BaseIcon = "🏛",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        }
    };

            // Generate random layout (south→north)
            Controls.MapPanel.GenerateLayout(regionNodes, 280, 300);

            var regionConnections = new List<Controls.MapConnection>
    {
        new() {
            FromId = "mountain_edge",
            ToId = "random_ruins"
        },
        new() {
            FromId = "random_ruins",
            ToId = "extravagant_palace"
        }
    };

            // ── Build location maps ───────────────────

            // Mountain Edge landmarks
            var mountainEdgeNodes = new List<Controls.MapNode>
    {
        new() {
            Id = "me_parking_lot",
            Name = "Parking Lot",
            BaseIcon = "🅿",
            State = Controls.LocationState.Visited,
            Special = Controls.SpecialMarker.CurrentLocation,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "me_empty_booth",
            Name = "Empty Booth",
            BaseIcon = "🏠",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "me_cluster_signs",
            Name = "Cluster of Signs",
            BaseIcon = "🪧",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        }
    };
            Controls.MapPanel.GenerateLayout(
                mountainEdgeNodes, 280, 300);

            var mountainEdgeConnections = new List<Controls.MapConnection>
    {
        new() { FromId = "me_parking_lot",
                ToId = "me_empty_booth" },
        new() { FromId = "me_empty_booth",
                ToId = "me_cluster_signs" }
    };

            // Random Ruins landmarks
            var ruinsNodes = new List<Controls.MapNode>
    {
        new() {
            Id = "rr_intact_house",
            Name = "Intact House",
            BaseIcon = "🏠",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "rr_damaged_store",
            Name = "Damaged Store",
            BaseIcon = "🏪",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "rr_warehouse",
            Name = "Warehouse",
            BaseIcon = "🏭",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "rr_tower",
            Name = "Half Collapsed Tower",
            BaseIcon = "🗼",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        }
    };
            Controls.MapPanel.GenerateLayout(ruinsNodes, 280, 300);

            var ruinsConnections = new List<Controls.MapConnection>
    {
        new() { FromId = "rr_intact_house",
                ToId = "rr_damaged_store" },
        new() { FromId = "rr_damaged_store",
                ToId = "rr_warehouse" },
        new() { FromId = "rr_warehouse",
                ToId = "rr_tower" },
        new() { FromId = "rr_intact_house",
                ToId = "rr_tower" }
    };

            // Extravagant Palace landmarks
            var palaceNodes = new List<Controls.MapNode>
    {
        new() {
            Id = "ep_main_hall",
            Name = "Main Hall",
            BaseIcon = "🏛",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "ep_basement",
            Name = "Basement",
            BaseIcon = "⬛",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "ep_storage",
            Name = "Storage Room",
            BaseIcon = "📦",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "ep_kitchen",
            Name = "Kitchen",
            BaseIcon = "🍳",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "ep_bedroom",
            Name = "Bedroom",
            BaseIcon = "🛏",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal
        },
        new() {
            Id = "ep_sturdy_room",
            Name = "Sturdy Room",
            BaseIcon = "⌂",
            State = Controls.LocationState.Undiscovered,
            Type = Controls.LocationType.Normal,
            HasDiscoveredSafeRoom = false
        }
    };
            Controls.MapPanel.GenerateLayout(palaceNodes, 280, 300);

            var palaceConnections = new List<Controls.MapConnection>
    {
        new() { FromId = "ep_main_hall",
                ToId = "ep_basement" },
        new() { FromId = "ep_main_hall",
                ToId = "ep_storage" },
        new() { FromId = "ep_storage",
                ToId = "ep_kitchen" },
        new() { FromId = "ep_kitchen",
                ToId = "ep_bedroom" },
        new() { FromId = "ep_bedroom",
                ToId = "ep_sturdy_room" }
    };

            // ── Load region map as current ────────────
            GameMap.LoadMap(regionNodes, regionConnections,
                "unknown_region", "Unknown",
                Controls.MapType.Region,
                "mountain_edge");

            // ── Add location maps as available ────────
            GameMap.AddAvailableMap(new Controls.MapTab
            {
                Id = "mountain_edge_map",
                Title = "Mountain Edge",
                Type = Controls.MapType.Location,
                IsUnlocked = true,
                Nodes = mountainEdgeNodes,
                Connections = mountainEdgeConnections
            });

            GameMap.AddAvailableMap(new Controls.MapTab
            {
                Id = "random_ruins_map",
                Title = "Random Ruins",
                Type = Controls.MapType.Location,
                IsUnlocked = false,  // unlocked when visited
                Nodes = ruinsNodes,
                Connections = ruinsConnections
            });

            GameMap.AddAvailableMap(new Controls.MapTab
            {
                Id = "extravagant_palace_map",
                Title = "Extravagant Palace",
                Type = Controls.MapType.Location,
                IsUnlocked = false,  // unlocked when visited
                Nodes = palaceNodes,
                Connections = palaceConnections
            });

            // Mountain Edge — player arrived from south
            GameMap.SetBorderEntry(
                "mountain_edge_map",
                "S",           // arrived from south
                0.5,           // center of south border
                GameMap.FindNearestLandmark(
                    "mountain_edge_map", "S"),
                isPlayerHere: true);

            // Random Ruins — will arrive from south (from Mountain Edge)
            GameMap.SetBorderEntry(
                "random_ruins_map",
                "S",
                0.5,
                GameMap.FindNearestLandmark(
                    "random_ruins_map", "S"),
                isPlayerHere: false);

            // Extravagant Palace — will arrive from south
            GameMap.SetBorderEntry(
                "extravagant_palace_map",
                "S",
                0.5,
                GameMap.FindNearestLandmark(
                    "extravagant_palace_map", "S"),
                isPlayerHere: false);

            // ── Wire dialogue ─────────────────────────
            var dialogue = new DialogueEngine();
            dialogue.LoadDialogue(DialogueData.GetTutorialDialogue());
            _activeTutorialDialogue = dialogue;

            string lastSpeaker = "";

            dialogue.OnLine += (line, next) =>
            {
                Dispatcher.Invoke(() =>
                {
                    string displayText;
                    TextType type;

                    if (line.IsGameline)
                    {
                        displayText = line.Text;
                        type = TextType.Gameline;
                        lastSpeaker = "";
                    }
                    else if (line.IsNarration ||
                             string.IsNullOrEmpty(line.Speaker))
                    {
                        displayText = line.Text;
                        type = TextType.Description;
                        lastSpeaker = "";
                    }
                    else
                    {
                        displayText = line.Speaker != lastSpeaker
                            ? $"{line.Speaker}: {line.Text}"
                            : line.Text;
                        lastSpeaker = line.Speaker;
                        type = TextType.Dialogue;
                    }

                    GameConsole.AddText(displayText, type,
                        onComplete: () => next());
                });
            };

            dialogue.OnEffectApplied += effect =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (effect.Stamina != 0)
                        _state.ModifyStamina(effect.Stamina);
                    if (effect.Sleep != 0)
                        _state.ModifySleep(effect.Sleep);
                    if (effect.Sanity != 0)
                        _state.ModifySanity(effect.Sanity);
                    if (effect.HP != 0)
                        _state.ModifyHP(effect.HP);
                    if (effect.Soul != 0)
                        _state.ModifySoul(effect.Soul);
                    if (effect.Resistance != 0)
                        _state.ModifyResistance(effect.Resistance);
                    if (effect.TimeMinutes != 0)
                        _state.AdvanceTime(effect.TimeMinutes);
                });
            };

            dialogue.OnChoices += choices =>
            {
                Dispatcher.Invoke(() =>
                {
                    var options = new List<string>();
                    foreach (var c in choices)
                        options.Add(c.Text);

                    GameConsole.ShowOptions(options, index =>
                    {
                        string nextId = choices[index].NextSceneId;
                        var effect = choices[index].Effect;

                        if (_deathCheckpoints.TryGetValue(
                            nextId, out string? checkpoint))
                        {
                            bool slow = nextId == "intro_death_exhaustion";
                            ShowTutorialDeath(
                                nextId == "intro_death_bridge"
                                    ? "He was brave, but bravery without " +
                                      "caution is just another way to die."
                                    : "He was cautious, but caution without " +
                                      "decisiveness is just a slower end.",
                                checkpoint, slow);
                            return;
                        }

                        switch (nextId)
                        {
                            case "intro_mountain_edge":
                                dialogue.GoToScene(
                                    "intro_mountain_edge_arrival", effect);
                                break;

                            // Movement choices — start travel animation
                            case "intro_move_parking_lot":
                                StartTravelAnimation(
                                    "mountain_edge", "mountain_edge",
                                    "me_parking_lot",
                                    () => dialogue.GoToScene(nextId, effect));
                                break;

                            case "intro_move_booth":
                                StartTravelAnimation(
                                    "mountain_edge", "mountain_edge",
                                    "me_empty_booth",
                                    () => dialogue.GoToScene(nextId, effect));
                                break;

                            case "intro_move_signs":
                                StartTravelAnimation(
                                    "mountain_edge", "mountain_edge",
                                    "me_cluster_signs",
                                    () => dialogue.GoToScene(nextId, effect));
                                break;

                            default:
                                dialogue.GoToScene(nextId, effect);
                                break;
                        }
                    });
                });
            };

            dialogue.OnSceneStart += sceneId =>
            {
                Dispatcher.Invoke(() =>
                {
                    switch (sceneId)
                    {
                        case "intro_mountain_edge_arrival":
                            RevealPanel(UIPanel.Map);
                            break;
                        case "intro_look_around_reveal":
                            RevealPanel(UIPanel.Clock);
                            RevealPanel(UIPanel.Stamina);
                            break;
                    }
                });
            };

            dialogue.OnAutoNext += id =>
                Dispatcher.Invoke(() => dialogue.StartScene(id));

            dialogue.OnSceneComplete += () =>
                Dispatcher.Invoke(() =>
                    GameConsole.AddText("The vision fades.",
                        TextType.Description));

            dialogue.StartScene(firstSceneId);
        }

        // ── Travel animation ──────────────────────────
        private void StartTravelAnimation(
            string regionFromId, string locationId,
            string landmarkToId, Action onComplete)
        {
            // Switch to location map
            var locationTabId = $"{locationId}_map";
            var locationTab = GameMap
                .GetAvailableTab(locationTabId);

            if (locationTab != null)
            {
                GameMap.SwitchToTab(locationTabId);
                GameMap.StartTravel(
                    GetCurrentLandmark(locationId),
                    landmarkToId);
            }

            // Show travel message in console
            ShowTravelMessage(onComplete);
        }

        private void ShowTravelMessage(Action onComplete)
        {
            var travelTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            int dots = 0;
            travelTimer.Tick += (s, e) =>
            {
                dots++;
                string msg = "Traveling" + new string('.', dots);
                // Update last console line
                if (dots >= 6)
                {
                    travelTimer.Stop();
                    onComplete();
                }
            };

            GameConsole.AddText("Traveling . . . . . .",
                TextType.Description,
                onComplete: () => { });
            travelTimer.Start();
        }

        private string GetCurrentLandmark(string locationId)
        {
            // Returns current landmark within a location
            return locationId switch
            {
                "mountain_edge" => "me_parking_lot",
                _ => ""
            };
        }

        // ── Test console ──────────────────────────
        private void LoadTestConsole()
        {
            GameConsole.AddText(
                "You stand at the edge of the city. The fog is " +
                "thick today, swallowing the outlines of the " +
                "buildings ahead. Somewhere in the distance, " +
                "something moves.",
                TextType.Description,
                onComplete: () =>
                {
                    GameConsole.AddText(
                        "Stay close to the walls and don't make noise.",
                        TextType.Gameline);
                    GameConsole.AddSeparator();
                    GameConsole.ShowOptions(
                        new List<string>
                        {
                            "Move toward the city",
                            "Examine the surroundings",
                            "Check your belongings",
                            "Stay and listen"
                        },
                        index =>
                        {
                            string response = index switch
                            {
                                0 => "You take your first steps into the fog...",
                                1 => "You scan the area carefully.",
                                2 => "You check what you're carrying.",
                                _ => "You stand still and listen."
                            };
                            _state.AddToHistory(response);
                            GameConsole.AddText(response, TextType.Description);
                            _state.ModifyStamina(-1);
                            _state.AdvanceTime(15);
                        });
                });
        }
    }
}