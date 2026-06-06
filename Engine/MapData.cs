using System.Collections.Generic;

namespace LostInAForgottenCity.Engine
{
    // ── Segment types ─────────────────────────────
    public enum SegmentType : byte
    {
        Empty = 0,
        Border = 1,
        Node = 2,
        Connection = 3,
        Restriction = 4
    }

    // ── Map types ─────────────────────────────────
    public enum GameMapType
    {
        Region,    // visible, shows locations
        Location,  // visible, shows landmarks
        Landmark   // invisible, game data only
    }

    // ── Map sizes ─────────────────────────────────
    public enum MapSize
    {
        Small,
        Medium,
        Large
    }

    // ── Node sizes in segments ────────────────────
    // Small  = 3×3
    // Medium = 5×5
    // Large  = 10×10

    // ── Direction ────────────────────────────────
    public enum Direction
    {
        None,
        North, NorthEast, East, SouthEast,
        South, SouthWest, West, NorthWest
    }

    // ── Node state ────────────────────────────────
    public enum NodeState
    {
        Undiscovered,
        Discovered,
        Visited,
        Explored,
        Looted
    }

    // ── Risk level ────────────────────────────────
    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    // ── Segment ───────────────────────────────────
    public class MapSegment
    {
        public SegmentType Type { get; set; } = SegmentType.Empty;
        public string? NodeId { get; set; } = null;
        public string? ConnectionId { get; set; } = null;
        public Direction ConnectionDirection { get; set; }
            = Direction.None;
        public bool IsPlayerHere { get; set; } = false;
        public bool IsJunction { get; set; } = false;
    }

    // ── Game map node ─────────────────────────────
    public class GameMapNode
    {
        // Identity
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "?";
        public GameMapType MapType { get; set; }

        // Size — determines segment footprint
        public MapSize Size { get; set; } = MapSize.Medium;
        public int SegmentSize => Size switch
        {
            MapSize.Small => 3,
            MapSize.Medium => 5,
            MapSize.Large => 10,
            _ => 5
        };

        // Position on parent map (top-left corner)
        public int GridX { get; set; }
        public int GridY { get; set; }

        // State
        public NodeState State { get; set; }
            = NodeState.Undiscovered;
        public RiskLevel Risk { get; set; }
            = RiskLevel.Low;
        public bool HasSafeRoom { get; set; } = false;
        public bool PlayerVisitedBefore { get; set; } = false;

        // Child map (Location's landmark map,
        //            Region's location map)
        public GeneratedMap? ChildMap { get; set; } = null;

        // Convenience: segment bounds
        public int Left => GridX;
        public int Top => GridY;
        public int Right => GridX + SegmentSize - 1;
        public int Bottom => GridY + SegmentSize - 1;
        public int CenterX => GridX + SegmentSize / 2;
        public int CenterY => GridY + SegmentSize / 2;
    }

    // ── Game map connection ───────────────────────
    public class GameMapConnection
    {
        public string Id { get; set; } = "";
        public string FromNodeId { get; set; } = "";
        public string ToNodeId { get; set; } = "";

        // Segment count determines distance
        public int SegmentCount { get; set; } = 0;
        public Controls.TravelDistance Distance =>SegmentCount switch
        {
            <= 5 => Controls.TravelDistance.Immediate,
            <= 12 => Controls.TravelDistance.Close,
            <= 20 => Controls.TravelDistance.Near,
            <= 30 => Controls.TravelDistance.Far,
            _ => Controls.TravelDistance.Distant
        };

        // Path of segments
        public List<(int x, int y)> Path { get; set; } = new();

        // State
        public bool IsDiscovered { get; set; } = false;
        public bool IsPlayerTravelling { get; set; } = false;
        public double TravelProgress { get; set; } = 0.0;
    }

    // ── Border entry point ────────────────────────
    public class BorderEntryPoint
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public Direction ArrivalDirection { get; set; }
        public string? ConnectedNodeId { get; set; }
        public bool IsPlayerHere { get; set; } = false;

