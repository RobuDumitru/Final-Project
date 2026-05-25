using System.Collections.Generic;

namespace LostInAForgottenCity.Models
{
    public class Location
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsLocked { get; set; } = false;
        public string Region { get; set; } = "";
        public List<string> ItemIds { get; set; } = new();
        public List<string> NpcIds { get; set; } = new();
        public List<string> ConnectedLocationIds { get; set; } = new();
        public bool IsTutorial { get; set; } = false;
        public bool IsSafeRoom { get; set; } = false;
        public bool HasDanger { get; set; } = false;
    }
}