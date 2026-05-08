using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class StaminaSleepBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(StaminaSleepBar),
                new PropertyMetadata(0, OnPropertyChanged));

        public static readonly DependencyProperty MaxSegmentsProperty =
            DependencyProperty.Register(
                "MaxSegments", typeof(int), typeof(StaminaSleepBar),
                new PropertyMetadata(10, OnPropertyChanged));

        public static readonly DependencyProperty SleepValueProperty =
            DependencyProperty.Register(
                "SleepValue", typeof(int), typeof(StaminaSleepBar),
                new PropertyMetadata(100, OnPropertyChanged));

        public static readonly DependencyProperty IsSleepVisibleProperty =
            DependencyProperty.Register(
                "IsSleepVisible", typeof(bool), typeof(StaminaSleepBar),
                new PropertyMetadata(true, OnPropertyChanged));

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

        public int SleepValue
        {
            get => (int)GetValue(SleepValueProperty);
            set => SetValue(SleepValueProperty, value);
        }

        public bool IsSleepVisible
        {
            get => (bool)GetValue(IsSleepVisibleProperty);
            set => SetValue(IsSleepVisibleProperty, value);
        }

        public StaminaSleepBar()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is StaminaSleepBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
        {
            StaminaPanel.Children.Clear();
            SleepPanel.Children.Clear();

            // Stamina
            for (int i = 0; i < MaxSegments; i++)
            {
                var symbol = new TextBlock
                {
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 14,
                    Width = 16,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(1, 0, 1, 0)
                };

                if (i >= MaxSegments - Value)
                {
                    symbol.Text = "▲";
                    symbol.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x60, 0xaa, 0x60));
                }
                else
                {
                    symbol.Text = "△";
                    symbol.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x60, 0xaa, 0x60));
                }

                StaminaPanel.Children.Add(symbol);
            }

            SleepPanel.Visibility = IsSleepVisible
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (IsSleepVisible)
            {
                const int totalSleepSymbols = 20;
                int sleepFilled = (SleepValue * totalSleepSymbols + 50) / 100;
                sleepFilled = Math.Clamp(sleepFilled, 0, totalSleepSymbols);
                int sleepEmpty = totalSleepSymbols - sleepFilled;

                var sleepTrack = new TextBlock
                {
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 14,
                    TextAlignment = TextAlignment.Left,
                    Margin = new Thickness(0),
                    TextWrapping = TextWrapping.NoWrap
                };

                if (sleepEmpty > 0)
                {
                    sleepTrack.Inlines.Add(new Run
                    {
                        Text = new string('░', sleepEmpty),
                        Foreground = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x50))
                    });
                }

                if (sleepFilled > 0)
                {
                    sleepTrack.Inlines.Add(new Run
                    {
                        Text = new string('¿', sleepFilled),
                        Foreground = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0xaa))
                    });
                }

                SleepPanel.Children.Add(sleepTrack);
            }

            StaminaLabel.Text = $"{Value}/{MaxSegments}";
            SleepLabel.Text = $"{SleepValue}%";
        }
    }
}