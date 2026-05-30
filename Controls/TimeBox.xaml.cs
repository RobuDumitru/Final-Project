using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class TimeBox : UserControl
    {
        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(int), typeof(TimeBox),
                new PropertyMetadata(1, OnPropertyChanged));
        public static readonly DependencyProperty HourProperty =
            DependencyProperty.Register("Hour", typeof(int), typeof(TimeBox),
                new PropertyMetadata(15, OnPropertyChanged));
        public static readonly DependencyProperty MinuteProperty =
            DependencyProperty.Register("Minute", typeof(int), typeof(TimeBox),
                new PropertyMetadata(0, OnPropertyChanged));
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register("Date", typeof(string), typeof(TimeBox),
                new PropertyMetadata("12/06/2012", OnPropertyChanged));
        public static readonly DependencyProperty WeatherProperty =
            DependencyProperty.Register("Weather", typeof(string), typeof(TimeBox),
                new PropertyMetadata("Foggy", OnPropertyChanged));
        public static readonly DependencyProperty TemperatureProperty =
            DependencyProperty.Register("Temperature", typeof(int), typeof(TimeBox),
                new PropertyMetadata(8, OnPropertyChanged));
        public static readonly DependencyProperty FeelsLikeProperty =
            DependencyProperty.Register("FeelsLike", typeof(string), typeof(TimeBox),
                new PropertyMetadata("Chilly", OnPropertyChanged));
        public static readonly DependencyProperty HazardProperty =
            DependencyProperty.Register("Hazard", typeof(string), typeof(TimeBox),
                new PropertyMetadata("None", OnPropertyChanged));
        public static readonly DependencyProperty IsCurseVisibleProperty =
            DependencyProperty.Register("IsCurseVisible", typeof(bool), typeof(TimeBox),
                new PropertyMetadata(false, OnPropertyChanged));
        public static readonly DependencyProperty CurseTextContentProperty =
            DependencyProperty.Register("CurseTextContent", typeof(string), typeof(TimeBox),
                new PropertyMetadata("", OnPropertyChanged));
        public static readonly DependencyProperty IsDayCounterVisibleProperty =
            DependencyProperty.Register("IsDayCounterVisible", typeof(bool), typeof(TimeBox),
                new PropertyMetadata(true, OnPropertyChanged));

        public int Day { get => (int)GetValue(DayProperty); set => SetValue(DayProperty, value); }
        public int Hour { get => (int)GetValue(HourProperty); set => SetValue(HourProperty, value); }
        public int Minute { get => (int)GetValue(MinuteProperty); set => SetValue(MinuteProperty, value); }
        public string Date { get => (string)GetValue(DateProperty); set => SetValue(DateProperty, value); }
        public string Weather { get => (string)GetValue(WeatherProperty); set => SetValue(WeatherProperty, value); }
        public int Temperature { get => (int)GetValue(TemperatureProperty); set => SetValue(TemperatureProperty, value); }
        public string FeelsLike { get => (string)GetValue(FeelsLikeProperty); set => SetValue(FeelsLikeProperty, value); }
        public string Hazard { get => (string)GetValue(HazardProperty); set => SetValue(HazardProperty, value); }
        public bool IsCurseVisible { get => (bool)GetValue(IsCurseVisibleProperty); set => SetValue(IsCurseVisibleProperty, value); }
        public string CurseTextContent { get => (string)GetValue(CurseTextContentProperty); set => SetValue(CurseTextContentProperty, value); }
        public bool IsDayCounterVisible { get => (bool)GetValue(IsDayCounterVisibleProperty); set => SetValue(IsDayCounterVisibleProperty, value); }

        public TimeBox()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeBox box) box.UpdateVisual();
        }

        public void RevealWeather()
        {
            WeatherOverlay.Visibility = Visibility.Collapsed;
        }

        private void UpdateVisual()
        {
            DateText.Text = Date;
            TimeText.Text = $"{Hour:D2}:{Minute:D2}";
            PeriodText.Text = GetPeriod(Hour);

            if (IsDayCounterVisible)
            {
                DayText.Text = Day.ToString();
                DayText.Foreground = Day >= 24
                    ? new SolidColorBrush(Color.FromRgb(0xcc, 0x40, 0x40))
                    : new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0xb0));
            }
            else
            {
                DayText.Text = "[ disabled ]";
                DayText.Foreground = new SolidColorBrush(
                    Color.FromRgb(0x4a, 0x4a, 0x5a));
            }

            CurseText.Visibility = IsCurseVisible
                ? Visibility.Visible : Visibility.Collapsed;
            CurseText.Text = IsCurseVisible ? $"⚠ {CurseTextContent}" : "";

            WeatherText.Text = $"{GetWeatherIcon(Weather)} {Weather}";

            var tempColor = GetTemperatureColor(Temperature);
            TempText.Text = $"{Temperature}°C";
            TempText.Foreground = new SolidColorBrush(tempColor);
            FeelsText.Text = $"  {FeelsLike}";
            FeelsText.Foreground = new SolidColorBrush(tempColor);

            HazardText.Text = Hazard;
            HazardText.Foreground = new SolidColorBrush(GetHazardColor(Hazard));
        }

        private string GetPeriod(int hour) => hour switch
        {
            0 => "Midnight",
            >= 1 and <= 4 => "Wee Hours",
            5 => "Dawn",
            >= 6 and <= 9 => "Morning",
            >= 10 and <= 11 => "Noon",
            >= 12 and <= 13 => "Midday",
            >= 14 and <= 17 => "Afternoon",
            >= 18 and <= 19 => "Evening",
            20 => "Dusk",
            >= 21 and <= 22 => "Night",
            23 => "Twilight",
            _ => ""
        };

        private string GetWeatherIcon(string weather) => weather switch
        {
            "Clear" or "Sunny" => "☀",
            "Cloudy" or "Overcast" => "☁",
            "Partly Cloudy" => "⛅",
            "Foggy" => "🌫",
            "Rainy" or "Heavy Rain" => "🌧",
            "Stormy" => "⛈",
            "Snowy" => "❄",
            "Windy" => "💨",
            "Thunder" => "⚡",
            _ => "~"
        };

        private Color GetTemperatureColor(int temp)
        {
            if (temp < 0)   return Color.FromRgb(0x60, 0x80, 0xcc);
            if (temp <= 8)  return Color.FromRgb(0x60, 0xa8, 0xa0);
            if (temp <= 15) return Color.FromRgb(0x60, 0xc8, 0xc8);
            if (temp <= 22) return Color.FromRgb(0xc8, 0xc8, 0xb0);
            if (temp <= 28) return Color.FromRgb(0xc8, 0xc8, 0x40);
            if (temp <= 35) return Color.FromRgb(0xc8, 0x78, 0x40);
            return Color.FromRgb(0xcc, 0x40, 0x40);
        }

        private Color GetHazardColor(string hazard) => hazard switch
        {
            "None"            => Color.FromRgb(0x7a, 0xaa, 0x60),
            "Stale Air"       => Color.FromRgb(0xc8, 0xc8, 0x40),
            "Low Visibility"  => Color.FromRgb(0xc8, 0x78, 0x40),
            "Toxic Air"       => Color.FromRgb(0xcc, 0x40, 0x40),
            "Soul Trap"       => Color.FromRgb(0x9a, 0x40, 0xcc),
            _ => Color.FromRgb(0x7a, 0xaa, 0x60)
        };
    }
}