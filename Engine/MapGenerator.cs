using System;
using System.Collections.Generic;
using System.Linq;

namespace LostInAForgottenCity.Engine
{
    public static class MapGenerator
    {
        // ── Direction vectors ─────────────────────
        private static readonly (int dx, int dy)[] DirectionVectors =
        {
            (0, -1),  // North
            (1, -1),  // NorthEast
            (1,  0),  // East
            (1,  1),  // SouthEast
            (0,  1),  // South
            (-1, 1),  // SouthWest
            (-1, 0),  // West
            (-1,-1),  // NorthWest
        };

        private static int DirIndex(Direction d) => d switch
        {
            Direction.North => 0,
            Direction.NorthEast => 1,
            Direction.East => 2,
            Direction.SouthEast => 3,
            Direction.South => 4,
            Direction.SouthWest => 5,
            Direction.West => 6,
            Direction.NorthWest => 7,
            _ => 0
        };

        private static Direction IndexDir(int i) => i switch
        {
            0 => Direction.North,
            1 => Direction.NorthEast,
            2 => Direction.East,
            3 => Direction.SouthEast,
            4 => Direction.South,
            5 => Direction.SouthWest,
            6 => Direction.West,
            7 => Direction.NorthWest,
            _ => Direction.North
        };

        // Angle between two directions (in direction steps)
        private static int DirectionDiff(Direction a, Direction b)
        {
            int diff = Math.Abs(DirIndex(a) - DirIndex(b));
            return Math.Min(diff, 8 - diff);
        }

        // Is turn sharp? (more than 2 direction steps = sharp)
        private static bool IsSharpTurn(Direction from,
            Direction to)
            => DirectionDiff(from, to) > 2;

        // ── Main generation entry point ───────────

        public static GeneratedMap Generate(
            string id, string name,
            GameMapType type, MapSize size,
            List<(string id, string name,
                  string icon, MapSize nodeSize)> nodeInfos,
            int seed = -1)
        {
            var random = seed >= 0
                ? new Random(seed) : new Random();
            int actualSeed = seed >= 0 ? seed : random.Next();
            random = new Random(actualSeed);

            var map = new GeneratedMap
            {
                Id = id,
                Name = name,
                Type = type,
                Size = size,
                Seed = actualSeed
            };

            map.InitializeGrid();

            // Step 1: Place border
            PlaceBorder(map);

            // Step 2: Place restrictions
            // (skip for landmark maps)
            if (type != GameMapType.Landmark)
                PlaceRestrictions(map, random);

            // Step 3: Place nodes
            bool placed = PlaceNodes(map, nodeInfos, random);
            if (!placed)
            {
                // Retry with new seed if placement failed
                return Generate(id, name, type, size,
                    nodeInfos, random.Next());
            }

            // Step 4: Route connections between nodes
            RouteAllConnections(map, random);

            // Step 6: Ensure all nodes accessible
            bool accessible = EnsureConnectivity(
                map, nodeInfos, random);
            if (!accessible)
            {
                return Generate(id, name, type, size,
                    nodeInfos, random.Next());
            }

            return map;
        }

        // ── Step 1: Place border ──────────────────

        private static void PlaceBorder(GeneratedMap map)
        {
            int w = map.Width;
            int h = map.Height;

            for (int x = 0; x < w; x++)
            {
                map.SetSegment(x, 0, SegmentType.Border);
                map.SetSegment(x, h - 1, SegmentType.Border);
            }
            for (int y = 0; y < h; y++)
            {
                map.SetSegment(0, y, SegmentType.Border);
                map.SetSegment(w - 1, y, SegmentType.Border);
            }
        }

        // ── Step 2: Place restrictions ────────────

        private static void PlaceRestrictions(
            GeneratedMap map, Random random)
        {
            if (map.Type == GameMapType.Region)
                PlaceNaturalRestrictions(map, random);
            else
                PlaceWallRestrictions(map, random);
        }

