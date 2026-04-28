using System.Collections.Generic;

namespace LostInAForgottenCity.Models
{
    public class Player
    {
        public string CurrentLocationId { get; set; } = "entrance";
        public int Sanity { get; set; } = 100;
        public int Danger { get; set; } = 0;
        public List<string> Inventory { get; set; } = new();
        public List<string> CompletedQuests { get; set; } = new();
        public List<string> ActiveQuests { get; set; } = new();
        public List<string> VisitedLocations { get; set; } = new();
    }
}