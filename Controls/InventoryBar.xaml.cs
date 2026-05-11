using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LostInAForgottenCity.Controls
{
    public partial class InventoryBar : UserControl
    {
        public static readonly DependencyProperty SlotCountProperty =
            DependencyProperty.Register(
                "SlotCount", typeof(int), typeof(InventoryBar),
                new PropertyMetadata(6, OnPropertyChanged));

        public int SlotCount
        {
            get => (int)GetValue(SlotCountProperty);
            set => SetValue(SlotCountProperty, value);
        }

        public InventoryBar()
        {
            InitializeComponent();
            UpdateVisual();
        }

        private static void OnPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is InventoryBar bar)
                bar.UpdateVisual();
        }

        private void UpdateVisual()
        {
            SlotsGrid.Children.Clear();
            SlotsGrid.Columns = SlotCount / 2;

            for (int i = 0; i < SlotCount; i++)
            {
                var border = new Border
                {
                    BorderBrush = new SolidColorBrush(
                        Color.FromRgb(0x3a, 0x4a, 0x3a)),
                    BorderThickness = new Thickness(1),
                    Width = 55,
                    Height = 40,
                    Margin = new Thickness(2),
                    Cursor = Cursors.Hand
                };

                var text = new TextBlock
                {
                    Text = "□",
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 18,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0x3a, 0x4a, 0x3a)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                border.Child = text;
                border.MouseEnter += Slot_MouseEnter;
                border.MouseLeave += Slot_MouseLeave;
                SlotsGrid.Children.Add(border);
            }
        }

        private void Slot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border b)
                b.BorderBrush = new SolidColorBrush(
                    Color.FromRgb(0x7a, 0xaa, 0x60));
        }

        private void Slot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border b)
                b.BorderBrush = new SolidColorBrush(
                    Color.FromRgb(0x3a, 0x4a, 0x3a));
        }
    }
}