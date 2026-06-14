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
        private bool _ruinsGenerated = false;
        private bool _palaceGenerated = false;
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
            PlayerStaminaSleepBar.SleepValue = (int)p.Sleep;
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
        private NavigationManager? _nav;

        private void InitTutorialMaps()
        {
            foreach (var loc in TutorialData.GetSimplifiedLocations())
                _state.Engine.Locations[loc.Key] = loc.Value;
            foreach (var item in TutorialData.GetSimplifiedItems())
                _state.Engine.Items[item.Key] = item.Value;
            foreach (var npc in TutorialData.GetSimplifiedNPCs())
                _state.Engine.NPCs[npc.Key] = npc.Value;

            _state.Engine.CurrentPlayer.CurrentLocationId =
                "mountain_edge";
            TimeDisplay.IsDayCounterVisible = false;

            _nav = NavigationManager.Instance;

            var random = new Random();

            var regionMap = MapGenerator.Generate(
                "unknown_region", "Unknown",
                GameMapType.Region, MapSize.Small,
                new List<(string, string, string, MapSize)>
                {
            ("mountain_edge",      "Mountain Edge",      "⛰", MapSize.Medium),
            ("random_ruins",       "Random Ruins",       "🏚", MapSize.Medium),
            ("extravagant_palace", "Extravagant Palace", "🏛", MapSize.Large)
                });

            var mountainEdgeMap = MapGenerator.Generate(
                "mountain_edge", "Mountain Edge",
                GameMapType.Location, MapSize.Small,
                new List<(string, string, string, MapSize)>
                {
            ("me_parking_lot",   "Parking Lot",     "🅿", MapSize.Small),
            ("me_empty_booth",   "Empty Booth",     "🏠", MapSize.Small),
            ("me_cluster_signs", "Cluster of Signs","🪧", MapSize.Small)
                });

            MapGenerator.AddBorderLandmarks(
                mountainEdgeMap, regionMap,
                "mountain_edge", random);

            // ── Add "Path to Nowhere" ─────────────────────
            // Special border landmark — direction hiker arrived from
            // South side, connects to Parking Lot, no exit
            MapGenerator.AddSpecialBorderLandmark(
                mountainEdgeMap,
                id: "border_path_to_nowhere",
                name: "Path to Nowhere",
                direction: Direction.South,
                connectToNodeId: "me_parking_lot",
                random);

            _nav.SetRegionMap(regionMap);
            _nav.AddLocationMap("mountain_edge", mountainEdgeMap);

            GameMap.LoadMap(mountainEdgeMap,
                "mountain_edge_map", "Mountain Edge",
                MapType.Location);

            GameMap.AddAvailableMap(new MapTab
            {
                Id = "random_ruins_map",
                Title = "Random Ruins",
                Type = MapType.Location,
                IsUnlocked = false,
                Map = null
            });

            GameMap.AddAvailableMap(new MapTab
            {
                Id = "extravagant_palace_map",
                Title = "Extravagant Palace",
                Type = MapType.Location,
                IsUnlocked = false,
                Map = null
            });

            GameMap.AddAvailableMap(new MapTab
            {
                Id = "unknown_region",
                Title = "Unknown",
                Type = MapType.Region,
                IsUnlocked = true,
                Map = regionMap
            });

            // Wire NavigationManager events
            _nav.OnConsoleMessage += (text, type) =>
                Dispatcher.Invoke(() =>
                    GameConsole.AddText(text, type));

            _nav.OnOptionsGenerated += options =>
                Dispatcher.Invoke(() =>
                    ShowNavigationOptions(options, _nav));

            _nav.OnStatEffect += effect =>
                Dispatcher.Invoke(() => ApplyStatEffect(effect));

            _nav.OnMapChanged += map =>
                Dispatcher.Invoke(() =>
                {
                    string tabId = map.Id switch
                    {
                        "unknown_region" => "unknown_region",
                        "mountain_edge" => "mountain_edge_map",
                        "random_ruins" => "random_ruins_map",
                        "extravagant_palace" => "extravagant_palace_map",
                        _ => map.Id
                    };
                    GameMap.SwitchToTab(tabId);
                    GameMap.MarkDirty();
                });

            _nav.OnTravelStart += () =>
                Dispatcher.Invoke(() => GameMap.MarkDirty());

            _nav.OnNarrativeTrigger += triggerId =>
                Dispatcher.Invoke(() =>
                {
                    if (triggerId == "arrive_random_ruins" &&
                        !_ruinsGenerated)
                    {
                        var ruinsMap = MapGenerator.Generate(
                            "random_ruins", "Random Ruins",
                            GameMapType.Location, MapSize.Small,
                            new List<(string, string, string, MapSize)>
                            {
                        ("rr_intact_house",  "Intact House",         "🏠", MapSize.Small),
                        ("rr_damaged_store", "Damaged Store",        "🏪", MapSize.Small),
                        ("rr_warehouse",     "Warehouse",            "🏭", MapSize.Medium),
                        ("rr_tower",         "Half Collapsed Tower", "🗼", MapSize.Medium)
                            });

                        MapGenerator.AddBorderLandmarks(
                            ruinsMap, regionMap,
                            "random_ruins", random);

                        _nav.AddLocationMap("random_ruins", ruinsMap);
                        _ruinsGenerated = true;

                        var tab = GameMap.GetAvailableTab(
                            "random_ruins_map");
                        if (tab != null) tab.Map = ruinsMap;
                    }
                    else if (triggerId == "arrive_extravagant_palace" &&
                             !_palaceGenerated)
                    {
                        var palaceMap = MapGenerator.Generate(
                            "extravagant_palace", "Extravagant Palace",
                            GameMapType.Location, MapSize.Medium,
                            new List<(string, string, string, MapSize)>
                            {
                        ("ep_main_hall",   "Main Hall",    "🏛", MapSize.Large),
                        ("ep_basement",    "Basement",     "⬛", MapSize.Medium),
                        ("ep_storage",     "Storage Room", "📦", MapSize.Small),
                        ("ep_kitchen",     "Kitchen",      "🍳", MapSize.Small),
                        ("ep_bedroom",     "Bedroom",      "🛏", MapSize.Small),
                        ("ep_sturdy_room", "Sturdy Room",  "⌂", MapSize.Medium)
                            });

                        MapGenerator.AddBorderLandmarks(
                            palaceMap, regionMap,
                            "extravagant_palace", random);

                        _nav.AddLocationMap("extravagant_palace", palaceMap);
                        _palaceGenerated = true;

                        var tab = GameMap.GetAvailableTab(
                            "extravagant_palace_map");
                        if (tab != null) tab.Map = palaceMap;
                    }

                    HandleTutorialTrigger(triggerId);
                });
        }

        private void StartTutorialDialogue(string firstSceneId)
        {
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
                Dispatcher.Invoke(() => ApplyStatEffect(effect));

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
                            bool slow = nextId ==
                                "intro_death_exhaustion";
                            ShowTutorialDeath(
                                nextId == "intro_death_bridge"
                                    ? "He was brave, but bravery " +
                                      "without caution is just " +
                                      "another way to die."
                                    : "He was cautious, but caution " +
                                      "without decisiveness is just " +
                                      "a slower end.",
                                checkpoint, slow);
                            return;
                        }

                        switch (nextId)
                        {
                            case "intro_mountain_edge":
                                // Mark opening as completed
                                if (!_state.Player.CompletedQuests
                                    .Contains("intro_opening"))
                                    _state.Player.CompletedQuests
                                        .Add("intro_opening");
                                OnOpeningComplete(_nav!);
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

        // Updated StartTutorial - now just orchestrates
        public void StartTutorial(string firstSceneId)
        {
            GameConsole.ClearConsole();
            StartConfiguringAnimation();

            InitTutorialMaps();

            bool openingCompleted = _state.Player.CompletedQuests
                .Contains("intro_opening");

            if (openingCompleted)
            {
                GameConsole.AddText(
                    "You have already witnessed this vision.",
                    TextType.Description,
                    onComplete: () =>
                    {
                        GameConsole.ShowOptions(
                            new List<string>
                            {
                        "Watch it again.",
                        "Skip to Mountain Edge."
                            },
                            index =>
                            {
                                if (index == 1)
                                {
                                    RevealPanel(UIPanel.Map);
                                    OnOpeningComplete(_nav!);
                                    return;
                                }
                                StartTutorialDialogue(firstSceneId);
                            });
                    });
                return;
            }

            StartTutorialDialogue(firstSceneId);
        }

        // ── Opening complete → hand off to NavigationManager ──
        private void OnOpeningComplete(NavigationManager nav)
        {
            // Reveal map
            RevealPanel(UIPanel.Map);

            // Set starting position
            nav.SetStartPosition("mountain_edge", "me_parking_lot");
        }

        // ── Apply stat effect ─────────────────────────
        private void ApplyStatEffect(StatEffect effect)
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
            if (effect.TimeSeconds != 0)
                _state.AdvanceTimeSeconds(effect.TimeSeconds);
        }

        // ── Show navigation options from NavigationManager ──
        private void ShowNavigationOptions(
            List<NavigationOption> options,
            NavigationManager nav)
        {
            var labels = options.Select(o => o.Label).ToList();

            GameConsole.ShowOptions(labels, index =>
            {
                var option = options[index];

                if (option.Type == OptionType.LookAround)
                {
                    nav.ExecuteLookAround();
                    return;
                }

                // Movement — show type choice then confirm
                ShowMovementTypeChoice(option, nav);
            });
        }

        // ── Movement type choice ──────────────────────
        private void ShowMovementTypeChoice(
            NavigationOption option,
            NavigationManager nav)
        {
            GameConsole.ShowOptions(
                new List<string>
                {
            "Move carefully.",
            "Move normally.",
            "Move quickly."
                },
                movIndex =>
                {
                    var movType = movIndex switch
                    {
                        0 => MovementType.Carefully,
                        1 => MovementType.Normally,
                        _ => MovementType.Quickly
                    };

                    var effect = MovementSystem.Calculate(
                        option.NavState,
                        option.Distance,
                        movType);

                    string confirmText = MovementSystem
                        .GetConfirmationText(
                            option.TargetNodeName ?? "",
                            option.NavState,
                            option.Distance,
                            movType);

                    GameConsole.AddText(confirmText,
                        TextType.Gameline,
                        onComplete: () =>
                        {
                            GameConsole.ShowOptions(
                                new List<string>
                                {
                            "Yes.",
                            "Let me think."
                                },
                                confirmIndex =>
                                {
                                    if (confirmIndex == 1)
                                    {
                                        nav.GenerateOptions();
                                        return;
                                    }

                                    nav.ExecuteMovement(
                                        option, movType);
                                });
                        });
                });
        }

        // ── Handle narrative triggers ─────────────────
        private void HandleTutorialTrigger(string triggerId)
        {
            switch (triggerId)
            {
                case "look_around_first":
                    RevealPanel(UIPanel.Clock);
                    RevealPanel(UIPanel.Stamina);
                    _activeTutorialDialogue?
                        .GoToScene("intro_look_around_reveal");
                    break;

                case "arrive_random_ruins":
                    GameMap.SwitchToTab("random_ruins_map");
                    break;

                case "arrive_extravagant_palace":
                    GameMap.SwitchToTab("extravagant_palace_map");
                    break;
            }
        }
    }
}