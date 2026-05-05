using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class SubconsciousBar : UserControl
    {
        // How many slots are filled (transformation progress)
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(SubconsciousBar),
                new PropertyMetadata(0, OnPropertyChanged));

        // Total slots available (5 base + up to 5 from ritual dolls)
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
            // Clear existing slots
            SlotsPanel.Children.Clear();

            // Generate slots dynamically
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
                    // White mask — transformation
                    slot.Text = "☻";
                    slot.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xc0, 0xc0, 0xc0));
                }
                else
                {
                    // Black mask — you
                    slot.Text = "☺";
                    slot.Foreground = new SolidColorBrush(
                        Color.FromRgb(0xc8, 0xa8, 0xc8));
                }

                SlotsPanel.Children.Add(slot);
            }
        }
    }
}