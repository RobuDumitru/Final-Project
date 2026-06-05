using System.Collections.Generic;

namespace LostInAForgottenCity.Models
{
    public class Player
    {
        // ── Location ────────────────────────────
        public string CurrentLocationId { get; set; } = "entrance";
        public string TravellingToId { get; set; } = "";
        public bool IsTravelling { get; set; } = false;

        // ── MIND ────────────────────────────────
        public double Sanity { get; set; } = 100;
        public int Subconscious { get; set; } = 0;
        public int MaxSubconscious { get; set; } = 5;
        public int Danger { get; set; } = 0;
        public string StatusEffect { get; set; } = "";

        // ── SPIRIT ──────────────────────────────
        public int Soul { get; set; } = 50;
        public int MaxSoul { get; set; } = 50;
        public int Resistance { get; set; } = 20;
        public int MaxResistance { get; set; } = 20;

        // ── BODY ────────────────────────────────
        public int HP { get; set; } = 10;
        public int MaxHP { get; set; } = 10;
        public int Stamina { get; set; } = 5;
        public int MaxStamina { get; set; } = 5;
        public double Sleep { get; set; } = 100;

        // ── TIME ────────────────────────────────
        public int Day { get; set; } = 1;
        public int Hour { get; set; } = 15;
        public int Minute { get; set; } = 0;
        public string Date { get; set; } = "12/06/2012";

        // ── ENVIRONMENT ─────────────────────────
        public string Weather { get; set; } = "Foggy";
        public int Temperature { get; set; } = 8;
        public string FeelsLike { get; set; } = "Chilly";
        public string Hazard { get; set; } = "None";

        // ── INVENTORY ───────────────────────────
        public List<string> Inventory { get; set; } = new();
        public int MaxInventorySlots { get; set; } = 6;
        public int Relics { get; set; } = 0;

        // ── QUESTS ──────────────────────────────
        public List<string> ActiveQuests { get; set; } = new();
        public List<string> CompletedQuests { get; set; } = new();
        public List<string> Collections { get; set; } = new();

        // ── HISTORY ─────────────────────────────
        public List<string> NarrativeHistory { get; set; } = new();

        // ── DIFFICULTY ──────────────────────────
        public bool IsSunnyDay { get; set; } = false;
        public bool IsHardMode { get; set; } = false;
        public string CurseText { get; set; } = "";
        public List<string> VisitedLocations { get; set; } = new();

        public bool HasVisitedTutorial { get; set; } = false;
    }
}