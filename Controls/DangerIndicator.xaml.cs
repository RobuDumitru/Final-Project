using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public enum DangerLevel
    {
        Insignificant,
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Deadly
    }

    public partial class DangerIndicator : UserControl
    {
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(
                "Level", typeof(DangerLevel), typeof(DangerIndicator),
                new PropertyMetadata(DangerLevel.Insignificant, OnPropertyChanged));

        public static readonly DependencyProperty StatusEffectProperty =
            DependencyProperty.Register(
                "StatusEffect", typeof(string), typeof(DangerIndicator),
                new PropertyMetadata("", OnPropertyChanged));

        public DangerLevel Level
        {
            get => (DangerLevel)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        public string StatusEffect
        {
            get => (string)GetValue(StatusEffectProperty);
            set => SetValue(StatusEffectProperty, value);
        }

        public DangerIndicator()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is DangerIndicator indicator)
                indicator.UpdateVisual();
        }

        private void UpdateVisual()
        {
            switch (Level)
            {
                case DangerLevel.Insignificant:
                    DangerIcon.Text = "⚫";
                    DangerText.Text = "INSIGNIFICANT";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x7a, 0xaa, 0x60));
                    CautionText.Text = "";
                    break;

                case DangerLevel.VeryLow:
                    DangerIcon.Text = "🔵";
                    DangerText.Text = "VERY LOW";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x60, 0x80, 0xaa));
                    CautionText.Text = "";
                    break;

                case DangerLevel.Low:
                    DangerIcon.Text = "🟢";
                    DangerText.Text = "LOW";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x60, 0xaa, 0x60));
                    CautionText.Text = "";
                    break;

                case DangerLevel.Medium:
                    DangerIcon.Text = "🟡";
                    DangerText.Text = "MEDIUM";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xaa, 0xaa, 0x40));
                    CautionText.Text = "";
                    break;

                case DangerLevel.High:
                    DangerIcon.Text = "🟠";
                    DangerText.Text = "HIGH";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xcc, 0x80, 0x20));
                    CautionText.Text = "— CAUTION";
                    break;

                case DangerLevel.VeryHigh:
                    DangerIcon.Text = "🔴";
                    DangerText.Text = "VERY HIGH";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xcc, 0x40, 0x40));
                    CautionText.Text = "— CAUTION";
                    break;

                case DangerLevel.Deadly:
                    DangerIcon.Text = "💀";
                    DangerText.Text = "DEADLY";
                    DangerText.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xaa, 0x20, 0x20));
                    CautionText.Text = "— CAUTION";
                    break;
            }

            // Status effect
            StatusText.Text = StatusEffect;
        }
    }
}