        // Which face of the border
        public Direction BorderFace =>
            ArrivalDirection switch
            {
                Direction.South or
                Direction.SouthEast or
                Direction.SouthWest => Direction.South,
                Direction.North or
                Direction.NorthEast or
                Direction.NorthWest => Direction.North,
                Direction.East => Direction.East,
                Direction.West => Direction.West,
                _ => Direction.South
            };
    }

    // ── Generated map ─────────────────────────────
    public class GeneratedMap
    {
        // Identity
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public GameMapType Type { get; set; }
        public MapSize Size { get; set; } = MapSize.Medium;

        // Grid dimensions in segments
        public int Width => Type switch
        {
            GameMapType.Region when Id == "unknown" => 3000,
            GameMapType.Region => 5000,
            GameMapType.Location => Size switch
            {
                MapSize.Small => 500,
                MapSize.Medium => 1000,
                MapSize.Large => 2000,
                _ => 1000
            },
            GameMapType.Landmark => Size switch
            {
                MapSize.Small => 50,
                MapSize.Medium => 100,
                MapSize.Large => 300,
                _ => 100
            },
            _ => 1000
        };
        public int Height => Width; // always square

        // Segment grid
        public MapSegment[,] Grid { get; private set; }
            = new MapSegment[0, 0];

        // Nodes and connections
        public List<GameMapNode> Nodes { get; set; } = new();
        public List<GameMapConnection> Connections { get; set; }
            = new();

        // Border entries
        public List<BorderEntryPoint> BorderEntries { get; set; }
            = new();

        // Player state
        public bool IsPlayerPresent { get; set; } = false;
        public bool WasPlayerPresent { get; set; } = false;
        public string? PlayerNodeId { get; set; } = null;
        public (int x, int y) PlayerPosition { get; set; }

        // Generation seed (reproducible layouts)
        public int Seed { get; set; }

        // Risk (changes daily)
        public RiskLevel GlobalRisk { get; set; } = RiskLevel.Low;
        public int LastRiskUpdateDay { get; set; } = 0;

        // Initialize grid
        public void InitializeGrid()
        {
            Grid = new MapSegment[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Grid[x, y] = new MapSegment();
        }

        // Helper: is position inside map
        public bool IsInBounds(int x, int y)
            => x >= 0 && x < Width &&
               y >= 0 && y < Height;

        // Helper: is position free for node placement
        public bool IsFreeForNode(int x, int y, int size)
        {
            // Check node footprint + 3 segment minimum gap
            int minX = x - 3;
            int minY = y - 3;
            int maxX = x + size + 3;
            int maxY = y + size + 3;

            for (int cx = minX; cx < maxX; cx++)
                for (int cy = minY; cy < maxY; cy++)
                {
                    if (!IsInBounds(cx, cy)) continue;
                    var seg = Grid[cx, cy];
                    if (seg.Type == SegmentType.Node ||
                        seg.Type == SegmentType.Border ||
                        seg.Type == SegmentType.Restriction)
                        return false;
                }
            return true;
        }

        // Helper: get segment at position
        public MapSegment GetSegment(int x, int y)
            => IsInBounds(x, y)
                ? Grid[x, y]
                : new MapSegment();

        // Helper: set segment type
        public void SetSegment(int x, int y,
            SegmentType type, string? nodeId = null,
            string? connectionId = null,
            Direction dir = Direction.None)
        {
            if (!IsInBounds(x, y)) return;
            Grid[x, y].Type = type;
            if (nodeId != null)
                Grid[x, y].NodeId = nodeId;
            if (connectionId != null)
                Grid[x, y].ConnectionId = connectionId;
            Grid[x, y].ConnectionDirection = dir;
        }

        // Helper: get node by id
        public GameMapNode? GetNode(string id)
            => Nodes.Find(n => n.Id == id);

        // Helper: get connections for a node
        public List<GameMapConnection> GetNodeConnections(
            string nodeId)
            => Connections.FindAll(c =>
                c.FromNodeId == nodeId ||
                c.ToNodeId == nodeId);

        // Helper: get distance category between two nodes
        public Controls.TravelDistance? GetDistance(
            string fromId, string toId)
        {
            var conn = Connections.Find(c =>
                (c.FromNodeId == fromId &&
                 c.ToNodeId == toId) ||
                (c.FromNodeId == toId &&
                 c.ToNodeId == fromId));
            return conn?.Distance;
        }
    }
}