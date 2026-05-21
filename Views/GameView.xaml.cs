using System.Collections.Generic;
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
            LoadTestMap();
        }

        private void LoadTestMap()
        {
            var nodes = new List<LostInAForgottenCity.Controls.MapNode>
    {
        new() { Id="outskirts", Name="Outskirts", BaseIcon="🏚",
                State=Controls.LocationState.Visited,
                Special=Controls.SpecialMarker.CurrentLocation,
                Type=Controls.LocationType.Normal,
                X=20, Y=60 },
        new() { Id="farm", Name="Farm", BaseIcon="🌾",
                State=Controls.LocationState.Visited,
                Special=Controls.SpecialMarker.MainQuestActive,
                Type=Controls.LocationType.Normal,
                X=100, Y=20 },
        new() { Id="church", Name="Church", BaseIcon="⛪",
                State=Controls.LocationState.Discovered,
                HasDiscoveredSafeRoom=true,
                Type=Controls.LocationType.Normal,
                X=100, Y=100 },
        new() { Id="crossroads", Name="Crossroads", BaseIcon="🏚",
                State=Controls.LocationState.Undiscovered,
                Type=Controls.LocationType.Normal,
                X=180, Y=60 },
        new() { Id="mill", Name="Mill", BaseIcon="⚒",
                State=Controls.LocationState.Undiscovered,
                Type=Controls.LocationType.Normal,
                X=260, Y=20 },
        new() { Id="houses", Name="Row of Houses", BaseIcon="⌂",
                State=Controls.LocationState.Undiscovered,
                Type=Controls.LocationType.Normal,
                X=260, Y=100 },
        new() { Id="expedition", Name="Expedition", BaseIcon="🗺",
                State=Controls.LocationState.Undiscovered,
                Type=Controls.LocationType.ExpeditionPoint,
                X=180, Y=140 },
    };

            var connections = new List<LostInAForgottenCity.Controls.MapConnection>
    {
        new() { FromId="outskirts", ToId="farm" },
        new() { FromId="outskirts", ToId="church" },
        new() { FromId="farm", ToId="crossroads" },
        new() { FromId="church", ToId="crossroads" },
        new() { FromId="crossroads", ToId="mill" },
        new() { FromId="crossroads", ToId="houses" },
        new() { FromId="crossroads", ToId="expedition" },
    };

            // Add available maps for testing
            GameMap.AddAvailableMap(new Controls.MapTab
            {
                Id = "suburban",
                Title = "Suburban",
                Type = Controls.MapType.Region,
                IsUnlocked = true,
                Nodes = new(),
                Connections = new()
            });

            GameMap.AddAvailableMap(new Controls.MapTab
            {
                Id = "outskirts_location",
                Title = "Outskirts",
                Type = Controls.MapType.Location,
                IsUnlocked = true,
                Nodes = new(),
                Connections = new()
            });

            GameMap.LoadMap(nodes, connections,
                "rural", "Rural",
                Controls.MapType.Region,
                "outskirts");
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