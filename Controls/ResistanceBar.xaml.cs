using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LostInAForgottenCity.Controls
{
    public partial class ResistanceBar : UserControl
    {
        private DispatcherTimer _flickerTimer = new();
        private bool _flickerState = true;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(ResistanceBar),
                new PropertyMetadata(50, OnPropertyChanged));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                "MaxValue", typeof(int), typeof(ResistanceBar),
                new PropertyMetadata(50, OnPropertyChanged));

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

        public ResistanceBar()
        {
            InitializeComponent();
            _flickerTimer.Interval = TimeSpan.FromMilliseconds(400);
            _flickerTimer.Tick += (s, e) =>
            {
                _flickerState = !_flickerState;
                UpdateVisual();
            };
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is ResistanceBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
{
    ResistancePanel.Children.Clear();

    int totalSegments = MaxValue;
    int filledSegments = Value;
    int emptySegments = totalSegments - filledSegments;
    int lostEachSide = emptySegments / 2;

    bool isCritical = Value <= 4;
    if (isCritical && !_flickerTimer.IsEnabled)
        _flickerTimer.Start();
    else if (!isCritical && _flickerTimer.IsEnabled)
        _flickerTimer.Stop();

    for (int i = 0; i < totalSegments; i++)
    {
        bool isLostLeft = i < lostEachSide;
        bool isLostRight = i >= totalSegments - lostEachSide;
        bool isEmpty = isLostLeft || isLostRight;

        bool isCenterSegment = i == totalSegments / 2;
        if (isCritical && isCenterSegment && !_flickerState)
            isEmpty = true;

        // Create segment here
        var segment = new TextBlock
        {
            FontFamily = new FontFamily("Courier New"),
            FontSize = 14,
            Width = 10,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0)
        };

        if (!isEmpty)
        {
            segment.Text = "‼";
            segment.Foreground = new SolidColorBrush(
                isCritical
                ? Color.FromRgb(0xcc, 0x40, 0x40)
                : Color.FromRgb(0x60, 0xc8, 0xa0));
        }
        else
        {
            segment.Text = "░";
            segment.Foreground = new SolidColorBrush(
                Color.FromRgb(0x30, 0x20, 0x10));
        }

        ResistancePanel.Children.Add(segment);
    }
}
    }
}