using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LostInAForgottenCity.Engine;

namespace LostInAForgottenCity.Controls
{
    public partial class MapPanel : UserControl
    {
        // ── Fields ────────────────────────────────
        private GeneratedMap? _activeMap = null;
        private bool _lookAroundActive = false;
        private double _bitmapOffsetX = 0;
        private double _bitmapOffsetY = 0;
        private bool _centerOnNextRender = false;

        // Tabs
        private List<MapTab> _openTabs = new();
        private List<MapTab> _availableTabs = new();
        private MapTab? _activeTab = null;

        // Drag
        private bool _isDragging = false;
        private Point _dragStart;
        private double _dragOriginX, _dragOriginY;
        private const double MinZoom = 0.5;
        private const double MaxZoom = 8.0;

        // Legend
        private bool _legendExpanded = true;

        private bool _isDirty = true;

        // Render timer
        private DispatcherTimer _renderTimer = new();

        public MapPanel()
        {
            InitializeComponent();
            _renderTimer.Interval =
                TimeSpan.FromMilliseconds(100);
            _renderTimer.Tick += (s, e) => Redraw();
            MapBorder.MouseMove += MapBorder_Hover;
            MapBorder.MouseLeave += (s, e) =>
                NodeTooltip.Visibility =
                    Visibility.Collapsed;
        }

        // ── Public API ────────────────────────────

        public void LoadMap(GeneratedMap map, string tabId, string tabTitle, MapType mapType)
        {
            _activeMap = map;

            var existing = _openTabs.FirstOrDefault(
                t => t.Id == tabId);
            if (existing == null)
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
                    Map = map
                };
                _openTabs.Add(tab);
                _activeTab = tab;
            }
            else
            {
                existing.Map = map;
                existing.IsCurrentMap = true;
                foreach (var t in _openTabs)
                    if (t.Id != tabId)
                        t.IsCurrentMap = false;
                _activeTab = existing;
            }

