using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace LostInAForgottenCity.Engine
{
    public class NavigationManager
    {
        // ── Singleton ─────────────────────────────
        public static NavigationManager Instance { get; }
            = new NavigationManager();
        private NavigationManager() { }

        // ── Fields ────────────────────────────────
        private GameState _state = GameState.Instance;
        private DispatcherTimer _travelTimer = new();
        private int _travelSegment = 0;
        private int _totalSegments = 0;
        private GameMapConnection? _travelConnection = null;

        // ── Maps ──────────────────────────────────
        // Region map (current region)
        public GeneratedMap? RegionMap { get; private set; }

        // Location maps — keyed by node ID on region map
        private Dictionary<string, GeneratedMap>
            _locationMaps = new();

        // Currently displayed map
        public GeneratedMap? ActiveLocationMap
            { get; private set; }

        // ── Player position ───────────────────────
        public string CurrentRegionNodeId
            { get; private set; } = "";
        public string CurrentLandmarkNodeId
            { get; private set; } = "";
        public bool IsInLocation
            => ActiveLocationMap != null;
        public bool IsTravelling
            => _travelTimer.IsEnabled;

        // ── Look around state ─────────────────────
        // Key = locationNodeId + "_" + landmarkNodeId
        private HashSet<string> _usedLookAround = new();

        // ── Events ────────────────────────────────
        public event Action<GeneratedMap>? OnMapChanged;
        public event Action<List<NavigationOption>>?
            OnOptionsGenerated;
        public event Action<string, TextType>?
            OnConsoleMessage;
        public event Action<StatEffect>? OnStatEffect;
        public event Action? OnTravelStart;
        public event Action? OnTravelComplete;
        public event Action<string>? OnNarrativeTrigger;

        // ── Initialization ────────────────────────

        public void SetRegionMap(GeneratedMap map)
        {
            RegionMap = map;
        }

        public void AddLocationMap(string regionNodeId,
            GeneratedMap locationMap)
        {
            _locationMaps[regionNodeId] = locationMap;
        }

        public void SetStartPosition(
            string regionNodeId,
            string landmarkNodeId)
        {
            CurrentRegionNodeId = regionNodeId;
            CurrentLandmarkNodeId = landmarkNodeId;

            // Set player on region map
            if (RegionMap != null)
            {
                MapGenerator.SetPlayerPosition(
                    RegionMap, regionNodeId);
                MapGenerator.DiscoverVisibleNodes(
                    RegionMap, regionNodeId);
            }

            // Load location map and set player
            if (_locationMaps.TryGetValue(
                regionNodeId, out var locMap))
            {
                ActiveLocationMap = locMap;
                MapGenerator.SetPlayerPosition(
                    locMap, landmarkNodeId);
                MapGenerator.DiscoverVisibleNodes(
                    locMap, landmarkNodeId);
                OnMapChanged?.Invoke(locMap);
            }

            GenerateOptions();
        }

        // ── Option generation ─────────────────────

        public void GenerateOptions()
        {
            if (IsTravelling) return;

            var options = new List<NavigationOption>();

            // Look Around — if not used here yet
            string lookKey = LookAroundKey();
            if (!_usedLookAround.Contains(lookKey))
            {
                options.Add(new NavigationOption
                {
                    Type = OptionType.LookAround,
                    Label = "Look around.",
                    Distance = TravelDistance.Immediate,
                    NavState = NavigationState.OnThePath
                });
            }

            // Movement options from map connections
            var currentMap = ActiveLocationMap ?? RegionMap;
            if (currentMap == null)
            {
                OnOptionsGenerated?.Invoke(options);
                return;
            }

            string currentNodeId = IsInLocation
                ? CurrentLandmarkNodeId
                : CurrentRegionNodeId;

            var destinations = MapGenerator
                .GetAvailableDestinations(
                    currentMap, currentNodeId);

            foreach (var (nodeId, nodeName, distance)
                in destinations)
            {
                var node = currentMap.GetNode(nodeId);
                if (node == null) continue;

                string label = node.State ==
                    NodeState.Undiscovered
                    ? $"Head towards the {GetDirection(currentMap, currentNodeId, nodeId)}."
                    : $"Head to {nodeName}.";

                // If look around used, show distance
                if (_usedLookAround.Contains(lookKey))
                    label = $"Head to {nodeName}. " +
                        $"[{distance}]";

                options.Add(new NavigationOption
                {
                    Type = OptionType.MoveTo,
                    Label = label,
                    TargetNodeId = nodeId,
                    TargetNodeName = nodeName,
                    Distance = distance,
                    NavState = IsInLocation
                        ? NavigationState.OnThePath
                        : NavigationState.OnTheRoad
                });
            }

            // Border exit — travel to next region location
            if (IsInLocation && RegionMap != null)
            {
                var regionDestinations = MapGenerator
                    .GetAvailableDestinations(
                        RegionMap, CurrentRegionNodeId);

                foreach (var (nodeId, nodeName, distance)
                    in regionDestinations)
                {
                    var node = RegionMap.GetNode(nodeId);
                    if (node == null) continue;

                    options.Add(new NavigationOption
                    {
                        Type = OptionType.MoveToLocation,
                        Label = $"Leave towards {nodeName}.",
                        TargetNodeId = nodeId,
                        TargetNodeName = nodeName,
                        Distance = distance,
                        NavState = NavigationState.OnTheRoad
                    });
                }
            }

            OnOptionsGenerated?.Invoke(options);
        }

        // ── Look Around ───────────────────────────

        public void ExecuteLookAround()
        {
            var currentMap = ActiveLocationMap ?? RegionMap;
            if (currentMap == null) return;

            string currentNodeId = IsInLocation
                ? CurrentLandmarkNodeId
                : CurrentRegionNodeId;

            // Mark as used
            _usedLookAround.Add(LookAroundKey());

            // Discover connected nodes
            MapGenerator.DiscoverVisibleNodes(
                currentMap, currentNodeId);

            // Apply cost
            var effect = new StatEffect
            {
                Stamina = -1,
                TimeMinutes = 5
            };
            OnStatEffect?.Invoke(effect);

            // Build description
            var destinations = MapGenerator
                .GetAvailableDestinations(
                    currentMap, currentNodeId);

            OnConsoleMessage?.Invoke(
                "You take a moment to assess " +
                "your surroundings.", TextType.Description);

            foreach (var (nodeId, nodeName, distance)
                in destinations)
            {
                var node = currentMap.GetNode(nodeId);
                string distLabel = distance.ToString()
                    .ToUpper();
                string nameLabel = node?.State ==
                    NodeState.Undiscovered
                    ? "???" : nodeName;

                OnConsoleMessage?.Invoke(
                    $"- {nameLabel} [{distLabel}]",
                    TextType.Description);
            }

            OnConsoleMessage?.Invoke(
                "Used 1 stamina. 5 minutes passed.",
                TextType.Gameline);

            // Fire narrative trigger first time
            OnNarrativeTrigger?.Invoke(
                "look_around_first");

            // Regenerate options with distances shown
            GenerateOptions();
        }

        // ── Movement execution ────────────────────

        public void ExecuteMovement(
            NavigationOption option,
            MovementType movType)
        {
            if (option.Type == OptionType.MoveToLocation)
                ExecuteLocationTravel(option, movType);
            else
                ExecuteLandmarkTravel(option, movType);
        }

        private void ExecuteLandmarkTravel(
            NavigationOption option,
            MovementType movType)
        {
            var effect = MovementSystem.Calculate(
                option.NavState,
                option.Distance,
                movType);

            // Apply stats
            OnStatEffect?.Invoke(effect);

            // Show result
            OnConsoleMessage?.Invoke(
                MovementSystem.GetResultGameline(effect),
                TextType.Gameline);

            // Start map travel animation
            var currentMap = ActiveLocationMap ?? RegionMap;
            if (currentMap == null) return;

            string fromId = IsInLocation
                ? CurrentLandmarkNodeId
                : CurrentRegionNodeId;

            var conn = currentMap.Connections.FirstOrDefault(
                c => (c.FromNodeId == fromId &&
                      c.ToNodeId == option.TargetNodeId) ||
                     (c.FromNodeId == option.TargetNodeId &&
                      c.ToNodeId == fromId));

            if (conn != null)
                StartTravelAnimation(
                    currentMap, conn,
                    option.TargetNodeId,
                    movType,
                    () => ArriveAtLandmark(option));
            else
                ArriveAtLandmark(option);
        }

        private void ExecuteLocationTravel(
            NavigationOption option,
            MovementType movType)
        {
            var effect = MovementSystem.Calculate(
                NavigationState.OnTheRoad,
                option.Distance,
                movType);

            OnStatEffect?.Invoke(effect);

            OnConsoleMessage?.Invoke(
                MovementSystem.GetResultGameline(effect),
                TextType.Gameline);

            // Switch to region map for travel
            if (RegionMap != null)
            {
                OnMapChanged?.Invoke(RegionMap);

                var conn = RegionMap.Connections
                    .FirstOrDefault(c =>
                    (c.FromNodeId == CurrentRegionNodeId &&
                     c.ToNodeId == option.TargetNodeId) ||
                    (c.FromNodeId == option.TargetNodeId &&
                     c.ToNodeId == CurrentRegionNodeId));

                if (conn != null)
                    StartTravelAnimation(
                        RegionMap, conn,
                        option.TargetNodeId,
                        movType,
                        () => ArriveAtLocation(option));
                else
                    ArriveAtLocation(option);
            }
        }

        private void ArriveAtLandmark(
            NavigationOption option)
        {
            string prevLandmark = CurrentLandmarkNodeId;
            CurrentLandmarkNodeId = option.TargetNodeId;

            // Reset look around for previous landmark
            // (re-entering same landmark allowed)
            _usedLookAround.Remove(
                LookAroundKey(prevLandmark));

            // Update map state
            if (ActiveLocationMap != null)
            {
                MapGenerator.SetPlayerPosition(
                    ActiveLocationMap,
                    CurrentLandmarkNodeId);
                MapGenerator.DiscoverVisibleNodes(
                    ActiveLocationMap,
                    CurrentLandmarkNodeId);
            }

            // Arrival message
            var node = ActiveLocationMap?.GetNode(
                CurrentLandmarkNodeId);
            string name = node?.Name ?? "the area";

            OnConsoleMessage?.Invoke(
                $"You arrive at {name}.",
                TextType.Description);

            // Fire narrative trigger
            OnNarrativeTrigger?.Invoke(
                $"arrive_{CurrentLandmarkNodeId}");

            // Generate new options
            GenerateOptions();
        }

        private void ArriveAtLocation(
            NavigationOption option)
        {
            CurrentRegionNodeId = option.TargetNodeId;

            // Update region map
            if (RegionMap != null)
            {
                MapGenerator.SetPlayerPosition(
                    RegionMap, CurrentRegionNodeId);
                MapGenerator.DiscoverVisibleNodes(
                    RegionMap, CurrentRegionNodeId);
            }

            // Load location map
            if (_locationMaps.TryGetValue(
                CurrentRegionNodeId,
                out var newLocMap))
            {
                ActiveLocationMap = newLocMap;

                // Find arrival landmark (nearest to border)
                var arrivalLandmark = newLocMap.Nodes
                    .OrderBy(n => n.GridY)
                    .LastOrDefault(); // southernmost

                if (arrivalLandmark != null)
                {
                    CurrentLandmarkNodeId =
                        arrivalLandmark.Id;
                    MapGenerator.SetPlayerPosition(
                        newLocMap, arrivalLandmark.Id);
                    MapGenerator.DiscoverVisibleNodes(
                        newLocMap, arrivalLandmark.Id);
                }

                OnMapChanged?.Invoke(newLocMap);
            }

            // Arrival message
            var regionNode = RegionMap?.GetNode(
                CurrentRegionNodeId);
            string name = regionNode?.Name ?? "the area";

            OnConsoleMessage?.Invoke(
                $"He arrived in {name}.",
                TextType.Description);

            OnNarrativeTrigger?.Invoke(
                $"arrive_{CurrentRegionNodeId}");

            GenerateOptions();
        }

        // ── Travel animation ──────────────────────

        private void StartTravelAnimation(
            GeneratedMap map,
            GameMapConnection conn,
            string targetNodeId,
            MovementType movType,
            Action onComplete)
        {
            conn.IsPlayerTravelling = true;
            conn.TravelProgress = 0;

            _travelConnection = conn;
            _totalSegments = MovementSystem
                .GetTravelSegments(
                    conn.Distance, movType);
            _travelSegment = 0;

            OnConsoleMessage?.Invoke(
                "Traveling . . . . . .",
                TextType.Description);

            OnTravelStart?.Invoke();

            _travelTimer.Interval =
                TimeSpan.FromMilliseconds(500);
            _travelTimer.Tick += (s, e) =>
            {
                _travelSegment++;
                conn.TravelProgress =
                    _travelSegment / (double)_totalSegments;

                if (_travelSegment >= _totalSegments)
                {
                    _travelTimer.Stop();
                    conn.IsPlayerTravelling = false;
                    conn.TravelProgress = 0;
                    OnTravelComplete?.Invoke();
                    onComplete();
                }
            };

            _travelTimer.Start();
        }

        // ── Helpers ───────────────────────────────

        private string LookAroundKey(
            string? landmarkId = null)
        {
            string lid = landmarkId
                ?? CurrentLandmarkNodeId;
            return $"{CurrentRegionNodeId}_{lid}";
        }

        private string GetDirection(
            GeneratedMap map,
            string fromId, string toId)
        {
            var from = map.GetNode(fromId);
            var to = map.GetNode(toId);
            if (from == null || to == null) return "area";

            int dx = to.CenterX - from.CenterX;
            int dy = to.CenterY - from.CenterY;

            if (Math.Abs(dx) < Math.Abs(dy) * 0.5)
                return dy < 0 ? "north" : "south";
            if (Math.Abs(dy) < Math.Abs(dx) * 0.5)
                return dx < 0 ? "west" : "east";
            if (dx > 0 && dy < 0) return "northeast";
            if (dx > 0 && dy > 0) return "southeast";
            if (dx < 0 && dy < 0) return "northwest";
            return "southwest";
        }

        // Reset look around when leaving location
        public void ClearLookAroundForLocation(
            string regionNodeId)
        {
            _usedLookAround.RemoveWhere(
                k => k.StartsWith(regionNodeId + "_"));
        }
    }

    // ── Navigation option ─────────────────────────

    public class NavigationOption
    {
        public OptionType Type { get; set; }
        public string Label { get; set; } = "";
        public string? TargetNodeId { get; set; }
        public string? TargetNodeName { get; set; }
        public TravelDistance Distance { get; set; }
        public NavigationState NavState { get; set; }
    }
}