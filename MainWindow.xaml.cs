using System.Windows;
using System.Windows.Controls;
using LostInAForgottenCity.Views;

namespace LostInAForgottenCity
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            NavigateTo(new MenuView());
        }

        public void NavigateTo(UserControl view)
        {
            MainContent.Content = view;
        }
    }
}