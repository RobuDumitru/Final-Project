using System.Collections.Generic;

namespace LostInAForgottenCity.Engine
{
    public class MapSegment
    {
        public SegmentType Type { get; set; } = SegmentType.Empty;
        public string? NodeId { get; set; } = null;
        public string? ConnectionId { get; set; } = null;
        public Direction ConnectionDirection { get; set; } = Direction.None;
        public bool IsPlayerHere { get; set; } = false;
        public bool IsJunction { get; set; } = false;
    }

    public class GameMapNode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "?";
        public GameMapType MapType { get; set; }
        public MapSize Size { get; set; } = MapSize.Medium;
        public int SegmentSize => Size switch
        {
            MapSize.Small  => 3,
            MapSize.Medium => 5,
            MapSize.Large  => 10,
            _ => 5
        };
        public int GridX { get; set; }
        public int GridY { get; set; }
        public NodeState State { get; set; } = NodeState.Undiscovered;
        public RiskLevel Risk { get; set; } = RiskLevel.Low;
        public bool HasSafeRoom { get; set; } = false;
        public bool PlayerVisitedBefore { get; set; } = false;
        public GeneratedMap? ChildMap { get; set; } = null;

        public int Left   => GridX;
        public int Top    => GridY;
        public int Right  => GridX + SegmentSize - 1;
        public int Bottom => GridY + SegmentSize - 1;
        public int CenterX => GridX + SegmentSize / 2;
        public int CenterY => GridY + SegmentSize / 2;
    }

    public class GameMapConnection
    {
        public string Id { get; set; } = "";
        public string FromNodeId { get; set; } = "";
        public string ToNodeId { get; set; } = "";
        public int SegmentCount { get; set; } = 0;
        public TravelDistance Distance => SegmentCount switch
        {
            <= 5  => TravelDistance.Immediate,
            <= 12 => TravelDistance.Close,
            <= 20 => TravelDistance.Near,
            <= 30 => TravelDistance.Far,
            _     => TravelDistance.Distant
        };
        public List<(int x, int y)> Path { get; set; } = new();
        public bool IsDiscovered { get; set; } = false;
        public bool IsPlayerTravelling { get; set; } = false;
        public double TravelProgress { get; set; } = 0.0;
    }

    public class BorderEntryPoint
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public Direction ArrivalDirection { get; set; }
        public string? ConnectedNodeId { get; set; }
        public bool IsPlayerHere { get; set; } = false;
        public Direction BorderFace => ArrivalDirection switch
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

    public class GeneratedMap
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public GameMapType Type { get; set; }
        public MapSize Size { get; set; } = MapSize.Medium;

        public int Width => Type switch
        {
            GameMapType.Region when Id == "unknown_region" => 3000,
            GameMapType.Region => 5000,
            GameMapType.Location => Size switch
            {
                MapSize.Small  => 500,
                MapSize.Medium => 1000,
                MapSize.Large  => 2000,
                _ => 1000
            },
            GameMapType.Landmark => Size switch
            {
                MapSize.Small  => 50,
                MapSize.Medium => 100,
                MapSize.Large  => 300,
                _ => 100
            },
            _ => 1000
        };
        public int Height => Width;

        public MapSegment[,] Grid { get; private set; }
            = new MapSegment[0, 0];
        public List<GameMapNode> Nodes { get; set; } = new();
        public List<GameMapConnection> Connections { get; set; } = new();
        public List<BorderEntryPoint> BorderEntries { get; set; } = new();

        public bool IsPlayerPresent { get; set; } = false;
        public bool WasPlayerPresent { get; set; } = false;
        public string? PlayerNodeId { get; set; } = null;
        public (int x, int y) PlayerPosition { get; set; }

        public int Seed { get; set; }
        public RiskLevel GlobalRisk { get; set; } = RiskLevel.Low;
        public int LastRiskUpdateDay { get; set; } = 0;

        public void InitializeGrid()
        {
            Grid = new MapSegment[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Grid[x, y] = new MapSegment();
        }

        public bool IsInBounds(int x, int y)
            => x >= 0 && x < Width && y >= 0 && y < Height;

        public bool IsFreeForNode(int x, int y, int size)
        {
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

        public MapSegment GetSegment(int x, int y)
            => IsInBounds(x, y) ? Grid[x, y] : new MapSegment();

        public void SetSegment(int x, int y,
            SegmentType type, string? nodeId = null,
            string? connectionId = null,
            Direction dir = Direction.None)
        {
            if (!IsInBounds(x, y)) return;
            Grid[x, y].Type = type;
            if (nodeId != null) Grid[x, y].NodeId = nodeId;
            if (connectionId != null)
                Grid[x, y].ConnectionId = connectionId;
            Grid[x, y].ConnectionDirection = dir;
        }

        public GameMapNode? GetNode(string id)
            => Nodes.Find(n => n.Id == id);

        public List<GameMapConnection> GetNodeConnections(
            string nodeId)
            => Connections.FindAll(c =>
                c.FromNodeId == nodeId ||
                c.ToNodeId == nodeId);

        public TravelDistance? GetDistance(
            string fromId, string toId)
        {
            var conn = Connections.Find(c =>
                (c.FromNodeId == fromId && c.ToNodeId == toId) ||
                (c.FromNodeId == toId && c.ToNodeId == fromId));
            return conn?.Distance;
        }
    }
}