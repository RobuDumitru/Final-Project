using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LostInAForgottenCity
{
    public partial class MainWindow : Window
    {
        public MainWindow()
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
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Tutorial_Click(object sender, RoutedEventArgs e)
        {
            TutorialWindow tutorial = new TutorialWindow();
            tutorial.Show();
            this.Hide();
        }
    }
}