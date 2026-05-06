using System.Windows;
using System.Windows.Controls;
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
                new PropertyMetadata(5, OnPropertyChanged));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public int MaxSegments
        {
            get { return (int)GetValue(MaxSegmentsProperty); }
            set { SetValue(MaxSegmentsProperty, value); }
        }

        public static readonly DependencyProperty SleepValueProperty =
            DependencyProperty.Register(
                "SleepValue", typeof(int), typeof(StaminaSleepBar),
                new PropertyMetadata(100, OnPropertyChanged));

        public static readonly DependencyProperty IsSleepVisibleProperty =
            DependencyProperty.Register(
                "IsSleepVisible", typeof(bool), typeof(StaminaSleepBar),
                new PropertyMetadata(true, OnPropertyChanged));

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
            // Step 1: Clear both panels
            StaminaPanel.Children.Clear();
            SleepPanel.Children.Clear();

            // Step 2: Generate stamina symbols
            // ▲ = full (i < Value)
            // △ = empty (i >= Value)
            // Loop from 0 to MaxSegments
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

                if (i < Value)
                {
                    symbol.Text = "▲";
                    symbol.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x60, 0xaa, 0x60));
                }
                else
                {
                    symbol.Text = "△";
                    symbol.Foreground = new SolidColorBrush(
                        Color.FromRgb(0x30, 0x50, 0x30));
                }

                StaminaPanel.Children.Add(symbol);
            }

            // Step 3: Show/hide sleep panel
            // SleepPanel.Visibility = IsSleepVisible ? 
            //     Visibility.Visible : Visibility.Collapsed
            SleepPanel.Visibility = IsSleepVisible ?
                Visibility.Visible : Visibility.Collapsed;

            // Step 4: Generate sleep symbols if visible
            // ¿ = full (i >= (100 - SleepValue) / 10)
            // ░ = empty
            // 10 total slots
            if (IsSleepVisible)
            {
                int sleepThreshold = (100 - SleepValue) / 10;
                for (int i = 0; i < 10; i++)
                {
                    var symbol = new TextBlock
                    {
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 14,
                        Width = 16,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(1, 0, 1, 0)
                    };

                    int sleepFilled = SleepValue / 10;
                    if (i >= sleepThreshold)
                    {
                        symbol.Text = "¿";
                        symbol.Foreground = new SolidColorBrush(
                            Color.FromRgb(0x60, 0x60, 0xaa));
                    }
                    else
                    {
                        symbol.Text = "░";
                        symbol.Foreground = new SolidColorBrush(
                            Color.FromRgb(0x30, 0x30, 0x50));
                    }

                    SleepPanel.Children.Add(symbol);
                }
            }
        }
    }
}