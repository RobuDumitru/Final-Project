using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class SubconsciousBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(SubconsciousBar),
                new PropertyMetadata(0, OnPropertyChanged));

        public static readonly DependencyProperty MaxSlotsProperty =
            DependencyProperty.Register(
                "MaxSlots", typeof(int), typeof(SubconsciousBar),
                new PropertyMetadata(5, OnPropertyChanged));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MaxSlots
        {
            get => (int)GetValue(MaxSlotsProperty);
            set => SetValue(MaxSlotsProperty, value);
        }

        public SubconsciousBar()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is SubconsciousBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
        {
            SlotsPanel.Children.Clear();

            for (int i = 0; i < MaxSlots; i++)
            {
                var slot = new TextBlock
                {
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 16,
                    Margin = new Thickness(2, 0, 2, 0)
                };

                if (i < Value)
                {
                    slot.Text = "☻"; // mask
                    slot.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xc0, 0xc0, 0xc0));
                }
                else
                {
                    slot.Text = "☺"; // self
                    slot.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xc0, 0xc0, 0xc0));
                }

                SlotsPanel.Children.Add(slot);
            }

            Label.Text = $"{Value}/{MaxSlots}";
        }
    }
}