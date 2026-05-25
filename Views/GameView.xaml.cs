using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LostInAForgottenCity.Controls;
using LostInAForgottenCity.Engine;
using LostInAForgottenCity.Views;

namespace LostInAForgottenCity.Views
{
    public partial class GameView : UserControl
    {
        private GameState _state = GameState.Instance;

        public GameView()
        {
            InitializeComponent();

            // Subscribe to state changes
            _state.OnStatsChanged += UpdateStats;
            _state.OnTimeChanged += UpdateTime;
            _state.OnInventoryChanged += UpdateInventory;

            // Initial UI update
            UpdateStats();
            UpdateTime();
            UpdateInventory();

            LoadTestConsole();
        }

        // ── Stat updates ─────────────────────────

        private void UpdateStats()
        {
            var p = _state.Player;

            // MIND
            PlayerSanityBar.Value = p.Sanity;
            PlayerSubconsciousBar.Value = p.Subconscious;
            PlayerSubconsciousBar.MaxSlots = p.MaxSubconscious;
            PlayerDangerIndicator.Level = (DangerLevel)p.Danger;
            PlayerDangerIndicator.StatusEffect = p.StatusEffect;

            // SPIRIT
            PlayerSoulBar.Value = p.Soul;
            PlayerSoulBar.MaxValue = p.MaxSoul;
            PlayerResistanceBar.Value = p.Resistance;
            PlayerResistanceBar.MaxValue = p.MaxResistance;

            // BODY
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

        private void ObjectivesBtn_Click(object sender,
            RoutedEventArgs e)
        {
            ShowOverlay("OBJECTIVES", BuildObjectivesContent());
        }

        private void CollectionsBtn_Click(object sender,
            RoutedEventArgs e)
        {
            ShowOverlay("COLLECTIONS", BuildCollectionsContent());
        }

        private void HistoryBtn_Click(object sender,
            RoutedEventArgs e)
        {
            ShowOverlay("HISTORY", BuildHistoryContent());
        }

        private void PauseBtn_Click(object sender,
            RoutedEventArgs e)
        {
            ShowOverlay("PAUSED", BuildPauseContent(), showClose: false);
        }

        // ── Overlay system ───────────────────────

        private void ShowOverlay(string title, UIElement content,bool showClose = true)
        {
            OverlayTitle.Text = title;
            OverlayContent.Content = content;
            OverlayCloseBtn.Visibility = showClose
                ? Visibility.Visible : Visibility.Collapsed;
            GameOverlay.Visibility = Visibility.Visible;
        }

        private void CloseOverlay_Click(object sender,
            RoutedEventArgs e)
        {
            GameOverlay.Visibility = Visibility.Collapsed;
        }

        private UIElement BuildObjectivesContent()
        {
            var panel = new StackPanel();
            var p = _state.Player;

            if (p.ActiveQuests.Count == 0)
            {
                panel.Children.Add(MakeOverlayText(
                    "No active objectives.", "#6a6a5a"));
            }
            else
            {
                panel.Children.Add(MakeOverlayText(
                    "ACTIVE", "#c8a840"));
                foreach (var q in p.ActiveQuests)
                    panel.Children.Add(MakeOverlayText(
                        $"  ★ {q}", "#c8c8b0"));
            }

            if (p.CompletedQuests.Count > 0)
            {
                panel.Children.Add(MakeOverlayText(
                    "\nCOMPLETED", "#6a8a6a"));
                foreach (var q in p.CompletedQuests)
                    panel.Children.Add(MakeOverlayText(
                        $"  ✓ {q}", "#4a6a4a"));
            }

            return panel;
        }

        private UIElement BuildCollectionsContent()
        {
            var panel = new StackPanel();
            var p = _state.Player;

            if (p.Collections.Count == 0)
            {
                panel.Children.Add(MakeOverlayText(
                    "No collections found yet.", "#6a6a5a"));
            }
            else
            {
                foreach (var c in p.Collections)
                    panel.Children.Add(MakeOverlayText(
                        $"  ◆ {c}", "#c8a840"));
            }

            return panel;
        }

        private UIElement BuildHistoryContent()
        {
            var panel = new StackPanel();
            var p = _state.Player;

            if (p.NarrativeHistory.Count == 0)
            {
                panel.Children.Add(MakeOverlayText(
                    "No history yet.", "#6a6a5a"));
            }
            else
            {
                // Show last 20 entries
                int start = System.Math.Max(
                    0, p.NarrativeHistory.Count - 20);
                for (int i = start;
                     i < p.NarrativeHistory.Count; i++)
                {
                    panel.Children.Add(MakeOverlayText(
                        p.NarrativeHistory[i], "#8a8a7a"));
                }
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
                Style = (Style)Application.Current
                    .Resources["ConsoleButton"],
                Margin = new System.Windows.Thickness(0, 4, 0, 4)
            };
            resumeBtn.Click += CloseOverlay_Click;

            var menuBtn = new Button
            {
                Content = "MAIN MENU",
                Style = (Style)Application.Current
                    .Resources["ConsoleButton"],
                Margin = new System.Windows.Thickness(0, 4, 0, 4)
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

        private TextBlock MakeOverlayText(string text,
            string color)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new System.Windows.Media
                    .FontFamily("Courier New"),
                FontSize = 13,
                Foreground = new System.Windows.Media
                    .SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows
                    .Media.ColorConverter.ConvertFromString(color)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new System.Windows.Thickness(0, 2, 0, 2)
            };
        }

        // ── Clock/Weather buttons ─────────────────

        private void ClockBtn_Click(object sender,
            RoutedEventArgs e)
        {
            // TODO: show detailed time thoughts
        }

        private void WeatherBtn_Click(object sender,
            RoutedEventArgs e)
        {
            // TODO: show detailed weather thoughts
        }

        // ── Test data ────────────────────────────

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
                        "Stay close to the walls and don't " +
                        "make noise.",
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
                                0 => "You take your first steps " +
                                     "into the fog...",
                                1 => "You scan the area carefully.",
                                2 => "You check what you're " +
                                     "carrying.",
                                _ => "You stand still and listen."
                            };
                            _state.AddToHistory(response);
                            GameConsole.AddText(response,
                                TextType.Description);

                            // Test stat change
                            _state.ModifyStamina(-1);
                            _state.AdvanceTime(15);
                        });
                });
        }
    }
}