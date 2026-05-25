namespace LostInAForgottenCity.Models
{
    public class Item
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsUsable { get; set; } = false;
        public string Effect { get; set; } = "";
        public string RequiredItem { get; set; } = "";
        public bool IsRelic { get; set; } = false;
        public bool IsTutorial { get; set; } = false;
    }
}