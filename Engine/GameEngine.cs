using LostInAForgottenCity.Models;

namespace LostInAForgottenCity.Engine
{
    public class GameEngine
    {
        public Player CurrentPlayer { get; set; } = new();
        public Dictionary<string, Location> Locations { get; set; } = new();
        public Dictionary<string, Item> Items { get; set; } = new();
        public Dictionary<string, NPC> NPCs { get; set; } = new();
        public string LastMessage { get; private set; } = "";

        private void Log(string message)
        {
            LastMessage = message;
        }

        public void MovePlayer(string locationId)
        {
            if (!Locations.TryGetValue(locationId, out var location))
            {
                Log("Invalid location.");
                return;
            }

            if (location.IsLocked)
            {
                Log("The location is locked. You need the required item to unlock it.");
                return;
            }

            CurrentPlayer.CurrentLocationId = locationId;
            Log($"You have moved to: {location.Name}\n{location.Description}");

            if (!CurrentPlayer.VisitedLocations.Contains(locationId))
                CurrentPlayer.VisitedLocations.Add(locationId);
        }

        public void PickUpItem(string itemId)
        {
            var currentLocation = Locations[CurrentPlayer.CurrentLocationId];
            if (!currentLocation.ItemIds.Contains(itemId))
            {
                Log("Item not found in this location.");
                return;
            }

            CurrentPlayer.Inventory.Add(itemId);
            currentLocation.ItemIds.Remove(itemId);
            Log(Items.TryGetValue(itemId, out var item)
                ? $"You picked up: {item.Name}"
                : "You picked up an item.");
        }

        public void TalkToNPC(string npcId)
        {
            // TODO: Implement dialogue system
            Log("Dialogue system coming soon.");
        }

        public Location GetCurrentLocation() => Locations[CurrentPlayer.CurrentLocationId];
    }
}