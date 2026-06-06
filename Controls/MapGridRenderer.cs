using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LostInAForgottenCity.Engine;

namespace LostInAForgottenCity.Controls
{
    public static class MapGridRenderer
    {
        // ── Segment colors ────────────────────────
        private static readonly Color ColorEmpty =
            Color.FromRgb(0x0a, 0x0f, 0x0a);
        private static readonly Color ColorBorder =
            Color.FromRgb(0x2a, 0x4a, 0x2a);
        private static readonly Color ColorConnection =
            Color.FromRgb(0x3a, 0x6a, 0x3a);
        private static readonly Color ColorRestriction =
            Color.FromRgb(0x2a, 0x2a, 0x1a);
        private static readonly Color ColorNodeUnknown =
            Color.FromRgb(0x1a, 0x2a, 0x1a);
        private static readonly Color ColorNodeDiscovered =
            Color.FromRgb(0x2a, 0x4a, 0x3a);
        private static readonly Color ColorNodeVisited =
            Color.FromRgb(0x3a, 0x6a, 0x4a);
        private static readonly Color ColorNodeExplored =
            Color.FromRgb(0x6a, 0x8a, 0x4a);
        private static readonly Color ColorNodeLooted =
            Color.FromRgb(0x8a, 0x7a, 0x3a);
        private static readonly Color ColorPlayer =
            Color.FromRgb(0x7a, 0xaa, 0x60);
        private static readonly Color ColorJunction =
            Color.FromRgb(0x6a, 0x9a, 0x6a);
        private static readonly Color ColorBorderEntry =
            Color.FromRgb(0x7a, 0xaa, 0x60);

        // ── Render map to bitmap ──────────────────
        // Renders at 1 segment = 1 pixel for full map
        // then scales to display size

        public static WriteableBitmap RenderToBitmap(
    GeneratedMap map,
    int displayWidth, int displayHeight)
        {
            double scaleX = displayWidth / (double)map.Width;
            double scaleY = displayHeight / (double)map.Height;
            double scale = Math.Min(scaleX, scaleY);

            int bitmapW = Math.Max(1, (int)(map.Width * scale));
            int bitmapH = Math.Max(1, (int)(map.Height * scale));

            var bitmap = new WriteableBitmap(
                bitmapW, bitmapH, 96, 96,
                PixelFormats.Bgr32, null);

            int stride = bitmapW * 4;
            byte[] pixels = new byte[bitmapH * stride];

            for (int py = 0; py < bitmapH; py++)
            {
                for (int px = 0; px < bitmapW; px++)
                {
                    int sx = (int)(px / scale);
                    int sy = (int)(py / scale);

                    Color c = GetSegmentColor(map, sx, sy);

                    int offset = py * stride + px * 4;
                    pixels[offset + 0] = c.B;
                    pixels[offset + 1] = c.G;
                    pixels[offset + 2] = c.R;
                    pixels[offset + 3] = 255;
                }
            }

            bitmap.WritePixels(
                new Int32Rect(0, 0, bitmapW, bitmapH),
                pixels, stride, 0);

            return bitmap;
        }

        private static Color GetSegmentColor(
            GeneratedMap map, int sx, int sy)
        {
            if (!map.IsInBounds(sx, sy))
                return ColorEmpty;

            var seg = map.Grid[sx, sy];

            if (seg.IsPlayerHere)
                return ColorPlayer;

            return seg.Type switch
            {
                SegmentType.Empty => ColorEmpty,
                SegmentType.Border => ColorBorder,
                SegmentType.Restriction => ColorRestriction,
                SegmentType.Connection =>
                    seg.IsJunction ? ColorJunction
                                   : ColorConnection,
                SegmentType.Node =>
                    GetNodeColor(map, seg.NodeId),
                _ => ColorEmpty
            };
        }

        private static Color GetNodeColor(
            GeneratedMap map, string? nodeId)
        {
            if (nodeId == null) return ColorNodeUnknown;
            var node = map.GetNode(nodeId);
            if (node == null) return ColorNodeUnknown;

            return node.State switch
            {
                NodeState.Undiscovered => ColorNodeUnknown,
                NodeState.Discovered => ColorNodeDiscovered,
                NodeState.Visited => ColorNodeVisited,
                NodeState.Explored => ColorNodeExplored,
                NodeState.Looted => ColorNodeLooted,
                _ => ColorNodeUnknown
            };
        }

        // ── Render overlays to canvas ─────────────
        // Node labels, player marker, border entries
        // Drawn on top of bitmap

        public static void RenderOverlays(
            Canvas canvas, GeneratedMap map,
            int displayWidth, int displayHeight,
            double zoom = 1.0,
            double offsetX = 0, double offsetY = 0)
        {
            canvas.Children.Clear();

            double scaleX = displayWidth / (double)map.Width;
            double scaleY = displayHeight / (double)map.Height;
            double baseScale = Math.Min(scaleX, scaleY);
            double finalScale = baseScale * zoom;

            // Draw node labels
            foreach (var node in map.Nodes)
            {
                if (node.State == NodeState.Undiscovered)
                    continue;

                double cx = node.CenterX * finalScale
                    + offsetX;
                double cy = node.CenterY * finalScale
                    + offsetY;

                // Node icon
                var icon = new TextBlock
                {
                    Text = node.Icon,
                    FontFamily = new FontFamily(
                        "Segoe UI Emoji"),
                    FontSize = Math.Max(8,
                        node.SegmentSize * finalScale * 0.6),
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0xc8, 0xc8, 0xb0)),
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(icon,
                    cx - icon.FontSize / 2);
                Canvas.SetTop(icon,
                    cy - icon.FontSize);
                canvas.Children.Add(icon);

                // Node name (only if zoomed in enough)
                if (finalScale > 0.05)
                {
                    var label = new TextBlock
                    {
                        Text = node.Name,
                        FontFamily = new FontFamily(
                            "Courier New"),
                        FontSize = Math.Max(7,
                            Math.Min(11,
                            node.SegmentSize * finalScale
                            * 0.4)),
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(0x7a, 0x9a, 0x7a)),
                        IsHitTestVisible = false
                    };

                    Canvas.SetLeft(label,
                        cx - label.FontSize * 3);
                    Canvas.SetTop(label,
                        cy + label.FontSize * 0.3);
                    canvas.Children.Add(label);
                }
            }

