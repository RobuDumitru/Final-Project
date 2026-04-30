using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LostInAForgottenCity.Views;

namespace LostInAForgottenCity.Views
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                string original = btn.Content.ToString()?.TrimStart() ?? "";
                btn.Content = "> " + original;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                string current = btn.Content.ToString() ?? "";
                if (current.StartsWith("> "))
                    btn.Content = "  " + current.Substring(2);
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new GameView());
        }

        private void Tutorial_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new TutorialView());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}