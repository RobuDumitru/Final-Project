using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LostInAForgottenCity.Views
{
    public enum TextType
    {
        Description,
        Dialogue,
        Gameline
    }

    public partial class ConsolePanel : UserControl
    {
        private DispatcherTimer _typewriterTimer = new();
        private string _pendingText = "";
        private int _charIndex = 0;
        private TextBlock? _currentTextBlock;
        private Action? _onTypewriterComplete;

        public ConsolePanel()
        {
            InitializeComponent();
            _typewriterTimer.Interval = TimeSpan.FromMilliseconds(18);
            _typewriterTimer.Tick += TypewriterTick;

            // Click anywhere to skip typewriter
            this.MouseDown += ConsolePanel_MouseDown;
            // In AddText — show hint when typing starts
            _typewriterTimer.Start();
            SkipHint.Visibility = Visibility.Visible;
        }

        // ── Public API ───────────────────────────

        public void AddText(string text,
            TextType type = TextType.Description,
            Action? onComplete = null)
        {
            // Format and color based on type
            string displayText = type switch
            {
                TextType.Dialogue => text,
                TextType.Gameline => $"[ {text} ]",
                _ => text
            };

            Color color = type switch
            {
                TextType.Dialogue => Color.FromRgb(0xc8, 0xa8, 0x60),
                TextType.Gameline => Color.FromRgb(0x6a, 0x8a, 0x6a),
                _ => Color.FromRgb(0xc8, 0xc8, 0xb0)
            };

            var tb = new TextBlock
            {
                FontFamily = new FontFamily("Courier New"),
                FontSize = 13,
                Foreground = new SolidColorBrush(color),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextPanel.Children.Add(tb);
            ScrollToBottom();

            _pendingText = displayText;
            _charIndex = 0;
            _currentTextBlock = tb;
            _onTypewriterComplete = onComplete;
            _typewriterTimer.Start();
        }

        public void AddSeparator()
        {
            TextPanel.Children.Add(new TextBlock { Height = 8 });
        }

        public void ShowOptions(List<string> options, Action<int> onSelected)
        {
            OptionsPanel.Children.Clear();
            QtePanel.Visibility = Visibility.Collapsed;
            OptionsPanel.Visibility = Visibility.Visible;

            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                var btn = new Button
                {
                    Content = options[i],
                    Style = (Style)Application.Current
                        .Resources["GameScreenButton"],
                    Margin = new Thickness(0, 2, 0, 2)
                };

                btn.Click += (s, e) =>
                {
                    AddChoiceEcho(options[index]);
                    ClearOptions();
                    onSelected(index);
                };
                OptionsPanel.Children.Add(btn);
            }
        }

        public void ShowQTE(string prompt,
            List<string> qteOptions,
            Action<int> onSelected)
        {
            QtePromptText.Text = prompt;
            QteOptionsPanel.Children.Clear();

            OptionsPanel.Visibility = Visibility.Collapsed;
            QtePanel.Visibility = Visibility.Visible;

            for (int i = 0; i < qteOptions.Count; i++)
            {
                int index = i;
                var btn = new Button
                {
                    Content = qteOptions[i],
                    Style = (Style)Application.Current
                        .Resources["GameScreenButton"],
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0xcc, 0x40, 0x40)),
                    Margin = new Thickness(0, 0, 8, 0)
                };
                btn.Click += (s, e) =>
                {
                    QtePanel.Visibility = Visibility.Collapsed;
                    OptionsPanel.Visibility = Visibility.Visible;
                    onSelected(index);
                };
                QteOptionsPanel.Children.Add(btn);
            }
        }

        public void ClearConsole()
        {
            _typewriterTimer.Stop();
            _pendingText = "";
            _charIndex = 0;
            _currentTextBlock = null;
            _onTypewriterComplete = null;
            OptionsPanel.Children.Clear();
            OptionsPanel.Visibility = Visibility.Collapsed;
            QtePanel.Visibility = Visibility.Collapsed;
            TextPanel.Children.Clear();
            _bootTextBlock = null;
        }

        // ── Skip typewriter on click ─────────────

        private void ConsolePanel_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            if (_typewriterTimer.IsEnabled)
                SkipTypewriter();
        }

        // In SkipTypewriter — hide when skipped
        private void SkipTypewriter()
        {
            _typewriterTimer.Stop();
            if (_currentTextBlock != null)
                _currentTextBlock.Text = _pendingText;
            _charIndex = _pendingText.Length;
            SkipHint.Visibility = Visibility.Collapsed;
            ScrollToBottom();
            _onTypewriterComplete?.Invoke();
        }

        // ── Private helpers ──────────────────────

        private void ClearOptions()
        {
            OptionsPanel.Children.Clear();
        }

        private void AddChoiceEcho(string choice)
        {
            var tb = new TextBlock
            {
                Text = $"> {choice}",
                FontFamily = new FontFamily("Courier New"),
                FontSize = 13,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(0x7a, 0xaa, 0x60)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };
            TextPanel.Children.Add(tb);
            ScrollToBottom();
        }

        // In TypewriterTick — hide when naturally complete
        private void TypewriterTick(object? sender, EventArgs e)
        {
            if (_currentTextBlock == null ||
                _charIndex >= _pendingText.Length)
            {
                _typewriterTimer.Stop();
                SkipHint.Visibility = Visibility.Collapsed;
                _onTypewriterComplete?.Invoke();
                return;
            }

            _currentTextBlock.Text += _pendingText[_charIndex];
            _charIndex++;
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            TextScroller.UpdateLayout();
            TextScroller.ScrollToBottom();
        }
        public bool IsTyping => _typewriterTimer.IsEnabled;

        private TextBlock? _bootTextBlock;

        public void SetBootText(string text)
        {
            if (_bootTextBlock == null)
            {
                _bootTextBlock = new TextBlock
                {
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0x4a, 0x6a, 0x4a)),
                    Margin = new Thickness(0, 0, 0, 4)
                };
                TextPanel.Children.Add(_bootTextBlock);
                ScrollToBottom();
            }
            _bootTextBlock.Text = text;
        }

        public void SetTypewriterSpeed(int milliseconds)
        {
            _typewriterTimer.Interval =
                TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}