            // Draw player marker
            if (map.PlayerNodeId != null)
            {
                var playerNode = map.GetNode(map.PlayerNodeId);
                if (playerNode != null)
                {
                    double px = playerNode.CenterX
                        * finalScale + offsetX;
                    double py = playerNode.CenterY
                        * finalScale + offsetY;

                    double markerSize = Math.Max(6,
                        playerNode.SegmentSize
                        * finalScale * 0.3);

                    var marker = new Ellipse
                    {
                        Width = markerSize,
                        Height = markerSize,
                        Fill = new SolidColorBrush(
                            ColorPlayer),
                        Stroke = new SolidColorBrush(
                            Color.FromRgb(
                                0x1a, 0x2a, 0x1a)),
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };

                    Canvas.SetLeft(marker,
                        px - markerSize / 2);
                    Canvas.SetTop(marker,
                        py - markerSize / 2);
                    canvas.Children.Add(marker);
                }
            }

            // Draw border entry arrows
            foreach (var entry in map.BorderEntries)
            {
                double ex = entry.GridX * finalScale
                    + offsetX;
                double ey = entry.GridY * finalScale
                    + offsetY;

                double as2 = Math.Max(4, finalScale * 3);

                var arrowColor = entry.IsPlayerHere
                    ? ColorPlayer
                    : Color.FromRgb(0x4a, 0x7a, 0x4a);

                PointCollection arrowPts =
                    entry.ArrivalDirection switch
                    {
                        Direction.South => new PointCollection
                        {
                            new(ex - as2, ey),
                            new(ex + as2, ey),
                            new(ex, ey - as2 * 1.5)
                        },
                        Direction.North => new PointCollection
                        {
                            new(ex - as2, ey),
                            new(ex + as2, ey),
                            new(ex, ey + as2 * 1.5)
                        },
                        Direction.West => new PointCollection
                        {
                            new(ex, ey - as2),
                            new(ex, ey + as2),
                            new(ex + as2 * 1.5, ey)
                        },
                        _ => new PointCollection
                        {
                            new(ex, ey - as2),
                            new(ex, ey + as2),
                            new(ex - as2 * 1.5, ey)
                        }
                    };

                canvas.Children.Add(new Polygon
                {
                    Points = arrowPts,
                    Fill = new SolidColorBrush(arrowColor),
                    IsHitTestVisible = false
                });
            }

            // Draw compass
            DrawCompass(canvas, displayWidth, displayHeight);
        }

        private static void DrawCompass(Canvas canvas,
            int w, int h)
        {
            var dirs = new[]
            {
                ("N", w / 2.0,  8.0),
                ("S", w / 2.0,  h - 8.0),
                ("W", 8.0,       h / 2.0),
                ("E", w - 8.0,   h / 2.0)
            };

            foreach (var (label, x, y) in dirs)
            {
                var tb = new TextBlock
                {
                    Text = label,
                    FontFamily = new FontFamily(
                        "Courier New"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0x4a, 0x6a, 0x4a)),
                    FontWeight = FontWeights.SemiBold,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(tb, x - 5);
                Canvas.SetTop(tb, y - 7);
                canvas.Children.Add(tb);
            }
        }

        // ── Render travel animation dot ───────────

        public static void RenderTravelDot(
            Canvas canvas, GeneratedMap map,
            GameMapConnection conn,
            int displayWidth, int displayHeight,
            double zoom = 1.0,
            double offsetX = 0, double offsetY = 0)
        {
            if (!conn.IsPlayerTravelling) return;
            if (conn.Path.Count == 0) return;

            double scaleX = displayWidth / (double)map.Width;
            double scaleY = displayHeight / (double)map.Height;
            double baseScale = Math.Min(scaleX, scaleY);
            double finalScale = baseScale * zoom;

            int segIdx = (int)(conn.TravelProgress
                * (conn.Path.Count - 1));
            segIdx = Math.Max(0, Math.Min(
                conn.Path.Count - 1, segIdx));

            var (sx, sy) = conn.Path[segIdx];
            double px = sx * finalScale + offsetX;
            double py = sy * finalScale + offsetY;

            double dotSize = Math.Max(5, finalScale * 3);

            var dot = new Ellipse
            {
                Width = dotSize,
                Height = dotSize,
                Fill = new SolidColorBrush(ColorPlayer),
                Stroke = new SolidColorBrush(
                    Color.FromRgb(0x1a, 0x2a, 0x1a)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(dot, px - dotSize / 2);
            Canvas.SetTop(dot, py - dotSize / 2);
            canvas.Children.Add(dot);
        }
    }
}