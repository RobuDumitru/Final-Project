using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class SanityBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(double),
                typeof(SanityBar),
                new PropertyMetadata(100.0, OnValueChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public SanityBar()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is SanityBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
        {
            double clampedValue = Math.Max(0.0, Math.Min(100.0, Value));
            Bar.Value = 100.0 - clampedValue;
            PercentText.Text = $"{(int)clampedValue}%";

            if (clampedValue > 60)
                Bar.Foreground = new SolidColorBrush(Color.FromRgb(0x7a, 0xaa, 0x60));
            else if (clampedValue > 30)
                Bar.Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0x30));
            else
                Bar.Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0x40, 0x40));
        }
    }
}