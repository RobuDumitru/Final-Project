using System.Collections.Generic;
using LostInAForgottenCity.Models;

namespace LostInAForgottenCity.Engine
{
    public static class TutorialData
    {
        // ── LOCATIONS ───────────────────────────

        public static Dictionary<string, Location> 
            GetSimplifiedLocations()
        {
            return new Dictionary<string, Location>
            {
                // ── REGION: Unknown ──────────────

                ["unknown_ruins"] = new Location
                {
                    Id = "unknown_ruins",
                    Name = "Random Ruins",
                    Description = 
                        "The remains of what was once a " +
                        "neighbourhood. Broken walls and " +
                        "collapsed rooftops stretch in every " +
                        "direction. The air smells of dust " +
                        "and old wood.",
                    IsLocked = false,
                    ItemIds = new List<string> 
                    { 
                        "tut_bandage", 
                        "tut_note_ruins" 
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_ruins_house",
                        "unknown_ruins_store",
                        "unknown_ruins_warehouse",
                        "unknown_ruins_tower"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_ruins_house"] = new Location
                {
                    Id = "unknown_ruins_house",
                    Name = "Intact House",
                    Description =
                        "Surprisingly, this house still stands. " +
                        "The door is unlocked. Inside, dust " +
                        "covers every surface. Someone lived " +
                        "here not long ago.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_canned_food",
                        "tut_old_key"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_ruins"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_ruins_store"] = new Location
                {
                    Id = "unknown_ruins_store",
                    Name = "Damaged Store",
                    Description =
                        "The front window is shattered. " +
                        "Shelves are overturned, most goods " +
                        "long since taken. A few items remain " +
                        "scattered on the floor.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_matches",
                        "tut_note_store"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_ruins"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_ruins_warehouse"] = new Location
                {
                    Id = "unknown_ruins_warehouse",
                    Name = "Warehouse",
                    Description =
                        "A large storage building. Most of " +
                        "the loading doors are jammed shut. " +
                        "The interior is dark and cavernous. " +
                        "Something moves in the shadows.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_rope",
                        "tut_lantern"
                    },
                    NpcIds = new List<string> 
                    { 
                        "tut_wanderer" 
                    },
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_ruins"
                    },
                    Region = "Unknown",
                    IsTutorial = true,
                    HasDanger = true
                },

                ["unknown_ruins_tower"] = new Location
                {
                    Id = "unknown_ruins_tower",
                    Name = "Half Collapsed Tower",
                    Description =
                        "Only half the tower remains standing. " +
                        "The upper floors are exposed to the " +
                        "sky. From here you can see most of " +
                        "the surrounding ruins.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_note_tower"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_ruins"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_palace"] = new Location
                {
                    Id = "unknown_palace",
                    Name = "Extravagant Palace",
                    Description =
                        "A grand building that seems out of " +
                        "place among the ruins. Its facade is " +
                        "still largely intact. The front gates " +
                        "stand open as if expecting visitors.",
                    IsLocked = false,
                    ItemIds = new List<string>(),
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_ruins",
                        "unknown_palace_hall",
                        "unknown_palace_basement",
                        "unknown_palace_storage",
                        "unknown_palace_kitchen",
                        "unknown_palace_bedroom",
                        "unknown_palace_sturdy"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_palace_hall"] = new Location
                {
                    Id = "unknown_palace_hall",
                    Name = "Main Hall",
                    Description =
                        "A vast entrance hall with high " +
                        "ceilings. Chandeliers hang dark " +
                        "overhead. A grand staircase leads " +
                        "to the upper floors, but the steps " +
                        "look unstable.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_note_palace",
                        "tut_ceremonial_key"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_palace",
                        "unknown_palace_basement",
                        "unknown_palace_storage"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_palace_basement"] = new Location
                {
                    Id = "unknown_palace_basement",
                    Name = "Basement",
                    Description =
                        "Stone steps lead down into darkness. " +
                        "The air is cold and damp. Old crates " +
                        "and forgotten furniture crowd the " +
                        "space. The silence here feels heavy.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_ritual_doll",
                        "tut_strange_box"
                    },
                    NpcIds = new List<string>
                    {
                        "tut_mournful"
                    },
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_palace_hall"
                    },
                    Region = "Unknown",
                    IsTutorial = true,
                    HasDanger = true
                },

                ["unknown_palace_storage"] = new Location
                {
                    Id = "unknown_palace_storage",
                    Name = "Storage Room",
                    Description =
                        "Shelves line every wall, filled with " +
                        "old supplies. Most are ruined by time " +
                        "and moisture. A few sealed containers " +
                        "might still hold something useful.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_healing_herbs",
                        "tut_pouch"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_palace_hall",
                        "unknown_palace_kitchen"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_palace_kitchen"] = new Location
                {
                    Id = "unknown_palace_kitchen",
                    Name = "Kitchen",
                    Description =
                        "A large kitchen built to serve many. " +
                        "The stoves are cold and rusted. " +
                        "A smell of something long spoiled " +
                        "lingers in the air.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_canned_food_2",
                        "tut_water_flask"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_palace_storage",
                        "unknown_palace_bedroom"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_palace_bedroom"] = new Location
                {
                    Id = "unknown_palace_bedroom",
                    Name = "Bedroom",
                    Description =
                        "A large bedroom with a canopied bed. " +
                        "Personal belongings are scattered " +
                        "around as if the owner left in a " +
                        "hurry and never returned.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_journal",
                        "tut_relic_1"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_palace_kitchen",
                        "unknown_palace_sturdy"
                    },
                    Region = "Unknown",
                    IsTutorial = true
                },

                ["unknown_palace_sturdy"] = new Location
                {
                    Id = "unknown_palace_sturdy",
                    Name = "Sturdy Room",
                    Description =
                        "A small reinforced room, perhaps once " +
                        "used as a panic room or vault. The " +
                        "walls are thick and the door is heavy. " +
                        "It feels safe here. Calmer.",
                    IsLocked = false,
                    ItemIds = new List<string>
                    {
                        "tut_bandage_2",
                        "tut_note_safe"
                    },
                    NpcIds = new List<string>(),
                    ConnectedLocationIds = new List<string>
                    {
                        "unknown_palace_bedroom"
                    },
                    Region = "Unknown",
                    IsTutorial = true,
                    IsSafeRoom = true
                }
            };
        }

        // ── ITEMS ────────────────────────────────

        public static Dictionary<string, Item> 
            GetSimplifiedItems()
        {
            return new Dictionary<string, Item>
            {
                ["tut_bandage"] = new Item
                {
                    Id = "tut_bandage",
                    Name = "Bandage",
                    Description = 
                        "A roll of cloth bandage. Useful for " +
                        "treating minor wounds.",
                    IsUsable = true,
                    Effect = "heal_hp_2",
                    IsTutorial = true
                },

                ["tut_bandage_2"] = new Item
                {
                    Id = "tut_bandage_2",
                    Name = "Bandage",
                    Description =
                        "A roll of cloth bandage.",
                    IsUsable = true,
                    Effect = "heal_hp_2",
                    IsTutorial = true
                },

                ["tut_canned_food"] = new Item
                {
                    Id = "tut_canned_food",
                    Name = "Canned Food",
                    Description =
                        "A dented can of preserved food. " +
                        "Not appetising but edible.",
                    IsUsable = true,
                    Effect = "restore_stamina_2",
                    IsTutorial = true
                },

                ["tut_canned_food_2"] = new Item
                {
                    Id = "tut_canned_food_2",
                    Name = "Canned Food",
                    Description =
                        "A dented can of preserved food.",
                    IsUsable = true,
                    Effect = "restore_stamina_2",
                    IsTutorial = true
                },

                ["tut_matches"] = new Item
                {
                    Id = "tut_matches",
                    Name = "Matches",
                    Description =
                        "A small box of matches. " +
                        "Useful for lighting things.",
                    IsUsable = false,
                    IsTutorial = true
                },

                ["tut_rope"] = new Item
                {
                    Id = "tut_rope",
                    Name = "Rope",
                    Description =
                        "A length of sturdy rope. " +
                        "Could be useful in many situations.",
                    IsUsable = false,
                    IsTutorial = true
                },

                ["tut_lantern"] = new Item
                {
                    Id = "tut_lantern",
                    Name = "Old Lantern",
                    Description =
                        "A metal lantern, dusty but intact. " +
                        "Needs matches to light.",
                    IsUsable = true,
                    Effect = "light_area",
                    RequiredItem = "tut_matches",
                    IsTutorial = true
                },

                ["tut_old_key"] = new Item
                {
                    Id = "tut_old_key",
                    Name = "Old Key",
                    Description =
                        "A worn iron key. It might open " +
                        "something nearby.",
                    IsUsable = false,
                    IsTutorial = true
                },

                ["tut_healing_herbs"] = new Item
                {
                    Id = "tut_healing_herbs",
                    Name = "Healing Herbs",
                    Description =
                        "A bundle of dried herbs. Someone " +
                        "knowledgeable prepared these.",
                    IsUsable = true,
                    Effect = "restore_stamina_3",
                    IsTutorial = true
                },

                ["tut_pouch"] = new Item
                {
                    Id = "tut_pouch",
                    Name = "Leather Pouch",
                    Description =
                        "A sturdy leather pouch. It could " +
                        "hold more items.",
                    IsUsable = true,
                    Effect = "expand_inventory",
                    IsTutorial = true
                },

                ["tut_water_flask"] = new Item
                {
                    Id = "tut_water_flask",
                    Name = "Water Flask",
                    Description =
                        "A metal flask. Still has some " +
                        "clean water inside.",
                    IsUsable = true,
                    Effect = "restore_sleep_20",
                    IsTutorial = true
                },

                ["tut_ritual_doll"] = new Item
                {
                    Id = "tut_ritual_doll",
                    Name = "Ritual Doll",
                    Description =
                        "A strange cloth doll covered in " +
                        "markings. It feels wrong to hold.",
                    IsUsable = false,
                    IsTutorial = true
                },

                ["tut_strange_box"] = new Item
                {
                    Id = "tut_strange_box",
                    Name = "Strange Box",
                    Description =
                        "A small box covered in carved " +
                        "symbols. It doesn't open normally.",
                    IsUsable = false,
                    IsTutorial = true
                },

                ["tut_ceremonial_key"] = new Item
                {
                    Id = "tut_ceremonial_key",
                    Name = "Ceremonial Key",
                    Description =
                        "An ornate key with unusual markings. " +
                        "It seems important.",
                    IsUsable = false,
                    IsTutorial = true
                },

                ["tut_journal"] = new Item
                {
                    Id = "tut_journal",
                    Name = "Leather Journal",
                    Description =
                        "A journal with handwritten entries. " +
                        "The last entry is dated 1985.",
                    IsUsable = true,
                    Effect = "read_journal",
                    IsTutorial = true
                },

                ["tut_relic_1"] = new Item
                {
                    Id = "tut_relic_1",
                    Name = "Strange Relic",
                    Description =
                        "A small carved object of unknown " +
                        "origin. It seems valuable.",
                    IsUsable = false,
                    IsRelic = true,
                    IsTutorial = true
                },

                // ── Notes ────────────────────────

                ["tut_note_ruins"] = new Item
                {
                    Id = "tut_note_ruins",
                    Name = "Torn Note",
                    Description =
                        "A torn piece of paper with hurried " +
                        "writing: 'Don't go near the warehouse " +
                        "after dark. I saw something.'",
                    IsUsable = true,
                    Effect = "read_note",
                    IsTutorial = true
                },

                ["tut_note_store"] = new Item
                {
                    Id = "tut_note_store",
                    Name = "Inventory List",
                    Description =
                        "An old store inventory list. " +
                        "Most items are crossed out. " +
                        "Someone was thorough.",
                    IsUsable = true,
                    Effect = "read_note",
                    IsTutorial = true
                },

                ["tut_note_tower"] = new Item
                {
                    Id = "tut_note_tower",
                    Name = "Carved Message",
                    Description =
                        "Words carved into the stone: " +
                        "'The palace at the end of the road. " +
                        "Don't enter at night.'",
                    IsUsable = true,
                    Effect = "read_note",
                    IsTutorial = true
                },

                ["tut_note_palace"] = new Item
                {
                    Id = "tut_note_palace",
                    Name = "Formal Letter",
                    Description =
                        "A letter on fine paper. The seal " +
                        "is broken. It speaks of a gathering " +
                        "that never concluded.",
                    IsUsable = true,
                    Effect = "read_note",
                    IsTutorial = true
                },

                ["tut_note_safe"] = new Item
                {
                    Id = "tut_note_safe",
                    Name = "Handwritten Note",
                    Description =
                        "Someone wrote: 'This room is safe. " +
                        "Rest here. Recover. Then keep going. " +
                        "Don't stop moving.'",
                    IsUsable = true,
                    Effect = "read_note",
                    IsTutorial = true
                }
            };
        }

        // ── NPCs ─────────────────────────────────

        public static Dictionary<string, NPC> 
            GetSimplifiedNPCs()
        {
            return new Dictionary<string, NPC>
            {
                ["tut_fortuneteller"] = new NPC
                {
                    Id = "tut_fortuneteller",
                    Name = "Fortuneteller",
                    Description =
                        "An old woman seated behind a crystal " +
                        "ball. Her eyes reflect something " +
                        "distant, as if she sees more than " +
                        "what is in front of her.",
                    IsHostile = false,
                    DialogueId = "dialogue_fortuneteller_intro",
                    IsTutorial = true
                },

                ["tut_hiker"] = new NPC
                {
                    Id = "tut_hiker",
                    Name = "The Traveler",
                    Description =
                        "A man seen through the crystal ball. " +
                        "He moves through the ruins carefully, " +
                        "unaware of what awaits him.",
                    IsHostile = false,
                    DialogueId = "",
                    IsTutorial = true
                },

                ["tut_wanderer"] = new NPC
                {
                    Id = "tut_wanderer",
                    Name = "???",
                    Description =
                        "Something moves in the darkness. " +
                        "It doesn't seem to notice you yet. " +
                        "A Night Dweller — a Wanderer.",
                    IsHostile = true,
                    IsNightDweller = true,
                    NightDwellerType = "Wanderer",
                    DialogueId = "",
                    IsTutorial = true
                },

                ["tut_mournful"] = new NPC
                {
                    Id = "tut_mournful",
                    Name = "???",
                    Description =
                        "A presence that fills the room with " +
                        "dread. It weeps softly in a corner, " +
                        "but its grief is dangerous.",
                    IsHostile = true,
                    IsNightDweller = true,
                    NightDwellerType = "Mournful",
                    DialogueId = "",
                    IsTutorial = true
                }
            };
        }
    }
}