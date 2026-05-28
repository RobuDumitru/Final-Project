using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LostInAForgottenCity.Engine;

namespace LostInAForgottenCity.Views
{
    public partial class TutorialView : UserControl
    {
        private DialogueEngine _dialogue = new();
        private DispatcherTimer _bootTimer = new();
        private int _bootDots = 0;
        private const int MaxBootDots = 6;
        private string lastSpeaker = "";

        public TutorialView()
        {
            InitializeComponent();
            Loaded += TutorialView_Loaded;
        }

        private void TutorialView_Loaded(object sender,
            RoutedEventArgs e)
        {
            StartBootSequence();
        }

        // ── Boot sequence ────────────────────────

        private void StartBootSequence()
        {
            _bootTimer.Interval =
                TimeSpan.FromMilliseconds(400);
            _bootTimer.Tick += BootTick;
            _bootTimer.Start();
        }

        private void BootTick(object? sender, EventArgs e)
        {
            _bootDots++;

            string dots = new string(' ', _bootDots)
                .Replace(" ", " .");
            TutorialConsole.SetBootText(
                $"Booting{dots}");

            if (_bootDots >= MaxBootDots)
            {
                _bootTimer.Stop();
                OnBootComplete();
            }
        }

        private void OnBootComplete()
        {
            // Small pause after boot
            var pauseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            pauseTimer.Tick += (s, e) =>
            {
                pauseTimer.Stop();
                StartDialogue();
            };
            pauseTimer.Start();
        }

        // ── Dialogue ─────────────────────────────

        private void StartDialogue()
        {
            _dialogue.LoadDialogue(
                DialogueData.GetTutorialDialogue());

            _dialogue.OnLine += (line, next) =>
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

                    TutorialConsole.AddText(displayText, type,
                        onComplete: () => next());
                });
            };

            _dialogue.OnChoices += choices =>
                Dispatcher.Invoke(() => DisplayChoices(choices));

            _dialogue.OnAutoNext += id =>
                Dispatcher.Invoke(() => _dialogue.StartScene(id));

            _dialogue.OnSceneComplete += () => { };

            // Check if first visit or return
            bool returning = GameState.Instance.Player.HasVisitedTutorial;

            if (!returning)
            {
                GameState.Instance.Player.HasVisitedTutorial = true;
                _dialogue.StartScene("fortuneteller_arrival");
            }
            else
            {
                _dialogue.StartScene("fortuneteller_return");
            }
        }

        private void DisplayChoices(List<DialogueChoice> choices)
        {
            var options = new List<string>();
            foreach (var c in choices)
                options.Add(c.Text);

            TutorialConsole.ShowOptions(options, index =>
            {
                string nextId = choices[index].NextSceneId;

                switch (nextId)
                {
                    case "intro_tutorial_begin":
                        _dialogue.UnsubscribeAll();
                        var gameView = new GameView();
                        gameView.StartTutorial("intro_tutorial_begin");
                        MainWindow.Instance?.NavigateTo(gameView);
                        break;

                    case "tutorial_scenarios_check":
                        bool introCompleted = GameState.Instance.Player
                            .CompletedQuests.Contains("intro_tutorial");
                        _dialogue.GoToScene(introCompleted
                            ? "tutorial_scenarios_unlocked"
                            : "tutorial_scenarios_locked");
                        break;

                    case "tutorial_go_back":
                        MainWindow.Instance?.NavigateTo(new MenuView());
                        break;

                    case "scenario_selection":
                        // TODO: open scenario selection screen
                        break;

                    default:
                        _dialogue.GoToScene(nextId);
                        break;
                }
            });
        }
    }
}