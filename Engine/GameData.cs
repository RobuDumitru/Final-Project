using System.Collections.Generic;
using LostInAForgottenCity.Models;

namespace LostInAForgottenCity.Engine
{
    public static class GameData
    {
        public static Dictionary<string, Location> LoadLocations()
        {
            // create and return all locations
            return new Dictionary<string, Location>();
        }

        public static Dictionary<string, Item> LoadItems()
        {
            // create and return all items
            return new Dictionary<string, Item>();
        }

        public static Dictionary<string, NPC> LoadNPCs()
        {
            // create and return all NPCs
            return new Dictionary<string, NPC>();
        }
    }
}