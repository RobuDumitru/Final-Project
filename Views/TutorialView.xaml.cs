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
            {
                Dispatcher.Invoke(() =>
                    DisplayChoices(choices));
            };

            _dialogue.OnAutoNext += id =>
                Dispatcher.Invoke(() =>
                    _dialogue.StartScene(id));

            _dialogue.OnSceneComplete += () => { };

            _dialogue.StartScene("fortuneteller_arrival");
        }

        private void DisplayChoices(
            List<DialogueChoice> choices)
        {
            var options = new List<string>();
            foreach (var c in choices)
                options.Add(c.Text);

            TutorialConsole.ShowOptions(options, index =>
            {
                // Tutorial type choices
                if (choices[index].NextSceneId ==
                    "tutorial_introduction_start")
                {
                    ShowProceedButton(
                        "tutorial_introduction_start");
                    return;
                }
                if (choices[index].NextSceneId ==
                    "tutorial_scenarios_start")
                {
                    ShowProceedButton(
                        "tutorial_scenarios_start");
                    return;
                }

                _dialogue.GoToScene(
                    choices[index].NextSceneId);
            });
        }

        private void ShowProceedButton(string sceneId)
        {
            TutorialConsole.ShowOptions(
                new List<string> { "Proceed" },
                _ =>
                {
                    var gameView = new GameView();
                    gameView.StartTutorial(sceneId);
                    MainWindow.Instance?.NavigateTo(gameView);
                });
        }
    }
}