namespace LostInAForgottenCity.Models
{
    public class Item
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsKeyItem { get; set; } = false;
    }
}