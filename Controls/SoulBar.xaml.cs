using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LostInAForgottenCity.Controls
{
    public partial class SoulBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(SoulBar),
                new PropertyMetadata(0, OnPropertyChanged));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                "MaxValue", typeof(int), typeof(SoulBar),
                new PropertyMetadata(100, OnPropertyChanged));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        private DispatcherTimer _flickerTimer;
        private bool _flickerState = false;

        public SoulBar()
        {
            InitializeComponent();
            _flickerTimer = new DispatcherTimer();
            _flickerTimer.Interval = TimeSpan.FromMilliseconds(500);
            _flickerTimer.Tick += (s, e) => { _flickerState = !_flickerState; UpdateVisual(); };
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is SoulBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
        {
            SoulPanel.Children.Clear();

            int totalSegments = MaxValue;
            int filledSegments = Value;
            int emptySegments = totalSegments - filledSegments;
            int lostEachSide = emptySegments / 2;

            bool isCritical = Value <= 4;
            if (isCritical)
                _flickerTimer.Start();
            else
                _flickerTimer.Stop();

            for (int i = 0; i < totalSegments; i++)
            {
                bool isLostLeft = i < lostEachSide;
                bool isLostRight = i >= totalSegments - lostEachSide;
                bool isEmpty = isLostLeft || isLostRight;
                bool isCenterSegment = i == totalSegments / 2;

                if (isCritical && isCenterSegment && !_flickerState)
                    isEmpty = true;

                var segment = new TextBlock
                {
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 10,
                    Width = 6,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0)
                };

                if (!isEmpty)
                {
                    segment.Text = "§";
                    segment.Foreground = new SolidColorBrush(
                        isCritical
                        ? Color.FromRgb(0xff, 0x40, 0x40)
                        : Color.FromRgb(0xff, 0xcc, 0x00));
                }
                else
                {
                    segment.Text = "§";
                    segment.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x40, 0x30, 0x00));
                }

                SoulPanel.Children.Add(segment);
            }

            Label.Text = $"{Value}/{MaxValue}";
        }
    }
}