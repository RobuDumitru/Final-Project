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