        private static void PlaceNaturalRestrictions(
            GeneratedMap map, Random random)
        {
            // Organic clusters — rock formations,
            // tree clusters, ravines
            int clusterCount = map.Width / 500;

            for (int c = 0; c < clusterCount; c++)
            {
                int cx = random.Next(
                    map.Width / 10, map.Width * 9 / 10);
                int cy = random.Next(
                    map.Height / 10, map.Height * 9 / 10);

                // Random organic shape via random walk
                int size = random.Next(5, 20);
                int x = cx, y = cy;

                for (int s = 0; s < size; s++)
                {
                    if (map.IsInBounds(x, y) &&
                        map.GetSegment(x, y).Type ==
                        SegmentType.Empty)
                    {
                        map.SetSegment(x, y,
                            SegmentType.Restriction);
                    }

                    // Random walk — organic shape
                    int dir = random.Next(8);
                    x += DirectionVectors[dir].dx;
                    y += DirectionVectors[dir].dy;

                    // Keep within bounds
                    x = Math.Max(3, Math.Min(map.Width - 4, x));
                    y = Math.Max(3, Math.Min(map.Height - 4, y));
                }
            }
        }

        private static void PlaceWallRestrictions(
            GeneratedMap map, Random random)
        {
            // Wall-like linear shapes
            int wallCount = map.Width / 100;

            for (int w = 0; w < wallCount; w++)
            {
                int sx = random.Next(
                    map.Width / 10, map.Width * 9 / 10);
                int sy = random.Next(
                    map.Height / 10, map.Height * 9 / 10);

                // Pick a direction for wall
                int dirIdx = random.Next(4) * 2; // cardinal only
                var (dx, dy) = DirectionVectors[dirIdx];

                int length = random.Next(5, 30);

                int x = sx, y = sy;
                for (int l = 0; l < length; l++)
                {
                    if (map.IsInBounds(x, y) &&
                        map.GetSegment(x, y).Type ==
                        SegmentType.Empty)
                    {
                        map.SetSegment(x, y,
                            SegmentType.Restriction);
                    }

                    x += dx;
                    y += dy;

                    if (!map.IsInBounds(x, y)) break;

                    // Small chance to turn
                    if (random.NextDouble() < 0.1)
                    {
                        dirIdx = (dirIdx + 2) % 8;
                        (dx, dy) = DirectionVectors[dirIdx];
                    }
                }
            }
        }

        // ── Step 3: Place nodes ───────────────────

        private static bool PlaceNodes(GeneratedMap map,
            List<(string id, string name,
                  string icon, MapSize nodeSize)> nodeInfos,
            Random random)
        {
            int maxAttempts = 1000;

            foreach (var info in nodeInfos)
            {
                int segSize = info.nodeSize switch
                {
                    MapSize.Small => 3,
                    MapSize.Medium => 5,
                    MapSize.Large => 10,
                    _ => 5
                };

                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < maxAttempts)
                {
                    attempts++;

                    // Random position within map bounds
                    // (keep away from border + min gap)
                    int margin = 4; // border(1) + gap(3)
                    int x = random.Next(margin,
                        map.Width - segSize - margin);
                    int y = random.Next(margin,
                        map.Height - segSize - margin);

                    if (!map.IsFreeForNode(x, y, segSize))
                        continue;

                    // Place node segments
                    var node = new GameMapNode
                    {
                        Id = info.id,
                        Name = info.name,
                        Icon = info.icon,
                        Size = info.nodeSize,
                        GridX = x,
                        GridY = y,
                        MapType = map.Type
                    };

                    for (int nx = x; nx < x + segSize; nx++)
                        for (int ny = y; ny < y + segSize; ny++)
                            map.SetSegment(nx, ny,
                                SegmentType.Node, info.id);

                    map.Nodes.Add(node);
                    placed = true;
                }

                if (!placed) return false;
            }

            return true;
        }

        // ── Step 4: Route connections ─────────────

