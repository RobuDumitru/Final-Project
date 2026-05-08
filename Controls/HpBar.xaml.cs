using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class HpBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(HpBar),
                new PropertyMetadata(0, OnPropertyChanged));

        public static readonly DependencyProperty MaxSegmentsProperty =
            DependencyProperty.Register(
                "MaxSegments", typeof(int), typeof(HpBar),
                new PropertyMetadata(10, OnPropertyChanged));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MaxSegments
        {
            get => (int)GetValue(MaxSegmentsProperty);
            set => SetValue(MaxSegmentsProperty, value);
        }

        public HpBar()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is HpBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
        {
            SegmentsPanel.Children.Clear();

            for (int i = 0; i < MaxSegments; i++)
            {
                var segment = new TextBlock
                {
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 14,
                    Width = 16,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(1, 0, 1, 0)
                };

                if (i >= MaxSegments - Value)
                {
                    segment.Text = "▮"; // filled
                    segment.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xaa, 0x40, 0x40));
                }
                else
                {
                    segment.Text = "▯"; // empty
                    segment.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xaa, 0x40, 0x40));
                }

                SegmentsPanel.Children.Add(segment);
            }

            Label.Text = $"{Value}/{MaxSegments}";
        }
    }
}