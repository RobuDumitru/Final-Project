namespace LostInAForgottenCity.Engine
{
    // ── Text ──────────────────────────────────────
    public enum TextType
    {
        Description,
        Dialogue,
        Gameline
    }

    // ── Travel ────────────────────────────────────
    public enum TravelDistance
    {
        Immediate,
        Close,
        Near,
        Far,
        Distant
    }

    public enum NavigationState
    {
        OnTheWay,
        OnThePath,
        OnTheRoad
    }

    public enum MovementType
    {
        Carefully,
        Normally,
        Quickly
    }

    public enum OptionType
    {
        LookAround,
        MoveTo,
        MoveToLocation
    }

    // ── Map ───────────────────────────────────────
    public enum SegmentType : byte
    {
        Empty       = 0,
        Border      = 1,
        Node        = 2,
        Connection  = 3,
        Restriction = 4
    }

    public enum GameMapType
    {
        Region,
        Location,
        Landmark
    }

    public enum MapSize
    {
        Small,
        Medium,
        Large
    }

    public enum Direction
    {
        None,
        North, NorthEast, East, SouthEast,
        South, SouthWest, West, NorthWest
    }

    public enum NodeState
    {
        Undiscovered,
        Discovered,
        Visited,
        Explored,
        Looted
    }

    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }
}