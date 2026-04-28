using LostInAForgottenCity.Models;

namespace LostInAForgottenCity.Engine
{
    public class GameEngine
    {
        public Player CurrentPlayer { get; set; }
        public Dictionary<string, Location> Locations { get; set; }
        public Dictionary<string, Item> Items { get; set; }
        public Dictionary<string, NPC> NPCs { get; set; }
        public void MovePlayer(string locationId)
        {
            if (Locations.ContainsKey(locationId))
            {
                var location = Locations[locationId];
                if (location.IsLocked)
                {
                    Console.WriteLine("The location is locked. You need to find the required item to unlock it.");
                    return;
                }
                CurrentPlayer.CurrentLocationId = locationId;
                Console.WriteLine($"You have moved to: {location.Name}");
                Console.WriteLine(location.Description);
            }
            else
            {
                Console.WriteLine("Invalid location.");
            }
            if (!CurrentPlayer.VisitedLocations.Contains(locationId))
            {
                CurrentPlayer.VisitedLocations.Add(locationId);
            }
        }
        public void PickUpItem(string itemId)
        {
            var currentLocation = Locations[CurrentPlayer.CurrentLocationId];
            if (currentLocation.ItemIds.Contains(itemId))
            {
                CurrentPlayer.Inventory.Add(itemId);
                currentLocation.ItemIds.Remove(itemId);
                if (Items.ContainsKey(itemId))
                {
                    Console.WriteLine($"You picked up: {Items[itemId].Name}");
                }
                else
                {
                    Console.WriteLine("You picked up an item.");
                }
            }
            else
            {
                Console.WriteLine("Item not found in this location.");
            }
        }
        public void TalkToNPC(string npcId)
        {
            var currentLocation = Locations[CurrentPlayer.CurrentLocationId];
            if (currentLocation.NpcId == npcId)
            {
                if (NPCs.ContainsKey(npcId))
                {
                    var npc = NPCs[npcId];
                    Console.WriteLine($"You talk to {npc.Name}:");
                    Console.WriteLine(npc.Dialogue);
                }
                else
                {
                    Console.WriteLine("You talk to someone, but they don't respond.");
                }
            }
            else
            {
                Console.WriteLine("No one to talk to here.");
            }
        }
        public void TalkToNPC(string npcId)
        {
            // TO DO: implement dialogue system here
            Console.WriteLine("Dialogue system is not implemented yet.");
        }
        public Location GetCurrentLocation() => Locations[CurrentPlayer.CurrentLocationId];
    }
}