        private static void RouteAllConnections(
            GeneratedMap map, Random random)
        {
            // Every node tries to connect to every other node
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                for (int j = i + 1; j < map.Nodes.Count; j++)
                {
                    var from = map.Nodes[i];
                    var to = map.Nodes[j];

                    var path = FindPath(map, from, to);
                    if (path == null) continue;

                    var connId = $"conn_{from.Id}_{to.Id}";
                    var conn = new GameMapConnection
                    {
                        Id = connId,
                        FromNodeId = from.Id,
                        ToNodeId = to.Id,
                        Path = path,
                        SegmentCount = path.Count
                    };

                    // Mark connection segments on grid
                    foreach (var (px, py) in path)
                    {
                        var seg = map.GetSegment(px, py);
                        if (seg.Type == SegmentType.Connection)
                        {
                            // Junction point
                            map.SetJunctionSegment(px, py, true);
                        }
                        else if (seg.Type == SegmentType.Empty)
                        {
                            map.SetSegment(px, py,
                                SegmentType.Connection,
                                connectionId: connId);
                        }
                    }

                    map.Connections.Add(conn);
                }
            }
        }

        // ── A* pathfinding with turn limit ────────

        private class PathNode
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Direction Dir { get; set; }
            public int Turns { get; set; }
            public int G { get; set; } // cost so far
            public int H { get; set; } // heuristic
            public int F => G + H;
            public PathNode? Parent { get; set; }
        }

        private static List<(int, int)>? FindPath(GeneratedMap map, GameMapNode from, GameMapNode to)
        {
            var goals = GetNodeEdgeSet(to);
            var starts = GetNodeEdgePoints(map, from);

            List<(int, int)>? bestPath = null;
            foreach (var start in starts)
            {
                var path = AStarFind(map, start,
                    to.CenterX, to.CenterY, goals);

                if (path != null &&
                    (bestPath == null ||
                     path.Count < bestPath.Count))
                    bestPath = path;
            }
            return bestPath;
        }

        private static List<(int x, int y)> GetNodeEdgePoints(
            GeneratedMap map, GameMapNode node)
        {
            var points = new List<(int, int)>();
            int size = node.SegmentSize;

            // All 4 edges of the node
            for (int x = node.Left; x <= node.Right; x++)
            {
                // Top edge
                int ty = node.Top - 1;
                if (map.IsInBounds(x, ty) &&
                    map.GetSegment(x, ty).Type
                    == SegmentType.Empty)
                    points.Add((x, ty));

                // Bottom edge
                int by = node.Bottom + 1;
                if (map.IsInBounds(x, by) &&
                    map.GetSegment(x, by).Type
                    == SegmentType.Empty)
                    points.Add((x, by));
            }

            for (int y = node.Top; y <= node.Bottom; y++)
            {
                // Left edge
                int lx = node.Left - 1;
                if (map.IsInBounds(lx, y) &&
                    map.GetSegment(lx, y).Type
                    == SegmentType.Empty)
                    points.Add((lx, y));

                // Right edge
                int rx = node.Right + 1;
                if (map.IsInBounds(rx, y) &&
                    map.GetSegment(rx, y).Type
                    == SegmentType.Empty)
                    points.Add((rx, y));
            }

            return points;
        }

        private static HashSet<(int, int)> GetNodeEdgeSet(
            GameMapNode node)
        {
            var set = new HashSet<(int, int)>();
            for (int x = node.Left - 1;
                     x <= node.Right + 1; x++)
            {
                set.Add((x, node.Top - 1));
                set.Add((x, node.Bottom + 1));
            }
            for (int y = node.Top - 1;
                     y <= node.Bottom + 1; y++)
            {
                set.Add((node.Left - 1, y));
                set.Add((node.Right + 1, y));
            }
            return set;
        }

        private static List<(int, int)>? AStarFind(
            GeneratedMap map,
            (int x, int y) start,
            int targetX, int targetY,
            HashSet<(int, int)> goals)
        {
            const int MaxTurns = 4;
            const int MaxNodes = 50000;

            // State: (x, y, directionIndex, turns)
            var gScores = new Dictionary<(int, int, int, int), int>();
            var open = new PriorityQueue<PathNode, int>();
            var closed = new HashSet<(int, int, int, int)>();
            int nodeCount = 0;

            // Initialize from start in all 8 directions
            for (int d = 0; d < 8; d++)
            {
                var (dx, dy) = DirectionVectors[d];
                int nx = start.x + dx;
                int ny = start.y + dy;

                if (!CanTraverse(map, nx, ny, goals)) continue;

                int h = Heuristic(nx, ny, targetX, targetY);
                var pn = new PathNode
                {
                    X = nx,
                    Y = ny,
                    Dir = IndexDir(d),
                    Turns = 0,
                    G = 1,
                    H = h,
                    Parent = null
                };

                var key = (nx, ny, d, 0);
                gScores[key] = 1;
                open.Enqueue(pn, pn.F);
                nodeCount++;
            }

            while (open.Count > 0 && nodeCount < MaxNodes)
            {
                var current = open.Dequeue();

                // Reached goal
                if (goals.Contains((current.X, current.Y)))
                    return ReconstructPath(current);

                var stateKey = (current.X, current.Y,
                    DirIndex(current.Dir), current.Turns);

                if (closed.Contains(stateKey)) continue;
                closed.Add(stateKey);

                // Expand neighbors
                for (int d = 0; d < 8; d++)
                {
                    var newDir = IndexDir(d);
                    var (dx, dy) = DirectionVectors[d];
                    int nx = current.X + dx;
                    int ny = current.Y + dy;

                    if (!CanTraverse(map, nx, ny, goals)) continue;

                    int newTurns = current.Turns;
                    if (current.Dir != Direction.None &&
                        IsSharpTurn(current.Dir, newDir))
                    {
                        newTurns++;
                        if (newTurns > MaxTurns) continue;
                    }

                    int newG = current.G + 1;
                    var newKey = (nx, ny, d, newTurns);

                    if (closed.Contains(newKey)) continue;

                    if (gScores.TryGetValue(newKey, out int existingG)
                        && existingG <= newG)
                        continue;

                    gScores[newKey] = newG;
                    int newH = Heuristic(nx, ny, targetX, targetY);

                    var newNode = new PathNode
                    {
                        X = nx,
                        Y = ny,
                        Dir = newDir,
                        Turns = newTurns,
                        G = newG,
                        H = newH,
                        Parent = current
                    };

                    open.Enqueue(newNode, newNode.F);
                    nodeCount++;
                }
            }

            return null;
        }

        private static bool CanTraverse(GeneratedMap map,
            int x, int y,
            HashSet<(int, int)> goals)
        {
            if (!map.IsInBounds(x, y)) return false;
            var type = map.GetSegment(x, y).Type;
            return type == SegmentType.Empty ||
                   type == SegmentType.Connection ||
                   type == SegmentType.Border ||
                   goals.Contains((x, y));
        }

        private static int Heuristic(int x, int y,
            int tx, int ty)
        {
            int dx = Math.Abs(x - tx);
            int dy = Math.Abs(y - ty);
            return Math.Max(dx, dy); // Chebyshev distance
        }

        private static List<(int, int)> ReconstructPath(
            PathNode node)
        {
            var path = new List<(int, int)>();
            var current = node;
            while (current != null)
            {
                path.Add((current.X, current.Y));
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        // ── Step 6: Ensure connectivity ───────────

        private static bool EnsureConnectivity(
            GeneratedMap map,
            List<(string id, string name,
                  string icon, MapSize nodeSize)> nodeInfos,
            Random random)
        {
            // Every node must have at least 1 connection
            foreach (var node in map.Nodes)
            {
                var conns = map.GetNodeConnections(node.Id);
                if (conns.Count == 0)
                    return false; // trigger regeneration
            }
            return true;
        }

        // ── Public helpers ────────────────────────

        // Get travel distance between two nodes
        public static TravelDistance GetDistance(
            GeneratedMap map,
            string fromId, string toId)
        {
            var conn = map.Connections.FirstOrDefault(c =>
                (c.FromNodeId == fromId &&
                 c.ToNodeId == toId) ||
                (c.FromNodeId == toId &&
                 c.ToNodeId == fromId));

            return conn?.Distance
                ?? TravelDistance.Distant;
        }

        // Get available destinations from current node
        public static List<(string nodeId,
            string nodeName, TravelDistance distance)>
            GetAvailableDestinations(
                GeneratedMap map, string currentNodeId)
        {
            var result = new List<(string, string,
                TravelDistance)>();

            foreach (var conn in
                map.GetNodeConnections(currentNodeId))
            {
                string otherId =
                    conn.FromNodeId == currentNodeId
                    ? conn.ToNodeId
                    : conn.FromNodeId;

                var otherNode = map.GetNode(otherId);
                if (otherNode == null) continue;

                result.Add((
                    otherId,
                    otherNode.Name,
                    conn.Distance));
            }

            return result;
        }

        // Update player position
        public static void SetPlayerPosition(
            GeneratedMap map, string nodeId)
        {
            // Clear old position
            if (map.PlayerNodeId != null)
            {
                var oldNode = map.GetNode(map.PlayerNodeId);
                if (oldNode != null)
                {
                    // Clear player marker from old node
                    for (int x = oldNode.Left;
                             x <= oldNode.Right; x++)
                        for (int y = oldNode.Top;
                                 y <= oldNode.Bottom; y++)
                            map.SetPlayerSegment(x, y, false);
                }
            }

            // Set new position
            map.PlayerNodeId = nodeId;
            map.IsPlayerPresent = true;
            map.WasPlayerPresent = true;

            var node = map.GetNode(nodeId);
            if (node == null) return;

            node.State = node.State == NodeState.Undiscovered
                ? NodeState.Visited
                : node.State;

            // Mark center segment as player position
            map.SetPlayerSegment(node.CenterX, node.CenterY, true);
            map.PlayerPosition =
                (node.CenterX, node.CenterY);
        }

        // Discover nodes visible from current position
        public static void DiscoverVisibleNodes(
            GeneratedMap map, string currentNodeId)
        {
            // All directly connected nodes become Discovered
            var connections =
                map.GetNodeConnections(currentNodeId);

            foreach (var conn in connections)
            {
                string otherId =
                    conn.FromNodeId == currentNodeId
                    ? conn.ToNodeId : conn.FromNodeId;

                var other = map.GetNode(otherId);
                if (other == null) continue;

                if (other.State == NodeState.Undiscovered)
                    other.State = NodeState.Discovered;

                conn.IsDiscovered = true;
            }
        }

        // Randomize daily risk
        public static void UpdateDailyRisk(
            GeneratedMap map, int currentDay,
            Random random)
        {
            if (map.LastRiskUpdateDay == currentDay) return;

            map.GlobalRisk = (RiskLevel)random.Next(3);
            foreach (var node in map.Nodes)
                node.Risk = (RiskLevel)random.Next(3);

            map.LastRiskUpdateDay = currentDay;
        }

        // ── Border landmarks ──────────────────────────

        public static void AddBorderLandmarks(
            GeneratedMap locationMap,
            GeneratedMap regionMap,
            string locationRegionNodeId,
            Random random)
        {
            var regionNode = regionMap.GetNode(
                locationRegionNodeId);
            if (regionNode == null) return;

            var connections = regionMap.GetNodeConnections(
                locationRegionNodeId);

            foreach (var conn in connections)
            {
                string neighborId =
                    conn.FromNodeId == locationRegionNodeId
                    ? conn.ToNodeId : conn.FromNodeId;

                var neighborNode = regionMap.GetNode(neighborId);
                if (neighborNode == null) continue;

                // Use actual region node positions
                // to determine which side to place on
                Direction dir = CalculateDirection(
                    regionNode.CenterX, regionNode.CenterY,
                    neighborNode.CenterX, neighborNode.CenterY);

                string borderId =
                    $"border_{locationRegionNodeId}_to_{neighborId}";
                string borderName =
                    $"Road towards {neighborNode.Name}";

                var borderNode = new GameMapNode
                {
                    Id = borderId,
                    Name = borderName,
                    Icon = "🛤",
                    Size = MapSize.Small,
                    IsBorderLandmark = true,
                    BorderTargetRegionNodeId = neighborId,
                    MapType = GameMapType.Location,
                    State = NodeState.Discovered
                };

                bool placed = PlaceBorderLandmarkNode(
                    locationMap, borderNode, dir, random);
                if (!placed) continue;

                for (int nx = borderNode.Left;
                         nx <= borderNode.Right; nx++)
                    for (int ny = borderNode.Top;
                             ny <= borderNode.Bottom; ny++)
                        locationMap.SetSegment(nx, ny,
                            SegmentType.Node, borderId);

                locationMap.Nodes.Add(borderNode);

                var nearest = FindNearestNormalLandmark(
                    locationMap, borderNode);
                if (nearest == null) continue;

                var path = FindPath(locationMap, borderNode, nearest);
                if (path == null) continue;

                var connId = $"conn_{borderId}_{nearest.Id}";
                var newConn = new GameMapConnection
                {
                    Id = connId,
                    FromNodeId = borderId,
                    ToNodeId = nearest.Id,
                    Path = path,
                    SegmentCount = path.Count
                };

                foreach (var (px, py) in path)
                {
                    var seg = locationMap.GetSegment(px, py);
                    if (seg.Type == SegmentType.Connection)
                        locationMap.SetJunctionSegment(px, py, true);
                    else if (seg.Type == SegmentType.Empty)
                        locationMap.SetSegment(px, py,
                            SegmentType.Connection,
                            connectionId: connId);
                }

                locationMap.Connections.Add(newConn);
            }
        }

        public static void AddSpecialBorderLandmark(
            GeneratedMap locationMap,
            string id,
            string name,
            Direction direction,
            string connectToNodeId,
            Random random)
        {
            var borderNode = new GameMapNode
            {
                Id = id,
                Name = name,
                Icon = "🛤",
                Size = MapSize.Small,
                IsBorderLandmark = true,
                BorderTargetRegionNodeId = null, // no exit
                MapType = GameMapType.Location,
                State = NodeState.Discovered
            };

            bool placed = PlaceBorderLandmarkNode(
                locationMap, borderNode, direction, random);
            if (!placed) return;

            for (int nx = borderNode.Left;
                     nx <= borderNode.Right; nx++)
                for (int ny = borderNode.Top;
                         ny <= borderNode.Bottom; ny++)
                    locationMap.SetSegment(nx, ny,
                        SegmentType.Node, id);

            locationMap.Nodes.Add(borderNode);

            // Connect to specified node
            var target = locationMap.GetNode(connectToNodeId);
            if (target == null) return;

            var path = FindPath(locationMap, borderNode, target);
            if (path == null) return;

            var connId = $"conn_{id}_{connectToNodeId}";
            var conn = new GameMapConnection
            {
                Id = connId,
                FromNodeId = id,
                ToNodeId = connectToNodeId,
                Path = path,
                SegmentCount = path.Count
            };

            foreach (var (px, py) in path)
            {
                var seg = locationMap.GetSegment(px, py);
                if (seg.Type == SegmentType.Connection)
                    locationMap.SetJunctionSegment(px, py, true);
                else if (seg.Type == SegmentType.Empty)
                    locationMap.SetSegment(px, py,
                        SegmentType.Connection,
                        connectionId: connId);
            }

            locationMap.Connections.Add(conn);
        }

        // Restore this — uses actual pixel positions
        private static Direction CalculateDirection(
            int fromX, int fromY, int toX, int toY)
        {
            int dx = toX - fromX;
            int dy = toY - fromY;

            if (Math.Abs(dx) < Math.Abs(dy) * 0.5)
                return dy < 0 ? Direction.North : Direction.South;
            if (Math.Abs(dy) < Math.Abs(dx) * 0.5)
                return dx < 0 ? Direction.West : Direction.East;
            if (dx > 0 && dy < 0) return Direction.NorthEast;
            if (dx > 0 && dy > 0) return Direction.SouthEast;
            if (dx < 0 && dy < 0) return Direction.NorthWest;
            return Direction.SouthWest;
        }

        private static bool PlaceBorderLandmarkNode(
            GeneratedMap map, GameMapNode node,
            Direction direction, Random random)
        {
            int segSize = node.SegmentSize;
            int margin = 4;
            int w = map.Width;
            int h = map.Height;
            int zone = w / 5; // 20% of map width

            for (int attempt = 0; attempt < 1000; attempt++)
            {
                int x, y;

                switch (direction)
                {
                    case Direction.North:
                    case Direction.NorthEast:
                    case Direction.NorthWest:
                        x = random.Next(margin, w - segSize - margin);
                        y = random.Next(margin, zone);
                        break;
                    case Direction.South:
                    case Direction.SouthEast:
                    case Direction.SouthWest:
                        x = random.Next(margin, w - segSize - margin);
                        y = random.Next(h - zone, h - segSize - margin);
                        break;
                    case Direction.East:
                        x = random.Next(w - zone, w - segSize - margin);
                        y = random.Next(margin, h - segSize - margin);
                        break;
                    case Direction.West:
                        x = random.Next(margin, zone);
                        y = random.Next(margin, h - segSize - margin);
                        break;
                    default:
                        x = random.Next(margin, w - segSize - margin);
                        y = random.Next(h - zone, h - segSize - margin);
                        break;
                }

                if (!map.IsFreeForNode(x, y, segSize)) continue;

                node.GridX = x;
                node.GridY = y;
                return true;
            }
            return false;
        }

        private static GameMapNode? FindNearestNormalLandmark(
            GeneratedMap map, GameMapNode borderLandmark)
        {
            GameMapNode? nearest = null;
            double minDist = double.MaxValue;

            foreach (var node in map.Nodes)
            {
                if (node.IsBorderLandmark) continue;

                double dx = node.CenterX - borderLandmark.CenterX;
                double dy = node.CenterY - borderLandmark.CenterY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = node;
                }
            }

            return nearest;
        }
    }
}