using System.Collections.Generic;

namespace LostInAForgottenCity.Models
{
    public class Location
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Zone { get; set; }
        public string? BackgroundColor { get; set; }
        public List<string> Exits { get; set; } = new();
        public List<string> ItemIds { get; set; } = new();
        public string? NpcId { get; set; }
        public bool IsLocked { get; set; } = false;
        public string? UnlockItemId { get; set; }
    }
}