            SectionTitle.Text = tabTitle.ToUpper();
            RefreshTabs();
            MarkDirty();
            _renderTimer.Start();
        }


        public void SetLookAroundActive(bool active)
        {
            _lookAroundActive = active;
        }

        public void AddAvailableMap(MapTab tab)
        {
            if (!_availableTabs.Any(t => t.Id == tab.Id))
                _availableTabs.Add(tab);
        }

        public void SwitchToMap(GeneratedMap map,
            string tabId)
        {
            var tab = _openTabs.FirstOrDefault(
                t => t.Id == tabId);
            if (tab != null)
            {
                _activeMap = map;
                tab.Map = map;
                _activeTab = tab;
                SectionTitle.Text = tab.Title.ToUpper();
                RefreshTabs();
                MarkDirty();
            }
        }

        public MapTab? GetAvailableTab(string tabId)
            => _availableTabs.FirstOrDefault(
                t => t.Id == tabId)
            ?? _openTabs.FirstOrDefault(
                t => t.Id == tabId);

        public void SwitchToTab(string tabId)
        {
            var existing = _openTabs.FirstOrDefault(
                t => t.Id == tabId);
            if (existing?.Map != null)
            {
                _activeMap = existing.Map;
                _activeTab = existing;
                SectionTitle.Text =
                    existing.Title.ToUpper();
                RefreshTabs();
                MarkDirty();
                return;
            }

            var available = _availableTabs.FirstOrDefault(t => t.Id == tabId);
            if (available != null && _openTabs.Count < 5)
            {
                _openTabs.Add(available);
                _activeTab = available;
                _activeMap = available.Map;
                SectionTitle.Text =
                    available.Title.ToUpper();
                RefreshTabs();
                MarkDirty();
            }
        }

        // ── Rendering ─────────────────────────────
        public void MarkDirty()
        {
            _isDirty = true;
            Redraw();
            if (!_renderTimer.IsEnabled)
                _renderTimer.Start();
        }

        private void Redraw()
        {
            if (!_isDirty) return;
            if (_activeMap == null) return;

            int w = (int)MapBorder.ActualWidth;
            int h = (int)MapBorder.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var bitmap = MapGridRenderer.RenderToBitmap(_activeMap, w, h);

            _bitmapOffsetX = (w - bitmap.PixelWidth) / 2.0;
            _bitmapOffsetY = (h - bitmap.PixelHeight) / 2.0;

            // Centrează harta la primul load al unui tab nou
            if (_centerOnNextRender)
            {
                MapTranslate.X = _bitmapOffsetX;
                MapTranslate.Y = _bitmapOffsetY;
                MapScale.ScaleX = 1.0;
                MapScale.ScaleY = 1.0;
                _centerOnNextRender = false;
            }

            MapImage.Source = bitmap;
            MapImage.Width = bitmap.PixelWidth;
            MapImage.Height = bitmap.PixelHeight;
            MapImage.Margin = new Thickness(0);

            OverlayCanvas.Width = w;
            OverlayCanvas.Height = h;

            MapGridRenderer.RenderOverlays(
                OverlayCanvas, _activeMap, w, h,
                MapScale.ScaleX,
                MapTranslate.X,
                MapTranslate.Y);

            if (_activeMap != null)
                foreach (var conn in _activeMap.Connections
                    .Where(c => c.IsPlayerTravelling))
                    MapGridRenderer.RenderTravelDot(
                        OverlayCanvas, _activeMap, conn,
                        w, h, MapScale.ScaleX,
                        MapTranslate.X, MapTranslate.Y);

            _isDirty = false;

            bool anyTravelling = _activeMap?.Connections
                .Any(c => c.IsPlayerTravelling) ?? false;
            if (!anyTravelling)
                _renderTimer.Stop();
        }

        private void MapBorder_SizeChanged(object sender, SizeChangedEventArgs e) => MarkDirty();

        // ── Hover tooltip ─────────────────────────

        private void MapBorder_Hover(object sender,
            MouseEventArgs e)
        {
            if (_activeMap == null) return;

            var pos = e.GetPosition(MapBorder);
            int w = (int)MapBorder.ActualWidth;
            int h = (int)MapBorder.ActualHeight;

            double scaleX = w / (double)_activeMap.Width;
            double scaleY = h / (double)_activeMap.Height;
            double scale = Math.Min(scaleX, scaleY)
                * MapScale.ScaleX;

            int sx = (int)((pos.X - MapTranslate.X) / scale);
            int sy = (int)((pos.Y - MapTranslate.Y) / scale);

            if (!_activeMap.IsInBounds(sx, sy))
            {
                NodeTooltip.Visibility = Visibility.Collapsed;
                return;
            }

            var seg = _activeMap.GetSegment(sx, sy);
            if (seg.Type != SegmentType.Node ||
                seg.NodeId == null)
            {
                NodeTooltip.Visibility = Visibility.Collapsed;
                return;
            }

            var node = _activeMap.GetNode(seg.NodeId);
            if (node == null) return;

            ShowNodeTooltip(node, pos);
        }

        private void ShowNodeTooltip(
            GameMapNode node, Point pos)
        {
            TooltipContent.Children.Clear();

            // Icon + name
            string icon = node.State ==
                NodeState.Undiscovered
                ? "?" : node.Icon;
            string name = node.State ==
                NodeState.Undiscovered
                ? "???" : node.Name;

            TooltipContent.Children.Add(
                MakeTooltipText(
                    $"{icon}  {name}",
                    "#c8c8b0", 11));

            // State
            if (node.State != NodeState.Undiscovered)
            {
                string stateStr = node.State.ToString();
                TooltipContent.Children.Add(
                    MakeTooltipText(stateStr, "#6a8a6a", 9));
            }

            // Look around info
            if (_lookAroundActive &&
                node.State != NodeState.Undiscovered)
            {
                // Distance
                if (_activeMap != null &&
                    !string.IsNullOrEmpty(
                        _activeMap.PlayerNodeId))
                {
                    var dist = _activeMap.GetDistance(
                        _activeMap.PlayerNodeId, node.Id);
                    if (dist.HasValue)
                        TooltipContent.Children.Add(
                            MakeTooltipText(
                                $"Distance: {dist}",
                                "#8a8a7a", 9));
                }

                // Risk
                TooltipContent.Children.Add(
                    MakeTooltipText(
                        $"Risk: {node.Risk}",
                        GetRiskColor(node.Risk), 9));

                // Safe room
                if (node.HasSafeRoom)
                    TooltipContent.Children.Add(
                        MakeTooltipText(
                            "⌂ Safe Room",
                            "#60a8d0", 9));
            }

            NodeTooltip.Visibility = Visibility.Visible;
            Canvas.SetLeft(NodeTooltip, pos.X + 12);
            Canvas.SetTop(NodeTooltip, pos.Y + 12);

            // Keep tooltip inside bounds
            double maxX = MapBorder.ActualWidth - 150;
            double maxY = MapBorder.ActualHeight - 100;
            var left = Math.Min(pos.X + 12, maxX);
            var top = Math.Min(pos.Y + 12, maxY);

            NodeTooltip.Margin = new Thickness(
                left, top, 0, 0);
        }

        private TextBlock MakeTooltipText(
            string text, string color, double size)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Courier New"),
                FontSize = size,
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter
                        .ConvertFromString(color)),
                Margin = new Thickness(0, 1, 0, 1)
            };
        }

        private string GetRiskColor(RiskLevel risk)
            => risk switch
            {
                RiskLevel.Low => "#7aaa60",
                RiskLevel.Medium => "#c8a840",
                RiskLevel.High => "#cc4040",
                _ => "#7aaa60"
            };

        // ── Tab management ────────────────────────

        private void RefreshTabs()
        {
            TabsPanel.Children.Clear();

            foreach (var tab in _openTabs)
            {
                var btn = new Button
                {
                    FontFamily = new FontFamily(
                        "Courier New"),
                    FontSize = 9,
                    Background = new SolidColorBrush(
                        tab == _activeTab
                        ? Color.FromRgb(0x1a, 0x2a, 0x1a)
                        : Colors.Transparent),
                    BorderBrush = new SolidColorBrush(
                        tab == _activeTab
                        ? Color.FromRgb(0x7a, 0xaa, 0x60)
                        : Color.FromRgb(0x2a, 0x3a, 0x2a)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(4, 2, 4, 2),
                    Margin = new Thickness(0, 0, 3, 0),
                    Cursor = Cursors.Hand,
                    Tag = tab.Id
                };

                string prefix = tab.Type == MapType.Region
                    ? "▦ " : "◎ ";
                string title = tab.Type == MapType.Region
                    ? tab.Title.ToUpper()
                    : tab.Title;

                var content = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                content.Children.Add(new TextBlock
                {
                    Text = prefix + title,
                    Foreground = new SolidColorBrush(
                        tab == _activeTab
                        ? Color.FromRgb(0x7a, 0xaa, 0x60)
                        : Color.FromRgb(0x4a, 0x6a, 0x4a))
                });

                if (!tab.IsCurrentMap)
                {
                    var close = new TextBlock
                    {
                        Text = " ×",
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(
                                0x6a, 0x4a, 0x4a)),
                        Cursor = Cursors.Hand,
                        Tag = tab.Id
                    };
                    close.MouseDown += CloseTab_Click;
                    content.Children.Add(close);
                }

                btn.Content = content;
                btn.Click += TabBtn_Click;
                TabsPanel.Children.Add(btn);
            }
        }

        private void TabBtn_Click(object sender,
            RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.Tag is string tabId)
                SwitchToTab(tabId);
        }

        private void CloseTab_Click(object sender,
            MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is TextBlock tb &&
                tb.Tag is string tabId)
            {
                var tab = _openTabs.FirstOrDefault(
                    t => t.Id == tabId);
                if (tab != null && !tab.IsCurrentMap)
                {
                    _openTabs.Remove(tab);
                    if (_activeTab == tab)
                    {
                        _activeTab = _openTabs
                            .FirstOrDefault();
                        _activeMap = _activeTab?.Map;
                    }
                    RefreshTabs();
                    MarkDirty();
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
                AvailableMapsPanel.Children.Add(
                    new TextBlock
                    {
                        Text = "No other maps available",
                        FontFamily = new FontFamily(
                            "Courier New"),
                        FontSize = 9,
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(
                                0x4a, 0x6a, 0x4a))
                    });
            }
            else
            {
                foreach (var tab in available)
                {
                    var btn = new Button
                    {
                        Content = tab.Title,
                        FontFamily = new FontFamily(
                            "Courier New"),
                        FontSize = 9,
                        Background = Colors.Transparent
                            .ToString() == null
                            ? null
                            : new SolidColorBrush(
                                Colors.Transparent),
                        BorderThickness = new Thickness(0),
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(
                                0x4a, 0x6a, 0x4a)),
                        Cursor = Cursors.Hand,
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
            if (sender is Button btn &&
                btn.Tag is string tabId)
            {
                var tab = _availableTabs.FirstOrDefault(
                    t => t.Id == tabId);
                if (tab != null && _openTabs.Count < 5)
                {
                    _openTabs.Add(tab);
                    _activeTab = tab;
                    _activeMap = tab.Map;
                    MapsPopup.Visibility =
                        Visibility.Collapsed;
                    SectionTitle.Text =
                        tab.Title.ToUpper();
                    RefreshTabs();
                    MarkDirty();
                }
            }
        }

        private void LegendToggleBtn_Click(object sender,
            RoutedEventArgs e)
        {
            _legendExpanded = !_legendExpanded;
            LegendContent.Visibility = _legendExpanded
                ? Visibility.Visible
                : Visibility.Collapsed;
            LegendToggleBtn.Content =
                _legendExpanded ? "▼" : "▲";
        }

        // ── Drag & zoom ───────────────────────────

        private void MapBorder_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            MapsPopup.Visibility = Visibility.Collapsed;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStart = e.GetPosition(
                    (UIElement)sender);
                _dragOriginX = MapTranslate.X;
                _dragOriginY = MapTranslate.Y;
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void MapBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition((UIElement)sender);
            MapTranslate.X = _dragOriginX + (pos.X - _dragStart.X);
            MapTranslate.Y = _dragOriginY + (pos.Y - _dragStart.Y);
            MarkDirty(); // ← lipsea
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
            double zoom = e.Delta > 0 ? 1.15 : 0.87;
            double newScale = MapScale.ScaleX * zoom;

            if (newScale < MinZoom ||
                newScale > MaxZoom) return;

            MapTranslate.X = mousePos.X -
                (mousePos.X - MapTranslate.X) * zoom;
            MapTranslate.Y = mousePos.Y -
                (mousePos.Y - MapTranslate.Y) * zoom;

            MapScale.ScaleX = newScale;
            MapScale.ScaleY = newScale;
            MarkDirty(); // ← lipsea
        }
    }

    // ── Map tab ───────────────────────────────────

    public class MapTab
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public MapType Type { get; set; } = MapType.Region;
        public bool IsCurrentMap { get; set; } = false;
        public bool IsUnlocked { get; set; } = false;
        public GeneratedMap? Map { get; set; } = null;
    }

    public enum MapType
    {
        Region,
        Location
    }
}