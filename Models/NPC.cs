namespace LostInAForgottenCity.Models
{
    public class NPC
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsHostile { get; set; } = false;
        public bool IsNightDweller { get; set; } = false;
        public string NightDwellerType { get; set; } = "";
        public string DialogueId { get; set; } = "";
        public bool IsTutorial { get; set; } = false;
    }
}