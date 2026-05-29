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

        // Checkpoint mapping — death scene → retry scene
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
                FontFamily = new System.Windows.Media.FontFamily("Courier New"),
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
                (MindOverlay, MindOverlayText),
                (SpiritOverlay, SpiritOverlayText),
                (BodyOverlay, BodyOverlayText),
                (TimeOverlay, TimeOverlayText),
                (InventoryOverlay, InventoryOverlayText),
                (MapOverlay, MapOverlayText)
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
            string dots = string.Concat(Enumerable.Repeat(" .", _configuringDots));
            string text = $"[ configuring{dots} ]";

            foreach (var (_, textBlock) in _overlays)
                textBlock.Text = text;

            if (_configuringDots >= MaxConfiguringDots)
            {
                _configuringTimer.Stop();
            }
        }

        // ── Panel reveal ─────────────────────────
        public enum UIPanel
        {
            Mind, Spirit, Body, Map, Time, Inventory,
            Clock, Weather, Stamina
        }

        public void RevealPanel(UIPanel panel)
        {
            switch (panel)
            {
                case UIPanel.Mind:
                    MindOverlay.Visibility = Visibility.Collapsed; break;
                case UIPanel.Spirit:
                    SpiritOverlay.Visibility = Visibility.Collapsed; break;
                case UIPanel.Body:
                    BodyOverlay.Visibility = Visibility.Collapsed; break;
                case UIPanel.Map:
                    MapOverlay.Visibility = Visibility.Collapsed; break;
                case UIPanel.Time:
                    TimeOverlay.Visibility = Visibility.Collapsed; break;
                case UIPanel.Inventory:
                    InventoryOverlay.Visibility = Visibility.Collapsed; break;
                case UIPanel.Clock:
                    TimeDisplay.RevealClock(); break;
                case UIPanel.Weather:
                    TimeDisplay.RevealWeather(); break;
                case UIPanel.Stamina:
                    // Reveal only stamina part of BODY
                    // Body overlay stays but stamina shows
                    PlayerStaminaSleepBar.Visibility = Visibility.Visible;
                    break;
            }
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

            _state.Engine.CurrentPlayer.CurrentLocationId = "unknown_ruins";

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

                        // Check if it's a death scene
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
                                checkpoint,
                                slow);
                            return;
                        }

                        switch (nextId)
                        {
                            case "intro_mountain_edge":
                                RevealPanel(UIPanel.Map);
                                dialogue.GoToScene("intro_mountain_edge_arrival");
                                break;
                            default:
                                dialogue.GoToScene(nextId);
                                break;
                        }
                    });
                });
            };

            dialogue.OnAutoNext += id =>
                Dispatcher.Invoke(() =>
                {
                    switch (id)
                    {
                        case "tut_movement": RevealPanel(UIPanel.Map); break;
                        case "tut_time": RevealPanel(UIPanel.Time); break;
                        case "tut_sanity": RevealPanel(UIPanel.Mind); break;
                        case "tut_items": RevealPanel(UIPanel.Inventory); break;
                        case "tut_spirit": RevealPanel(UIPanel.Spirit); break;
                        case "tut_body": RevealPanel(UIPanel.Body); break;
                        case "intro_look_around_reveal":
                            RevealPanel(UIPanel.Clock);
                            RevealPanel(UIPanel.Stamina);
                            dialogue.StartScene(id);
                            break;
                    }
                    dialogue.StartScene(id);
                });
                
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

            dialogue.OnSceneComplete += () =>
                Dispatcher.Invoke(() =>
                    GameConsole.AddText("The vision fades.",
                        TextType.Description));

            dialogue.StartScene(firstSceneId);
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