using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LostInAForgottenCity.Controls
{
    public enum LocationState
    {
        Undiscovered, Discovered, Visited, Explored, Looted
    }

    public enum SpecialMarker
    {
        None,
        CurrentLocation,
        MainQuestActive, MainQuestAvailable,
        SideQuestActive, SideQuestAvailable,
        SpecialQuestActive, SpecialQuestAvailable,
        Unavailable, Locked, Danger, Deadly, SafeRoom
    }

    public enum LocationType
    {
        Normal, Bridge, ExpeditionPoint
    }

    public enum MapType
    {
        Region, Location
    }

    public class MapNode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string BaseIcon { get; set; } = "?";
        public LocationState State { get; set; } = LocationState.Undiscovered;
        public SpecialMarker Special { get; set; } = SpecialMarker.None;
        public LocationType Type { get; set; } = LocationType.Normal;
        public double X { get; set; }
        public double Y { get; set; }
        public bool HasDiscoveredSafeRoom { get; set; } = false;
    }

    public class MapConnection
    {
        public string FromId { get; set; } = "";
        public string ToId { get; set; } = "";
        public bool IsPlayerTravelling { get; set; } = false;
        public double TravelProgress { get; set; } = 0.0;
    }

    public class MapTab
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public MapType Type { get; set; } = MapType.Region;
        public bool IsCurrentMap { get; set; } = false;
        public bool IsUnlocked { get; set; } = false;
        public List<MapNode> Nodes { get; set; } = new();
        public List<MapConnection> Connections { get; set; } = new();
    }

    public partial class MapPanel : UserControl
    {
        // ── Fields ──────────────────────────────
        private List<MapTab> _openTabs = new();
        private List<MapTab> _availableTabs = new();
        private MapTab? _activeTab = null;
        private string _currentLocationId = "";
        private string _travellingToId = "";
        private const int MaxTabs = 5;

        // Drag
        private bool _isDragging = false;
        private Point _dragStart;
        private double _dragOriginX;
        private double _dragOriginY;

        // Zoom
        private const double MinZoom = 0.5;
        private const double MaxZoom = 3.0;

        // Legend
        private bool _legendExpanded = true;

        public MapPanel()
        {
            InitializeComponent();
        }

        // ── Public API ───────────────────────────

        public void LoadMap(List<MapNode> nodes,
                           List<MapConnection> connections,
                           string tabId,
                           string tabTitle,
                           MapType mapType,
                           string currentLocationId,
                           string travellingToId = "")
        {
            _currentLocationId = currentLocationId;
            _travellingToId = travellingToId;

            // Create or update the current tab
            var existingTab = _openTabs.FirstOrDefault(t => t.Id == tabId);
            if (existingTab == null)
            {
                var tab = new MapTab
                {
                    Id = tabId,
                    Title = tabTitle,
                    Type = mapType,
                    IsCurrentMap = true,
                    IsUnlocked = true,
                    Nodes = nodes,
                    Connections = connections
                };
                // Mark previous current as non-current
                foreach (var t in _openTabs)
                    t.IsCurrentMap = false;

                _openTabs.Add(tab);
                _activeTab = tab;
            }
            else
            {
                existingTab.Nodes = nodes;
                existingTab.Connections = connections;
                existingTab.IsCurrentMap = true;
                foreach (var t in _openTabs)
                    if (t.Id != tabId) t.IsCurrentMap = false;
                _activeTab = existingTab;
            }

            RefreshTabs();
            DrawMap();
        }

        public void AddAvailableMap(MapTab tab)
        {
            if (!_availableTabs.Any(t => t.Id == tab.Id))
                _availableTabs.Add(tab);
        }

        // ── Drawing ──────────────────────────────

        private void DrawMap()
        {
            if (_activeTab == null) return;

            MapCanvas.Children.Clear();
            SectionTitle.Text = _activeTab.Title.ToUpper();

            // Draw connections
            foreach (var conn in _activeTab.Connections)
            {
                var from = _activeTab.Nodes.Find(n => n.Id == conn.FromId);
                var to = _activeTab.Nodes.Find(n => n.Id == conn.ToId);
                if (from == null || to == null) continue;

                double x1 = from.X + 20;
                double y1 = from.Y + 20;
                double x2 = to.X + 20;
                double y2 = to.Y + 20;

                bool bothKnown = from.State != LocationState.Undiscovered
                              && to.State != LocationState.Undiscovered;

                var line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(bothKnown
                        ? Color.FromRgb(0x4a, 0x7a, 0x4a)
                        : Color.FromRgb(0x2a, 0x3a, 0x2a)),
                    StrokeThickness = 1.5
                };
                MapCanvas.Children.Add(line);

                // Junction dots
                foreach (var (cx, cy) in new[] { (x1, y1), (x2, y2) })
                {
                    var dot = new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = new SolidColorBrush(bothKnown
                            ? Color.FromRgb(0x4a, 0x7a, 0x4a)
                            : Color.FromRgb(0x2a, 0x3a, 0x2a))
                    };
                    Canvas.SetLeft(dot, cx - 2);
                    Canvas.SetTop(dot, cy - 2);
                    MapCanvas.Children.Add(dot);
                }

                // Travel indicator
                if (conn.IsPlayerTravelling)
                {
                    double tx = x1 + (x2 - x1) * conn.TravelProgress;
                    double ty = y1 + (y2 - y1) * conn.TravelProgress;
                    var traveller = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = new SolidColorBrush(
                            Color.FromRgb(0x7a, 0xaa, 0x60))
                    };
                    Canvas.SetLeft(traveller, tx - 3);
                    Canvas.SetTop(traveller, ty - 3);
                    MapCanvas.Children.Add(traveller);
                }
            }

            // Draw nodes
            foreach (var node in _activeTab.Nodes)
                DrawNode(node);

            // Auto size canvas
            double maxX = 0, maxY = 0;
            foreach (var node in _activeTab.Nodes)
            {
                if (node.X + 80 > maxX) maxX = node.X + 80;
                if (node.Y + 60 > maxY) maxY = node.Y + 60;
            }
            MapCanvas.Width = maxX;
            MapCanvas.Height = maxY;
        }

        private void DrawNode(MapNode node)
        {
            bool isCurrent = node.Id == _currentLocationId;
            bool isTravelling = node.Id == _travellingToId;
            bool isUndiscovered = node.State == LocationState.Undiscovered;

            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string stateIcon = node.State switch
            {
                LocationState.Undiscovered => "□",
                LocationState.Discovered => "◈",
                LocationState.Visited => "◉",
                LocationState.Explored => "■",
                LocationState.Looted => "◆",
                _ => "□"
            };

            SpecialMarker effectiveSpecial = node.Special;
            if (node.HasDiscoveredSafeRoom &&
                node.Special == SpecialMarker.None)
                effectiveSpecial = SpecialMarker.SafeRoom;

            string specialIcon = effectiveSpecial switch
            {
                SpecialMarker.MainQuestActive => "★",
                SpecialMarker.MainQuestAvailable => "☆",
                SpecialMarker.SideQuestActive => "◈",
                SpecialMarker.SideQuestAvailable => "◇",
                SpecialMarker.SpecialQuestActive => "✦",
                SpecialMarker.SpecialQuestAvailable => "✧",
                SpecialMarker.Unavailable => "✕",
                SpecialMarker.Locked => "🔒",
                SpecialMarker.Danger => "▲",
                SpecialMarker.Deadly => "💀",
                SpecialMarker.SafeRoom => "⌂",
                _ => ""
            };

            double iconSize = node.Type == LocationType.Bridge ? 11 : 14;

            var iconRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var baseText = new TextBlock
            {
                Text = isUndiscovered ? "?" : node.BaseIcon,
                FontFamily = new FontFamily("Segoe UI Emoji"),
                FontSize = iconSize,
                Foreground = GetNodeColor(node, isCurrent),
                VerticalAlignment = VerticalAlignment.Center
            };

            var stateText = new TextBlock
            {
                Text = stateIcon,
                FontFamily = new FontFamily("Courier New"),
                FontSize = 9,
                Foreground = GetStateColor(node.State),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(1, 0, 0, 0)
            };

            var specialText = new TextBlock
            {
                Text = specialIcon,
                FontFamily = new FontFamily("Courier New"),
                FontSize = 9,
                Foreground = GetSpecialColor(effectiveSpecial),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(1, 0, 0, 0)
            };

            iconRow.Children.Add(baseText);
            if (!isUndiscovered) iconRow.Children.Add(stateText);
            if (effectiveSpecial != SpecialMarker.None &&
                effectiveSpecial != SpecialMarker.CurrentLocation)
                iconRow.Children.Add(specialText);

            var nameText = new TextBlock
            {
                Text = isUndiscovered ? "???" : node.Name,
                FontFamily = new FontFamily("Courier New"),
                FontSize = 8,
                Foreground = isUndiscovered
                    ? new SolidColorBrush(Color.FromRgb(0x3a, 0x4a, 0x3a))
                    : new SolidColorBrush(Color.FromRgb(0x6a, 0x8a, 0x6a)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            };

            if (isTravelling)
            {
                var travelLabel = new TextBlock
                {
                    Text = "...",
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 8,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0x7a, 0xaa, 0x60)),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                container.Children.Add(travelLabel);
            }

            if (isCurrent)
            {
                var highlight = new Border
                {
                    BorderBrush = new SolidColorBrush(
                        Color.FromRgb(0x7a, 0xaa, 0x60)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(3),
                    Child = iconRow
                };
                container.Children.Add(highlight);
            }
            else
            {
                container.Children.Add(iconRow);
            }

            container.Children.Add(nameText);

            Canvas.SetLeft(container, node.X);
            Canvas.SetTop(container, node.Y);
            MapCanvas.Children.Add(container);
        }

        // ── Tab management ───────────────────────

        private void RefreshTabs()
        {
            TabsPanel.Children.Clear();

            foreach (var tab in _openTabs)
            {
                var tabBtn = new Button
                {
                    Style = (Style)FindResource("TabButton"),
                    Tag = tab.Id
                };

                var tabContent = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                var tabTitle = new TextBlock
                {
                    Text = tab.Type == MapType.Region
                       ? $"▦ {tab.Title.ToUpper()}"
                       : $"◎ {tab.Title}",
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 9,
                    Foreground = tab == _activeTab
                        ? new SolidColorBrush(Color.FromRgb(0x7a, 0xaa, 0x60))
                        : tab.Type == MapType.Region
                            ? new SolidColorBrush(Color.FromRgb(0x4a, 0x8a, 0x4a))
                            : new SolidColorBrush(Color.FromRgb(0x4a, 0x6a, 0x8a)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                tabContent.Children.Add(tabTitle);

                // Close button — only if not current map
                if (!tab.IsCurrentMap)
                {
                    var closeBtn = new TextBlock
                    {
                        Text = " ×",
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 9,
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(0x6a, 0x4a, 0x4a)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Cursor = Cursors.Hand,
                        Tag = tab.Id
                    };
                    closeBtn.MouseDown += CloseTab_Click;
                    tabContent.Children.Add(closeBtn);
                }

                tabBtn.Content = tabContent;
                tabBtn.Click += TabBtn_Click;

                // Highlight active tab
                if (tab == _activeTab)
                    tabBtn.BorderBrush = new SolidColorBrush(
                        Color.FromRgb(0x7a, 0xaa, 0x60));

                TabsPanel.Children.Add(tabBtn);
            }
        }

        private void TabBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tabId)
            {
                var tab = _openTabs.FirstOrDefault(t => t.Id == tabId);
                if (tab != null)
                {
                    _activeTab = tab;
                    SectionTitle.Text = tab.Title.ToUpper();
                    RefreshTabs();
                    DrawMap();
                }
            }
        }

        private void CloseTab_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is TextBlock tb && tb.Tag is string tabId)
            {
                var tab = _openTabs.FirstOrDefault(t => t.Id == tabId);
                if (tab != null && !tab.IsCurrentMap)
                {
                    _openTabs.Remove(tab);
                    if (_activeTab == tab)
                        _activeTab = _openTabs.FirstOrDefault();
                    RefreshTabs();
                    DrawMap();
                }
            }
        }

        private void OpenMapsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MapsPopup.Visibility == Visibility.Visible)
            {
                MapsPopup.Visibility = Visibility.Collapsed;
                return;
            }

            // Build available maps list
            AvailableMapsPanel.Children.Clear();

            var available = _availableTabs
                .Where(t => t.IsUnlocked && !_openTabs.Any(o => o.Id == t.Id))
                .ToList();

            if (!available.Any())
            {
                AvailableMapsPanel.Children.Add(new TextBlock
                {
                    Text = "No other maps available",
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 9,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0x4a, 0x6a, 0x4a))
                });
            }
            else
            {
                foreach (var tab in available)
                {
                    var btn = new Button
                    {
                        Content = tab.Title,
                        Style = (Style)FindResource("LegendButton"),
                        Tag = tab.Id,
                        Margin = new Thickness(0, 2, 0, 2),
                        IsEnabled = _openTabs.Count < MaxTabs
                    };
                    btn.Click += AvailableMapBtn_Click;
                    AvailableMapsPanel.Children.Add(btn);
                }
            }

            MapsPopup.Visibility = Visibility.Visible;
        }

        private void AvailableMapBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tabId)
            {
                var tab = _availableTabs.FirstOrDefault(t => t.Id == tabId);
                if (tab != null && _openTabs.Count < MaxTabs)
                {
                    _openTabs.Add(tab);
                    _activeTab = tab;
                    MapsPopup.Visibility = Visibility.Collapsed;
                    RefreshTabs();
                    DrawMap();
                }
            }
        }

        // ── Legend ───────────────────────────────

        private void LegendToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            _legendExpanded = !_legendExpanded;
            LegendContent.Visibility = _legendExpanded
                ? Visibility.Visible : Visibility.Collapsed;
            LegendToggleBtn.Content = _legendExpanded ? "▼" : "▲";
        }

        // ── Drag & Zoom ──────────────────────────

        private void MapBorder_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            MapsPopup.Visibility = Visibility.Collapsed;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStart = e.GetPosition((UIElement)sender);
                _dragOriginX = MapTranslate.X;
                _dragOriginY = MapTranslate.Y;
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void MapBorder_MouseMove(object sender,
            MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition((UIElement)sender);
            var border = (FrameworkElement)sender;

            double newX = _dragOriginX + (pos.X - _dragStart.X);
            double newY = _dragOriginY + (pos.Y - _dragStart.Y);

            double padding = 50;
            double maxX = padding;
            double maxY = padding;
            double minX = -(Math.Max(0, MapCanvas.Width * MapScale.ScaleX
                           - border.ActualWidth) + padding);
            double minY = -(Math.Max(0, MapCanvas.Height * MapScale.ScaleY
                           - border.ActualHeight) + padding);

            MapTranslate.X = Math.Max(minX, Math.Min(maxX, newX));
            MapTranslate.Y = Math.Max(minY, Math.Min(maxY, newY));
        }

        private void MapBorder_MouseUp(object sender,
            MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void MapBorder_MouseWheel(object sender,
            MouseWheelEventArgs e)
        {
            var border = (UIElement)sender;
            var mousePos = e.GetPosition(border);

            double zoom = e.Delta > 0 ? 1.1 : 0.9;
            double newScale = MapScale.ScaleX * zoom;

            if (newScale < MinZoom || newScale > MaxZoom) return;

            MapTranslate.X = mousePos.X -
                (mousePos.X - MapTranslate.X) * zoom;
            MapTranslate.Y = mousePos.Y -
                (mousePos.Y - MapTranslate.Y) * zoom;

            MapScale.ScaleX = newScale;
            MapScale.ScaleY = newScale;
        }

        // ── Color helpers ────────────────────────

        private Brush GetNodeColor(MapNode node, bool isCurrent)
        {
            if (isCurrent)
                return new SolidColorBrush(Color.FromRgb(0x7a, 0xaa, 0x60));
            if (node.Type == LocationType.Bridge)
                return new SolidColorBrush(Color.FromRgb(0x8a, 0x7a, 0x60));
            if (node.Type == LocationType.ExpeditionPoint)
                return new SolidColorBrush(Color.FromRgb(0x60, 0x8a, 0xaa));
            if (node.State == LocationState.Undiscovered)
                return new SolidColorBrush(Color.FromRgb(0x3a, 0x4a, 0x3a));
            return new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0xb0));
        }

        private Brush GetStateColor(LocationState state)
        {
            return state switch
            {
                LocationState.Undiscovered => new SolidColorBrush(
                    Color.FromRgb(0x3a, 0x4a, 0x3a)),
                LocationState.Discovered => new SolidColorBrush(
                    Color.FromRgb(0x6a, 0x8a, 0x6a)),
                LocationState.Visited => new SolidColorBrush(
                    Color.FromRgb(0x7a, 0xaa, 0x60)),
                LocationState.Explored => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0xc8, 0x60)),
                LocationState.Looted => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0xa8, 0x40)),
                _ => new SolidColorBrush(Color.FromRgb(0x6a, 0x8a, 0x6a))
            };
        }

        private Brush GetSpecialColor(SpecialMarker special)
        {
            return special switch
            {
                SpecialMarker.MainQuestActive => new SolidColorBrush(
                    Color.FromRgb(0xff, 0xcc, 0x00)),
                SpecialMarker.MainQuestAvailable => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0xa8, 0x40)),
                SpecialMarker.SideQuestActive => new SolidColorBrush(
                    Color.FromRgb(0x60, 0xc8, 0xc8)),
                SpecialMarker.SideQuestAvailable => new SolidColorBrush(
                    Color.FromRgb(0x40, 0x8a, 0x8a)),
                SpecialMarker.SpecialQuestActive => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0x60, 0xc8)),
                SpecialMarker.SpecialQuestAvailable => new SolidColorBrush(
                    Color.FromRgb(0x8a, 0x40, 0x8a)),
                SpecialMarker.Unavailable => new SolidColorBrush(
                    Color.FromRgb(0x8a, 0x8a, 0x8a)),
                SpecialMarker.Locked => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0xc8, 0x40)),
                SpecialMarker.Danger => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0x78, 0x40)),
                SpecialMarker.Deadly => new SolidColorBrush(
                    Color.FromRgb(0xcc, 0x40, 0x40)),
                SpecialMarker.SafeRoom => new SolidColorBrush(
                    Color.FromRgb(0x60, 0xa8, 0xd0)),
                _ => new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0xb0))
            };
        }
    }
}