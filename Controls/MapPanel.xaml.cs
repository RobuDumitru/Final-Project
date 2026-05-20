using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LostInAForgottenCity.Controls
{
    public enum LocationState
    {
        Undiscovered,
        Discovered,
        Visited,
        Explored,
        Looted
    }

    public enum SpecialMarker
    {
        None,
        CurrentLocation,
        MainQuestActive,
        MainQuestAvailable,
        SideQuestActive,
        SideQuestAvailable,
        SpecialQuestActive,
        SpecialQuestAvailable,
        Unavailable,
        Locked,
        Danger,
        Deadly,
        SafeRoom
    }

    public enum LocationType
    {
        Normal,
        Bridge,
        ExpeditionPoint
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
        public bool HasDiscoveredSafeRoom { get; set; } = false;  // Fixed: added semicolon
    }

    public class MapConnection
    {
        public string FromId { get; set; } = "";
        public string ToId { get; set; } = "";
        public bool IsPlayerTravelling { get; set; } = false;
        public double TravelProgress { get; set; } = 0.0;
    }

    public partial class MapPanel : UserControl
    {
        private List<MapNode> _nodes = new();
        private List<MapConnection> _connections = new();
        private string _currentLocationId = "";
        private string _travellingToId = "";

        // Drag state
        private bool _isDragging = false;
        private Point _dragStart;
        private double _dragOriginX;
        private double _dragOriginY;

        // Zoom limits
        private const double MinZoom = 0.5;
        private const double MaxZoom = 3.0;

        public MapPanel()
        {
            InitializeComponent();
        }

        public void LoadMap(List<MapNode> nodes,
                           List<MapConnection> connections,
                           string section,
                           string currentLocationId,
                           string travellingToId = "")
        {
            _nodes = nodes;
            _connections = connections;
            _currentLocationId = currentLocationId;
            _travellingToId = travellingToId;
            SectionTitle.Text = section.ToUpper();
            DrawMap();
        }

        private void DrawMap()
        {
            MapCanvas.Children.Clear();

            // Draw connections
            foreach (var conn in _connections)
            {
                var from = _nodes.Find(n => n.Id == conn.FromId);
                var to = _nodes.Find(n => n.Id == conn.ToId);
                if (from == null || to == null) continue;

                double x1 = from.X + 20;
                double y1 = from.Y + 20;
                double x2 = to.X + 20;
                double y2 = to.Y + 20;

                bool bothKnown = from.State != LocationState.Undiscovered
                              && to.State != LocationState.Undiscovered;

                // Road line — solid, natural
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

                // Junction dot at FROM end
                var dot1 = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(bothKnown
                        ? Color.FromRgb(0x4a, 0x7a, 0x4a)
                        : Color.FromRgb(0x2a, 0x3a, 0x2a))
                };
                Canvas.SetLeft(dot1, x1 - 2);
                Canvas.SetTop(dot1, y1 - 2);
                MapCanvas.Children.Add(dot1);

                // Junction dot at TO end
                var dot2 = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(bothKnown
                        ? Color.FromRgb(0x4a, 0x7a, 0x4a)
                        : Color.FromRgb(0x2a, 0x3a, 0x2a))
                };
                Canvas.SetLeft(dot2, x2 - 2);
                Canvas.SetTop(dot2, y2 - 2);
                MapCanvas.Children.Add(dot2);

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
            foreach (var node in _nodes)
                DrawNode(node);

            // Auto size canvas
            double maxX = 0, maxY = 0;
            foreach (var node in _nodes)
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

            // State icon
            string stateIcon = node.State switch
            {
                LocationState.Undiscovered => "□",
                LocationState.Discovered => "◈",
                LocationState.Visited => "◉",
                LocationState.Explored => "■",
                LocationState.Looted => "◆",
                _ => "□"
            };

            // Special marker — safe room only if discovered
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

            // Icon size based on type
            double iconSize = node.Type switch
            {
                LocationType.Bridge => 11,
                LocationType.ExpeditionPoint => 14,
                _ => 14
            };

            // Icon row
            var iconRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Base icon
            var baseText = new TextBlock
            {
                Text = isUndiscovered ? "?" : node.BaseIcon,
                FontFamily = new FontFamily("Segoe UI Emoji"),
                FontSize = iconSize,
                Foreground = GetNodeColor(node, isCurrent),
                VerticalAlignment = VerticalAlignment.Center
            };

            // State icon
            var stateText = new TextBlock
            {
                Text = stateIcon,
                FontFamily = new FontFamily("Courier New"),
                FontSize = 9,
                Foreground = GetStateColor(node.State),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(1, 0, 0, 0)
            };

            // Special icon
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
            if (!isUndiscovered)
                iconRow.Children.Add(stateText);
            if (effectiveSpecial != SpecialMarker.None)
                iconRow.Children.Add(specialText);

            // Location name
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

            // Travelling indicator
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

            // Current location = green border only
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
                SpecialMarker.Danger => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0x78, 0x40)),
                SpecialMarker.Locked => new SolidColorBrush(
                    Color.FromRgb(0xc8, 0xc8, 0x40)),
                SpecialMarker.Deadly => new SolidColorBrush(
                    Color.FromRgb(0xcc, 0x40, 0x40)),
                SpecialMarker.SafeRoom => new SolidColorBrush(
                    Color.FromRgb(0x60, 0xa8, 0xd0)),
                _ => new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0xb0))
            };
        }

        // ==================== DRAG AND ZOOM EVENT HANDLERS ====================

        private void MapBorder_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStart = e.GetPosition((UIElement)sender);
                _dragOriginX = MapTranslate.X;
                _dragOriginY = MapTranslate.Y;
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void MapBorder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition((UIElement)sender);
            var border = (FrameworkElement)sender;

            double newX = _dragOriginX + (pos.X - _dragStart.X);
            double newY = _dragOriginY + (pos.Y - _dragStart.Y);

            // Allow dragging with generous padding
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

            // Zoom toward mouse position
            MapTranslate.X = mousePos.X -
                (mousePos.X - MapTranslate.X) * zoom;
            MapTranslate.Y = mousePos.Y -
                (mousePos.Y - MapTranslate.Y) * zoom;

            MapScale.ScaleX = newScale;
            MapScale.ScaleY = newScale;
        }
        private bool _legendExpanded = true;

        private void LegendToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            _legendExpanded = !_legendExpanded;
            LegendContent.Visibility = _legendExpanded
                ? Visibility.Visible
                : Visibility.Collapsed;
            LegendToggleBtn.Content = _legendExpanded ? "▼" : "▲";
        }
    }
}