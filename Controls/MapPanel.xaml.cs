using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LostInAForgottenCity.Controls
{
    public enum LocationState
    {
        Undiscovered, Discovered, Visited, Explored, Looted
    }

    public enum SpecialMarker
    {
        None, CurrentLocation,
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

    public enum TravelDistance
    {
        Immediate,
        Close,
        Near,
        Far,
        Distant
    }

    public class MapConnection
    {
        public string FromId { get; set; } = "";
        public string ToId { get; set; } = "";
        public TravelDistance Distance { get; set; } = TravelDistance.Close;
        public bool IsPlayerTravelling { get; set; } = false;
        public double TravelProgress { get; set; } = 0.0;
    }

    public class BorderEntry
    {
        public string Direction { get; set; } = "S"; // N S E W
        public double PositionRatio { get; set; } = 0.5; // 0.0-1.0 along border
        public string ConnectsToId { get; set; } = ""; // nearest landmark id
        public bool IsPlayerHere { get; set; } = false;
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
        public List<BorderEntry> BorderEntries { get; set; } = new(); // ← new
    }

    public partial class MapPanel : UserControl
    {
        // ── Constants ────────────────────────────
        private const int SegmentPixelLength = 18;  // pixels per dash segment
        private const int SegmentTravelMs = 500;    // ms per segment

        // ── Fields ───────────────────────────────
        private List<MapTab> _openTabs = new();
        private List<MapTab> _availableTabs = new();
        private MapTab? _activeTab = null;
        private string _currentLocationId = "";

        // Travel
        private DispatcherTimer _travelTimer = new();
        private string _travelFromId = "";
        private string _travelToId = "";
        private int _totalSegments = 0;
        private int _currentSegment = 0;
        public Action? OnTravelComplete { get; set; }
        public bool IsTravelling => _travelTimer.IsEnabled;

        // Drag
        private bool _isDragging = false;
        private Point _dragStart;
        private double _dragOriginX, _dragOriginY;
        private const double MinZoom = 0.5;
        private const double MaxZoom = 3.0;

        // Legend
        private bool _legendExpanded = true;

        public MapPanel()
        {
            InitializeComponent();
            _travelTimer.Interval =
                TimeSpan.FromMilliseconds(SegmentTravelMs);
            _travelTimer.Tick += TravelTick;
        }

        // ── Public API ───────────────────────────

        public void LoadMap(List<MapNode> nodes,
                           List<MapConnection> connections,
                           string tabId, string tabTitle,
                           MapType mapType,
                           string currentLocationId,
                           string travellingToId = "")
        {
            _currentLocationId = currentLocationId;

            var existingTab = _openTabs.FirstOrDefault(
                t => t.Id == tabId);
            if (existingTab == null)
            {
                foreach (var t in _openTabs)
                    t.IsCurrentMap = false;

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

        public void UpdateCurrentLocation(string locationId)
        {
            _currentLocationId = locationId;
            DrawMap();
        }

        // ── Travel ───────────────────────────────

        public void StartTravel(string fromId, string toId)
        {
            if (_activeTab == null) return;

            var conn = _activeTab.Connections.FirstOrDefault(
                c => (c.FromId == fromId && c.ToId == toId) ||
                     (c.FromId == toId && c.ToId == fromId));
            if (conn == null) return;

            _travelFromId = fromId;
            _travelToId = toId;
            _totalSegments = GetTravelSegments(conn.Distance);
            _currentSegment = 0;

            conn.IsPlayerTravelling = true;
            conn.TravelProgress = 0;

            _travelTimer.Interval =
                TimeSpan.FromMilliseconds(SegmentTravelMs);
            _travelTimer.Start();
            DrawMap();
        }

        private void TravelTick(object? sender, EventArgs e)
        {
            _currentSegment++;
            double progress = _currentSegment / (double)_totalSegments;

            var conn = _activeTab?.Connections.FirstOrDefault(
                c => (c.FromId == _travelFromId &&
                      c.ToId == _travelToId) ||
                     (c.FromId == _travelToId &&
                      c.ToId == _travelFromId));

            if (conn != null)
                conn.TravelProgress = progress;

            DrawMap();

            if (_currentSegment >= _totalSegments)
            {
                _travelTimer.Stop();
                if (conn != null)
                    conn.IsPlayerTravelling = false;
                _currentLocationId = _travelToId;
                DrawMap();
                OnTravelComplete?.Invoke();
            }
        }

        // Travel segments for animation (how many ticks)
        private int GetTravelSegments(TravelDistance distance)
        {
            return distance switch
            {
                TravelDistance.Immediate => 2,
                TravelDistance.Close => 4,
                TravelDistance.Near => 6,
                TravelDistance.Far => 8,
                TravelDistance.Distant => 12,
                _ => 4
            };
        }

        // Visual dashes — communicates distance to player
        private int GetVisualDashes(TravelDistance distance)
        {
            return distance switch
            {
                TravelDistance.Immediate => 3,
                TravelDistance.Close => 5,
                TravelDistance.Near => 7,
                TravelDistance.Far => 10,
                TravelDistance.Distant => 14,
                _ => 5
            };
        }

        // Travel time in minutes
        public static int GetTravelMinutes(TravelDistance distance,
            bool isLandmark = false)
        {
            if (isLandmark)
            {
                return distance switch
                {
                    TravelDistance.Immediate => 5,
                    TravelDistance.Close => 10,
                    TravelDistance.Near => 15,
                    TravelDistance.Far => 20,
                    TravelDistance.Distant => 25,
                    _ => 10
                };
            }
            return distance switch
            {
                TravelDistance.Immediate => 30,
                TravelDistance.Close => 45,
                TravelDistance.Near => 60,
                TravelDistance.Far => 75,
                TravelDistance.Distant => 90,
                _ => 45
            };
        }

        // Sleep cost per distance category
        public static int GetSleepCost(TravelDistance distance,
            bool isLandmark = false)
        {
            if (isLandmark)
            {
                return distance switch
                {
                    TravelDistance.Immediate => 1,
                    TravelDistance.Close => 2,
                    TravelDistance.Near => 3,
                    TravelDistance.Far => 4,
                    TravelDistance.Distant => 5,
                    _ => 2
                };
            }
            return distance switch
            {
                TravelDistance.Immediate => 5,
                TravelDistance.Close => 8,
                TravelDistance.Near => 12,
                TravelDistance.Far => 16,
                TravelDistance.Distant => 20,
                _ => 8
            };
        }


        // ── Drawing ──────────────────────────────

        private void DrawMap()
        {
            if (_activeTab == null) return;

            MapCanvas.Children.Clear();
            SectionTitle.Text = _activeTab.Title.ToUpper();

            // Draw border entries FIRST (behind everything)
            foreach (var entry in _activeTab.BorderEntries)
                DrawBorderEntry(entry);

            // Draw connections
            foreach (var conn in _activeTab.Connections)
            {
                var from = _activeTab.Nodes.Find(n => n.Id == conn.FromId);
                var to = _activeTab.Nodes.Find(n => n.Id == conn.ToId);
                if (from == null || to == null) continue;
                DrawRoad(from, to, conn);
            }

            // Draw nodes (on top)
            foreach (var node in _activeTab.Nodes)
                DrawNode(node);

            // Size canvas
            double maxX = 0, maxY = 0;
            foreach (var node in _activeTab.Nodes)
            {
                if (node.X + NodeBoxWidth + 10 > maxX)
                    maxX = node.X + NodeBoxWidth + 10;
                if (node.Y + NodeBoxHeight + 10 > maxY)
                    maxY = node.Y + NodeBoxHeight + 10;
            }
            MapCanvas.Width = Math.Max(maxX, 200);
            MapCanvas.Height = Math.Max(maxY, 200);
        }

        private void DrawBorderEntry(BorderEntry entry)
        {
            double cw = MapCanvas.Width > 0 ? MapCanvas.Width : 280;
            double ch = MapCanvas.Height > 0 ? MapCanvas.Height : 300;

            // Entry point on border
            double ex, ey;
            switch (entry.Direction)
            {
                case "S": ex = entry.PositionRatio * cw; ey = ch; break;
                case "N": ex = entry.PositionRatio * cw; ey = 0; break;
                case "W": ex = 0; ey = entry.PositionRatio * ch; break;
                case "E": ex = cw; ey = entry.PositionRatio * ch; break;
                default: return;
            }

            // Arrow indicator on border
            var arrowColor = Color.FromRgb(0x7a, 0xaa, 0x60);
            const double as2 = 5;
            PointCollection arrowPts = entry.Direction switch
            {
                "S" => new PointCollection {
            new(ex - as2, ey),
            new(ex + as2, ey),
            new(ex, ey - as2 * 1.5) },
                "N" => new PointCollection {
            new(ex - as2, ey),
            new(ex + as2, ey),
            new(ex, ey + as2 * 1.5) },
                "W" => new PointCollection {
            new(ex, ey - as2),
            new(ex, ey + as2),
            new(ex + as2 * 1.5, ey) },
                _ => new PointCollection {
            new(ex, ey - as2),
            new(ex, ey + as2),
            new(ex - as2 * 1.5, ey) }
            };

            MapCanvas.Children.Add(new Polygon
            {
                Points = arrowPts,
                Fill = new SolidColorBrush(arrowColor)
            });

            // Path from border to nearest landmark
            var nearest = _activeTab?.Nodes.FirstOrDefault(
                n => n.Id == entry.ConnectsToId);
            if (nearest != null)
            {
                Point edge = GetBoxEdgePoint(nearest, ex, ey);
                DrawDashedLine(ex, ey, edge.X, edge.Y, arrowColor, 5);
            }

            // Player dot at border if here
            if (entry.IsPlayerHere)
                DrawTravelDot(ex, ey);
        }

        private void DrawRoad(MapNode from, MapNode to,
            MapConnection conn)
        {
            Point center1 = GetBoxCenter(from);
            Point center2 = GetBoxCenter(to);
            Point p1 = GetBoxEdgePoint(from, center2.X, center2.Y);
            Point p2 = GetBoxEdgePoint(to, center1.X, center1.Y);

            bool bothKnown =
                from.State != LocationState.Undiscovered &&
                to.State != LocationState.Undiscovered;

            Color roadColor = bothKnown
                ? Color.FromRgb(0x4a, 0x7a, 0x4a)
                : Color.FromRgb(0x2a, 0x3a, 0x2a);

            // Use distance category for dash count
            int dashes = GetVisualDashes(conn.Distance);
            DrawDashedLine(p1.X, p1.Y, p2.X, p2.Y,
                roadColor, dashes);

            // Travel dot
            if (conn.IsPlayerTravelling)
            {
                bool forward = conn.FromId == _travelFromId;
                double progress = forward
                    ? conn.TravelProgress
                    : 1.0 - conn.TravelProgress;

                double px = p1.X + (p2.X - p1.X) * progress;
                double py = p1.Y + (p2.Y - p1.Y) * progress;
                DrawTravelDot(px, py);
            }
        }

        private void DrawDashedLine(double x1, double y1,
            double x2, double y2, Color color,
            int dashCount, double thickness = 1.5)
        {
            // Distribute fixed number of dashes across the line
            // regardless of physical length
            double dashLength = 0.6 / dashCount;

            for (int s = 0; s < dashCount; s++)
            {
                double t1 = s / (double)dashCount;
                double t2 = t1 + dashLength;

                MapCanvas.Children.Add(new Line
                {
                    X1 = x1 + (x2 - x1) * t1,
                    Y1 = y1 + (y2 - y1) * t1,
                    X2 = x1 + (x2 - x1) * t2,
                    Y2 = y1 + (y2 - y1) * t2,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = thickness
                });
            }
        }

        private void DrawTravelDot(double x, double y)
        {
            var dot = new Ellipse
            {
                Width = 7,
                Height = 7,
                Fill = new SolidColorBrush(
                    Color.FromRgb(0x7a, 0xaa, 0x60)),
                Stroke = new SolidColorBrush(
                    Color.FromRgb(0x1a, 0x2a, 0x1a)),
                StrokeThickness = 1
            };
            Canvas.SetLeft(dot, x - 3.5);
            Canvas.SetTop(dot, y - 3.5);
            MapCanvas.Children.Add(dot);
        }

        private int CalculateSegments(double x1, double y1,
            double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return Math.Max(2, (int)(distance / SegmentPixelLength));
        }

        private void DrawNode(MapNode node)
        {
            bool isCurrent = node.Id == _currentLocationId;
            bool isUndiscovered = node.State == LocationState.Undiscovered;

            SpecialMarker effectiveSpecial = node.Special;
            if (node.HasDiscoveredSafeRoom &&
                node.Special == SpecialMarker.None)
                effectiveSpecial = SpecialMarker.SafeRoom;

            // ── Box border ────────────────────────────
            var box = new Border
            {
                Width = NodeBoxWidth,
                Height = NodeBoxHeight,
                BorderBrush = isCurrent
                    ? new SolidColorBrush(Color.FromRgb(0x7a, 0xaa, 0x60))
                    : isUndiscovered
                        ? new SolidColorBrush(Color.FromRgb(0x2a, 0x3a, 0x2a))
                        : new SolidColorBrush(Color.FromRgb(0x3a, 0x5a, 0x3a)),
                BorderThickness = new Thickness(isCurrent ? 2 : 1),
                Background = new SolidColorBrush(
                    Color.FromRgb(0x0a, 0x12, 0x0a)),
                Padding = new Thickness(3, 2, 3, 2)
            };

            // ── Content ───────────────────────────────
            var content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Icon row
            var iconRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            iconRow.Children.Add(new TextBlock
            {
                Text = isUndiscovered ? "?" : node.BaseIcon,
                FontFamily = new FontFamily("Segoe UI Emoji"),
                FontSize = 13,
                Foreground = GetNodeColor(node, isCurrent),
                VerticalAlignment = VerticalAlignment.Center
            });

            if (!isUndiscovered)
            {
                iconRow.Children.Add(new TextBlock
                {
                    Text = node.State switch
                    {
                        LocationState.Discovered => "◈",
                        LocationState.Visited => "◉",
                        LocationState.Explored => "■",
                        LocationState.Looted => "◆",
                        _ => "□"
                    },
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 8,
                    Foreground = GetStateColor(node.State),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(1, 0, 0, 0)
                });
            }

            if (effectiveSpecial != SpecialMarker.None &&
                effectiveSpecial != SpecialMarker.CurrentLocation)
            {
                iconRow.Children.Add(new TextBlock
                {
                    Text = effectiveSpecial switch
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
                    },
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 8,
                    Foreground = GetSpecialColor(effectiveSpecial),
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(1, 0, 0, 0)
                });
            }

            content.Children.Add(iconRow);

            // Name
            content.Children.Add(new TextBlock
            {
                Text = isUndiscovered ? "???" : node.Name,
                FontFamily = new FontFamily("Courier New"),
                FontSize = 7,
                Foreground = isUndiscovered
                    ? new SolidColorBrush(Color.FromRgb(0x3a, 0x4a, 0x3a))
                    : new SolidColorBrush(Color.FromRgb(0x8a, 0xaa, 0x8a)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = NodeBoxWidth - 8
            });

            box.Child = content;

            Canvas.SetLeft(box, node.X);
            Canvas.SetTop(box, node.Y);
            MapCanvas.Children.Add(box);
        }

        public void UpdateNodeState(string tabId, string nodeId,
            LocationState newState,
            SpecialMarker special = SpecialMarker.None)
        {
            var tab = _openTabs.FirstOrDefault(t => t.Id == tabId)
                ?? _availableTabs.FirstOrDefault(t => t.Id == tabId);
            if (tab == null) return;

            var node = tab.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null) return;

            node.State = newState;
            if (special != SpecialMarker.None)
                node.Special = special;

            if (_activeTab?.Id == tabId)
                DrawMap();
        }

        // ── Random layout ─────────────────────────

        public static void GenerateLayout(List<MapNode> nodes,
            List<MapConnection> connections,
            double mapWidth = 280,
            double mapHeight = 300,
            int seed = -1)
        {
            var random = seed >= 0
                ? new Random(seed) : new Random();

            int padding = 20;
            double yMin = padding;
            double yMax = mapHeight - NodeBoxHeight - padding;
            double xMin = padding;
            double xMax = mapWidth - NodeBoxWidth - padding;

            // Place nodes south→north (index 0 = south)
            for (int i = 0; i < nodes.Count; i++)
            {
                double yFraction = nodes.Count > 1
                    ? 1.0 - (i / (double)(nodes.Count - 1))
                    : 0.5;

                double yCenter = yMin + yFraction * (yMax - yMin);
                double yVariance = Math.Min(15,
                    (yMax - yMin) / nodes.Count * 0.25);

                nodes[i].Y = Math.Max(yMin, Math.Min(yMax,
                    yCenter + (random.NextDouble() - 0.5)
                    * yVariance));

                nodes[i].X = xMin +
                    random.NextDouble() * (xMax - xMin);
            }

            // Adjust X positions based on distance category
            // to give visual hint of distance
            foreach (var conn in connections)
            {
                var from = nodes.FirstOrDefault(
                    n => n.Id == conn.FromId);
                var to = nodes.FirstOrDefault(
                    n => n.Id == conn.ToId);
                if (from == null || to == null) continue;

                // Target pixel separation based on distance
                double targetDist = conn.Distance switch
                {
                    TravelDistance.Immediate => 80,
                    TravelDistance.Close => 110,
                    TravelDistance.Near => 140,
                    TravelDistance.Far => 175,
                    TravelDistance.Distant => 210,
                    _ => 110
                };

                double cx1 = from.X + NodeBoxWidth / 2;
                double cy1 = from.Y + NodeBoxHeight / 2;
                double cx2 = to.X + NodeBoxWidth / 2;
                double cy2 = to.Y + NodeBoxHeight / 2;

                double current = Math.Sqrt(
                    (cx2 - cx1) * (cx2 - cx1) +
                    (cy2 - cy1) * (cy2 - cy1));

                // If too close, push nodes apart horizontally
                if (current < targetDist * 0.6)
                {
                    double push = (targetDist * 0.6 - current) / 2;
                    from.X = Math.Max(xMin, Math.Min(
                        xMax, from.X - push));
                    to.X = Math.Max(xMin, Math.Min(
                        xMax, to.X + push));
                }
            }
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
                        ? new SolidColorBrush(
                            Color.FromRgb(0x7a, 0xaa, 0x60))
                        : tab.Type == MapType.Region
                            ? new SolidColorBrush(
                                Color.FromRgb(0x4a, 0x8a, 0x4a))
                            : new SolidColorBrush(
                                Color.FromRgb(0x4a, 0x6a, 0x8a)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                tabContent.Children.Add(tabTitle);

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
                var tab = _openTabs.FirstOrDefault(
                    t => t.Id == tabId);
                if (tab != null)
                {
                    _activeTab = tab;
                    SectionTitle.Text = tab.Title.ToUpper();
                    RefreshTabs();
                    DrawMap();
                }
            }
        }

        private void CloseTab_Click(object sender,
            MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is TextBlock tb && tb.Tag is string tabId)
            {
                var tab = _openTabs.FirstOrDefault(
                    t => t.Id == tabId);
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

        private void OpenMapsBtn_Click(object sender,
            RoutedEventArgs e)
        {
            if (MapsPopup.Visibility == Visibility.Visible)
            {
                MapsPopup.Visibility = Visibility.Collapsed;
                return;
            }

            AvailableMapsPanel.Children.Clear();

            var available = _availableTabs
                .Where(t => t.IsUnlocked &&
                    !_openTabs.Any(o => o.Id == t.Id))
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
                        IsEnabled = _openTabs.Count < 5
                    };
                    btn.Click += AvailableMapBtn_Click;
                    AvailableMapsPanel.Children.Add(btn);
                }
            }

            MapsPopup.Visibility = Visibility.Visible;
        }

        private void AvailableMapBtn_Click(object sender,
            RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tabId)
            {
                var tab = _availableTabs.FirstOrDefault(
                    t => t.Id == tabId);
                if (tab != null && _openTabs.Count < 5)
                {
                    _openTabs.Add(tab);
                    _activeTab = tab;
                    MapsPopup.Visibility = Visibility.Collapsed;
                    RefreshTabs();
                    DrawMap();
                }
            }
        }

        // Set border entry for a tab
        public void SetBorderEntry(string tabId,
            string direction, double positionRatio,
            string nearestLandmarkId,
            bool isPlayerHere = false)
        {
            var tab = _openTabs.FirstOrDefault(t => t.Id == tabId)
                ?? _availableTabs.FirstOrDefault(t => t.Id == tabId);
            if (tab == null) return;

            tab.BorderEntries.Clear();
            tab.BorderEntries.Add(new BorderEntry
            {
                Direction = direction,
                PositionRatio = positionRatio,
                ConnectsToId = nearestLandmarkId,
                IsPlayerHere = isPlayerHere
            });

            if (_activeTab?.Id == tabId) DrawMap();
        }

        // Find nearest landmark to a border direction
        public string FindNearestLandmark(string tabId,
            string direction)
        {
            var tab = _openTabs.FirstOrDefault(t => t.Id == tabId)
                ?? _availableTabs.FirstOrDefault(t => t.Id == tabId);
            if (tab == null || !tab.Nodes.Any()) return "";

            double cw = 280, ch = 300;
            double bx = direction switch
            {
                "W" => 0,
                "E" => cw,
                _ => cw / 2
            };
            double by = direction switch
            {
                "S" => ch,
                "N" => 0,
                _ => ch / 2
            };

            return tab.Nodes
                .OrderBy(n =>
                {
                    double cx = n.X + NodeBoxWidth / 2;
                    double cy = n.Y + NodeBoxHeight / 2;
                    double dx = cx - bx;
                    double dy = cy - by;
                    return Math.Sqrt(dx * dx + dy * dy);
                })
                .First().Id;
        }

        // ── Legend ───────────────────────────────

        private void LegendToggleBtn_Click(object sender,
            RoutedEventArgs e)
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
            double minX = -(Math.Max(0,
                MapCanvas.Width * MapScale.ScaleX
                - border.ActualWidth) + padding);
            double minY = -(Math.Max(0,
                MapCanvas.Height * MapScale.ScaleY
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
                return new SolidColorBrush(
                    Color.FromRgb(0x7a, 0xaa, 0x60));
            if (node.Type == LocationType.Bridge)
                return new SolidColorBrush(
                    Color.FromRgb(0x8a, 0x7a, 0x60));
            if (node.Type == LocationType.ExpeditionPoint)
                return new SolidColorBrush(
                    Color.FromRgb(0x60, 0x8a, 0xaa));
            if (node.State == LocationState.Undiscovered)
                return new SolidColorBrush(
                    Color.FromRgb(0x3a, 0x4a, 0x3a));
            return new SolidColorBrush(
                Color.FromRgb(0xc8, 0xc8, 0xb0));
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
                _ => new SolidColorBrush(
                    Color.FromRgb(0x6a, 0x8a, 0x6a))
            };
        }

        private Brush GetSpecialColor(SpecialMarker special)
        {
            return special switch
            {
                SpecialMarker.MainQuestActive => new SolidColorBrush(Color.FromRgb(0xff, 0xcc, 0x00)),
                SpecialMarker.MainQuestAvailable => new SolidColorBrush(Color.FromRgb(0xc8, 0xa8, 0x40)),
                SpecialMarker.SideQuestActive => new SolidColorBrush(Color.FromRgb(0x60, 0xc8, 0xc8)),
                SpecialMarker.SideQuestAvailable => new SolidColorBrush(Color.FromRgb(0x40, 0x8a, 0x8a)),
                SpecialMarker.SpecialQuestActive => new SolidColorBrush(Color.FromRgb(0xc8, 0x60, 0xc8)),
                SpecialMarker.SpecialQuestAvailable => new SolidColorBrush(Color.FromRgb(0x8a, 0x40, 0x8a)),
                SpecialMarker.Unavailable => new SolidColorBrush(Color.FromRgb(0x8a, 0x8a, 0x8a)),
                SpecialMarker.Locked => new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0x40)),
                SpecialMarker.Danger => new SolidColorBrush(Color.FromRgb(0xc8, 0x78, 0x40)),
                SpecialMarker.Deadly => new SolidColorBrush(Color.FromRgb(0xcc, 0x40, 0x40)),
                SpecialMarker.SafeRoom => new SolidColorBrush(Color.FromRgb(0x60, 0xa8, 0xd0)),
                _ => new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0xb0))
            };
        }
        public MapTab? GetAvailableTab(string tabId)
        {
            return _availableTabs.FirstOrDefault(t => t.Id == tabId)
                ?? _openTabs.FirstOrDefault(t => t.Id == tabId);
        }

        public void SwitchToTab(string tabId)
        {
            // Check if already open
            var existing = _openTabs.FirstOrDefault(
                t => t.Id == tabId);
            if (existing != null)
            {
                _activeTab = existing;
                RefreshTabs();
                DrawMap();
                return;
            }

            // Add from available
            var available = _availableTabs.FirstOrDefault(
                t => t.Id == tabId);
            if (available != null && _openTabs.Count < 5)
            {
                _openTabs.Add(available);
                _activeTab = available;
                RefreshTabs();
                DrawMap();
            }
        }

        // ── Node box dimensions ───────────────────────
        private const double NodeBoxWidth = 70;
        private const double NodeBoxHeight = 46;

        // ── Get box center ────────────────────────────
        private static Point GetBoxCenter(MapNode node) =>
            new(node.X + NodeBoxWidth / 2, node.Y + NodeBoxHeight / 2);

        // ── Get point where line from (targetX,targetY) 
        //    intersects box edge ─────────────────────────
        private static Point GetBoxEdgePoint(MapNode node,
            double targetX, double targetY)
        {
            double cx = node.X + NodeBoxWidth / 2;
            double cy = node.Y + NodeBoxHeight / 2;
            double hw = NodeBoxWidth / 2;
            double hh = NodeBoxHeight / 2;

            double dx = targetX - cx;
            double dy = targetY - cy;

            if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
                return new Point(cx, cy);

            // Which edge does the line exit through?
            if (Math.Abs(dx) * hh >= Math.Abs(dy) * hw)
            {
                // Left or right edge
                double sign = dx > 0 ? 1 : -1;
                double x = cx + sign * hw;
                double y = cy + (Math.Abs(dx) > 0.001
                    ? dy * hw / Math.Abs(dx) : 0);
                y = Math.Max(cy - hh, Math.Min(cy + hh, y));
                return new Point(x, y);
            }
            else
            {
                // Top or bottom edge
                double sign = dy > 0 ? 1 : -1;
                double y = cy + sign * hh;
                double x = cx + (Math.Abs(dy) > 0.001
                    ? dx * hh / Math.Abs(dy) : 0);
                x = Math.Max(cx - hw, Math.Min(cx + hw, x));
                return new Point(x, y);
            }
        }

    }
}