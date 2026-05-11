using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LostInAForgottenCity.Views
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        private void MenuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                string original = btn.Content.ToString()?.TrimStart() ?? "";
                btn.Content = "> " + original;
            }
        }

        private void MenuButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                string current = btn.Content.ToString() ?? "";
                if (current.StartsWith("> "))
                    btn.Content = "  " + current.Substring(2);
            }
        }

        private void ClockBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: show detailed time info + thoughts
        }

        private void WeatherBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: show detailed weather info + thoughts
        }

        private void InventorySlot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x7a, 0xaa, 0x60));
            }
        }

        private void InventorySlot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3a, 0x4a, 0x3a));
            }
        }
    }
}