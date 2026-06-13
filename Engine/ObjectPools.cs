using System;
using System.Collections.Generic;

namespace LostInAForgottenCity.Engine
{
    public static class ObjectPools
    {
        // ── Rarity chances by size ────────────────
        // Roll under this value = special spawns
        private const double ChanceSmall  = 0.28; // 28%
        private const double ChanceMedium = 0.18; // 18%
        private const double ChanceLarge  = 0.12; // 12%

        private static double GetChance(MapSize size)
            => size switch
            {
                MapSize.Small  => ChanceSmall,
                MapSize.Medium => ChanceMedium,
                MapSize.Large  => ChanceLarge,
                _ => ChanceSmall
            };

        // ── Main generation method ────────────────
        // Takes the landmark id, its size and a
        // random instance, returns a ready pool

        public static LandmarkObjectPool Generate(
            string landmarkId,
            MapSize landmarkSize,
            Random random)
        {
            var pairs = GetPairs(landmarkId);
            double chance = GetChance(landmarkSize);

            var pool = new LandmarkObjectPool
            {
                LandmarkId = landmarkId
            };

            foreach (var pair in pairs)
            {
                // Roll for special version
                bool spawnSpecial =
                    random.NextDouble() < chance;

                pool.Objects.Add(
                    spawnSpecial
                    ? pair.Special
                    : pair.Normal);
            }

            return pool;
        }

        // ── Object pair definitions ───────────────

        private static List<ObjectPair> GetPairs(
            string landmarkId)
        {
            return landmarkId switch
            {
                "me_parking_lot"   => ParkingLot(),
                "me_empty_booth"   => EmptyBooth(),
                "me_cluster_signs" => ClusterOfSigns(),
                "rr_intact_house"  => IntactHouse(),
                "rr_damaged_store" => DamagedStore(),
                "rr_warehouse"     => Warehouse(),
                "rr_tower"         => CollapsedTower(),
                "ep_main_hall"     => MainHall(),
                "ep_basement"      => Basement(),
                "ep_storage"       => StorageRoom(),
                "ep_kitchen"       => Kitchen(),
                "ep_bedroom"       => Bedroom(),
                _ => new List<ObjectPair>()
            };
        }

        // ── MOUNTAIN EDGE ─────────────────────────

        private static List<ObjectPair> ParkingLot()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "pl_car_normal",
                    Name = "Abandoned Car",
                    Description =
                        "A rusted car, doors sealed shut. " +
                        "Nothing of use here.",
                },
                Special = new LandmarkObject
                {
                    Id = "pl_car_special",
                    Name = "Unlocked Car",
                    IsSpecial = true,
                    Description =
                        "An old car, but the door is " +
                        "unlocked. Someone left in a hurry.",
                    SpecialDescription =
                        "Inside the glove compartment " +
                        "you find supplies.",
                    HasItem = true,
                    ItemId = "random_supply"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "pl_vending_normal",
                    Name = "Vending Machine",
                    Description =
                        "An old vending machine, empty. " +
                        "The glass is cracked.",
                },
                Special = new LandmarkObject
                {
                    Id = "pl_vending_special",
                    Name = "Jammed Vending Machine",
                    IsSpecial = true,
                    Description =
                        "The machine is jammed mid-drop. " +
                        "Something is stuck inside.",
                    SpecialDescription =
                        "You force the tray open and " +
                        "retrieve a preserved snack.",
                    HasItem = true,
                    ItemId = "snack"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "pl_bin_normal",
                    Name = "Trash Bin",
                    Description =
                        "A metal trash bin, long since " +
                        "emptied by time and weather.",
                },
                Special = new LandmarkObject
                {
                    Id = "pl_bin_special",
                    Name = "Overturned Bin",
                    IsSpecial = true,
                    Description =
                        "The bin is on its side. " +
                        "Something is pinned underneath.",
                    SpecialDescription =
                        "A weathered note, still legible. " +
                        "Someone left it deliberately.",
                    HasItem = true,
                    ItemId = "note_parking"
                }
            }
        };

        private static List<ObjectPair> EmptyBooth()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "eb_counter_normal",
                    Name = "Wooden Counter",
                    Description =
                        "A plain wooden counter, warped " +
                        "from moisture. Nothing on it.",
                },
                Special = new LandmarkObject
                {
                    Id = "eb_counter_special",
                    Name = "Counter with Hidden Drawer",
                    IsSpecial = true,
                    Description =
                        "The counter has an unusual seam " +
                        "along the front panel.",
                    SpecialDescription =
                        "A hidden drawer slides out. " +
                        "Inside — a small key.",
                    HasItem = true,
                    ItemId = "small_key"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "eb_locker_normal",
                    Name = "Metal Locker",
                    Description =
                        "A dented metal locker, door " +
                        "hanging open. Completely empty.",
                },
                Special = new LandmarkObject
                {
                    Id = "eb_locker_special",
                    Name = "Locked Locker",
                    IsSpecial = true,
                    Description =
                        "A locker, still sealed. " +
                        "The lock looks old but intact.",
                    SpecialDescription =
                        "With the right key this could " +
                        "be opened.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "eb_chair_normal",
                    Name = "Old Chair",
                    Description =
                        "A wooden chair with three legs. " +
                        "The fourth snapped long ago.",
                },
                Special = new LandmarkObject
                {
                    Id = "eb_chair_special",
                    Name = "Chair with Torn Cushion",
                    IsSpecial = true,
                    Description =
                        "A chair with a torn fabric " +
                        "cushion. Something bulges inside.",
                    SpecialDescription =
                        "Stuffed inside the cushion — " +
                        "a wrapped cloth package.",
                    HasItem = true,
                    ItemId = "cloth_package"
                }
            }
        };

        private static List<ObjectPair> ClusterOfSigns()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "cs_roadsign_normal",
                    Name = "Road Sign",
                    Description =
                        "A standard road sign, faded " +
                        "beyond readability.",
                },
                Special = new LandmarkObject
                {
                    Id = "cs_roadsign_special",
                    Name = "Defaced Sign",
                    IsSpecial = true,
                    Description =
                        "A sign deliberately marked over " +
                        "with black paint. Underneath...",
                    SpecialDescription =
                        "The original text reads: " +
                        "ILUMINATION — TURN BACK.",
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "cs_board_normal",
                    Name = "Notice Board",
                    Description =
                        "A wooden notice board, empty. " +
                        "Old pin holes mark where papers hung.",
                },
                Special = new LandmarkObject
                {
                    Id = "cs_board_special",
                    Name = "Notice Board with Recent Note",
                    IsSpecial = true,
                    Description =
                        "One note still clings to the " +
                        "board. The paper looks recent.",
                    SpecialDescription =
                        "The note reads: " +
                        "Day 11. I found a way in. " +
                        "Don't follow me. — D.",
                    HasItem = true,
                    ItemId = "note_board"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "cs_signpost_normal",
                    Name = "Fallen Signpost",
                    Description =
                        "A metal signpost lying in the " +
                        "dirt, too corroded to read.",
                },
                Special = new LandmarkObject
                {
                    Id = "cs_signpost_special",
                    Name = "Signpost with Carved Markings",
                    IsSpecial = true,
                    Description =
                        "This post has symbols carved " +
                        "deep into the metal. Not erosion.",
                    SpecialDescription =
                        "The symbols match markings " +
                        "described in the city's folklore.",
                }
            }
        };

        // ── RANDOM RUINS ──────────────────────────

        private static List<ObjectPair> IntactHouse()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ih_table_normal",
                    Name = "Dining Table",
                    Description =
                        "A dusty dining table, still set " +
                        "with plates. A meal left mid-bite.",
                },
                Special = new LandmarkObject
                {
                    Id = "ih_table_special",
                    Name = "Table with Hidden Compartment",
                    IsSpecial = true,
                    Description =
                        "The table has an unusual thickness " +
                        "to its center panel.",
                    SpecialDescription =
                        "A compartment releases. Inside — " +
                        "a family photograph and a key.",
                    HasItem = true,
                    ItemId = "photograph"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ih_shelf_normal",
                    Name = "Bookshelf",
                    Description =
                        "Rows of books, bloated from " +
                        "moisture. None are readable.",
                },
                Special = new LandmarkObject
                {
                    Id = "ih_shelf_special",
                    Name = "Bookshelf with False Back",
                    IsSpecial = true,
                    Description =
                        "One shelf sits slightly further " +
                        "back than the others.",
                    SpecialDescription =
                        "The false back swings open. " +
                        "A small cache of preserved items.",
                    HasItem = true,
                    ItemId = "cache_items"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ih_chest_normal",
                    Name = "Storage Chest",
                    Description =
                        "A wooden chest, lid rotted open. " +
                        "Empty except for old cloth.",
                },
                Special = new LandmarkObject
                {
                    Id = "ih_chest_special",
                    Name = "Locked Chest",
                    IsSpecial = true,
                    Description =
                        "A sturdy chest, padlocked shut. " +
                        "The lock shows no rust.",
                    SpecialDescription =
                        "This chest is sealed tight. " +
                        "You would need a key.",
                    HasItem = false
                }
            }
        };

        private static List<ObjectPair> DamagedStore()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ds_shelves_normal",
                    Name = "Store Shelves",
                    Description =
                        "Empty metal shelves, some " +
                        "collapsed. Long since looted.",
                },
                Special = new LandmarkObject
                {
                    Id = "ds_shelves_special",
                    Name = "Shelves with False Bottom",
                    IsSpecial = true,
                    Description =
                        "One shelf unit feels heavier " +
                        "than the others when nudged.",
                    SpecialDescription =
                        "The bottom panel lifts. " +
                        "Supplies hidden underneath.",
                    HasItem = true,
                    ItemId = "hidden_supplies"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ds_register_normal",
                    Name = "Cash Register",
                    Description =
                        "An old mechanical register, " +
                        "drawer hanging open. Empty.",
                },
                Special = new LandmarkObject
                {
                    Id = "ds_register_special",
                    Name = "Register with Jammed Drawer",
                    IsSpecial = true,
                    Description =
                        "The drawer is stuck half-open. " +
                        "Something is caught inside.",
                    SpecialDescription =
                        "A folded paper wedged in the " +
                        "mechanism. A handwritten list.",
                    HasItem = true,
                    ItemId = "handwritten_list"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ds_crate_normal",
                    Name = "Supply Crate",
                    Description =
                        "A wooden crate, sides split. " +
                        "Whatever was inside is long gone.",
                },
                Special = new LandmarkObject
                {
                    Id = "ds_crate_special",
                    Name = "Sealed Supply Crate",
                    IsSpecial = true,
                    Description =
                        "A crate banded with metal strips, " +
                        "still sealed. Surprisingly intact.",
                    SpecialDescription =
                        "The bands give way. Inside — " +
                        "preserved emergency rations.",
                    HasItem = true,
                    ItemId = "emergency_rations"
                }
            }
        };

        private static List<ObjectPair> Warehouse()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_rack_normal",
                    Name = "Storage Rack",
                    Description =
                        "A tall metal rack, shelves bare. " +
                        "Some bolts missing.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_rack_special",
                    Name = "Rack with Concealed Section",
                    IsSpecial = true,
                    Description =
                        "One section of the rack has a " +
                        "panel welded to the back.",
                    SpecialDescription =
                        "Behind the panel — a small " +
                        "sealed container.",
                    HasItem = true,
                    ItemId = "sealed_container"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_crates_normal",
                    Name = "Wooden Crates",
                    Description =
                        "A stack of crates, most crushed " +
                        "under their own weight.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_crates_special",
                    Name = "Reinforced Crate",
                    IsSpecial = true,
                    Description =
                        "One crate at the bottom is " +
                        "metal-reinforced and intact.",
                    SpecialDescription =
                        "The lid opens with effort. " +
                        "Equipment sealed in oilcloth.",
                    HasItem = true,
                    ItemId = "sealed_equipment"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_container_normal",
                    Name = "Metal Container",
                    Description =
                        "A shipping container, doors " +
                        "rusted open. Hollow inside.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_container_special",
                    Name = "Welded Container",
                    IsSpecial = true,
                    Description =
                        "The container doors are welded " +
                        "shut from the outside.",
                    SpecialDescription =
                        "Something moved inside " +
                        "when you knocked.",
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_bench_normal",
                    Name = "Workbench",
                    Description =
                        "A heavy wooden workbench, " +
                        "tools long removed.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_bench_special",
                    Name = "Workbench with Locked Drawer",
                    IsSpecial = true,
                    Description =
                        "One drawer has a combination " +
                        "lock fitted to it.",
                    SpecialDescription =
                        "You don't know the combination. " +
                        "But someone wrote numbers nearby.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_drum_normal",
                    Name = "Oil Drum",
                    Description =
                        "A rusted oil drum, empty and " +
                        "dented from the bottom.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_drum_special",
                    Name = "Drum with Hidden Contents",
                    IsSpecial = true,
                    Description =
                        "This drum is sealed with a " +
                        "newer cap than the rest.",
                    SpecialDescription =
                        "Inside — not oil. " +
                        "Someone stored provisions here.",
                    HasItem = true,
                    ItemId = "provisions"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_cabinet_normal",
                    Name = "Filing Cabinet",
                    Description =
                        "A metal filing cabinet, drawers " +
                        "pulled out and emptied.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_cabinet_special",
                    Name = "Cabinet with Locked Section",
                    IsSpecial = true,
                    Description =
                        "The bottom drawer is locked " +
                        "separately from the others.",
                    SpecialDescription =
                        "Inside — company documents " +
                        "marked CLASSIFIED.",
                    HasItem = true,
                    ItemId = "classified_documents"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_machinery_normal",
                    Name = "Broken Machinery",
                    Description =
                        "Heavy equipment, seized up. " +
                        "Stripped of useful parts long ago.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_machinery_special",
                    Name = "Machinery with Intact Compartment",
                    IsSpecial = true,
                    Description =
                        "Most of the machine is gutted " +
                        "but one access panel is intact.",
                    SpecialDescription =
                        "A maintenance compartment, " +
                        "untouched. Tools still inside.",
                    HasItem = true,
                    ItemId = "maintenance_tools"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "wh_rope_normal",
                    Name = "Rope Pile",
                    Description =
                        "A coil of old rope, brittle " +
                        "and frayed. Unusable.",
                },
                Special = new LandmarkObject
                {
                    Id = "wh_rope_special",
                    Name = "Rope Pile covering Trapdoor",
                    IsSpecial = true,
                    Description =
                        "Moving the rope reveals " +
                        "a trapdoor set into the floor.",
                    SpecialDescription =
                        "The trapdoor is padlocked. " +
                        "The drop below looks deep.",
                    HasItem = false
                }
            }
        };

        private static List<ObjectPair> CollapsedTower()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_rubble_normal",
                    Name = "Rubble Pile",
                    Description =
                        "A mound of collapsed stone " +
                        "and mortar. Unstable.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_rubble_special",
                    Name = "Rubble covering Old Chest",
                    IsSpecial = true,
                    Description =
                        "Something angular beneath " +
                        "the rubble catches your eye.",
                    SpecialDescription =
                        "A chest, partially crushed " +
                        "but still sealed on one side.",
                    HasItem = true,
                    ItemId = "crushed_chest_item"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_ladder_normal",
                    Name = "Old Ladder",
                    Description =
                        "A wooden ladder leaning " +
                        "against the wall, rungs rotted.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_ladder_special",
                    Name = "Ladder to Upper Section",
                    IsSpecial = true,
                    Description =
                        "This ladder is metal, " +
                        "bolted to the wall. Still holds.",
                    SpecialDescription =
                        "You climb up. A hidden platform " +
                        "with undisturbed belongings.",
                    HasItem = true,
                    ItemId = "platform_belongings"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_floor_normal",
                    Name = "Cracked Floor",
                    Description =
                        "The stone floor is fractured " +
                        "throughout. Watch your step.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_floor_special",
                    Name = "Floor with Hidden Hollow",
                    IsSpecial = true,
                    Description =
                        "One section of floor sounds " +
                        "hollow when you step on it.",
                    SpecialDescription =
                        "A stone lifts free. " +
                        "A small hollow, deliberately dug.",
                    HasItem = true,
                    ItemId = "hollow_cache"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_debris_normal",
                    Name = "Stone Debris",
                    Description =
                        "Scattered chunks of stone " +
                        "from the collapsed upper floors.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_debris_special",
                    Name = "Debris hiding Blocked Door",
                    IsSpecial = true,
                    Description =
                        "Moving the larger stones " +
                        "reveals a door behind them.",
                    SpecialDescription =
                        "The door is jammed but not " +
                        "locked. It leads somewhere.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_equipment_normal",
                    Name = "Rusted Equipment",
                    Description =
                        "Old tools and equipment, " +
                        "fused with rust. Useless.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_equipment_special",
                    Name = "Equipment with Salvageable Parts",
                    IsSpecial = true,
                    Description =
                        "Most is ruined but one piece " +
                        "is wrapped in protective cloth.",
                    SpecialDescription =
                        "The cloth preserved it well. " +
                        "A functional tool.",
                    HasItem = true,
                    ItemId = "salvaged_tool"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_wall_normal",
                    Name = "Collapsed Wall",
                    Description =
                        "A wall that gave way, " +
                        "bricks spread across the floor.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_wall_special",
                    Name = "Wall with Hidden Alcove",
                    IsSpecial = true,
                    Description =
                        "Behind the fallen bricks " +
                        "the wall has a deliberate recess.",
                    SpecialDescription =
                        "The alcove holds a wrapped " +
                        "bundle, placed there intentionally.",
                    HasItem = true,
                    ItemId = "wrapped_bundle"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_chain_normal",
                    Name = "Hanging Chain",
                    Description =
                        "A heavy chain hanging from " +
                        "the ceiling. Swings slightly.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_chain_special",
                    Name = "Chain attached to Locked Hatch",
                    IsSpecial = true,
                    Description =
                        "Following the chain up — " +
                        "it runs through a ceiling hatch.",
                    SpecialDescription =
                        "The hatch has a lock. " +
                        "The chain is the release mechanism.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "ct_crate_normal",
                    Name = "Broken Crate",
                    Description =
                        "A shattered wooden crate, " +
                        "contents scattered and rotted.",
                },
                Special = new LandmarkObject
                {
                    Id = "ct_crate_special",
                    Name = "Crate with Preserved Contents",
                    IsSpecial = true,
                    Description =
                        "One crate fell intact, " +
                        "landing upright. Still sealed.",
                    SpecialDescription =
                        "Inside — items in oilcloth, " +
                        "remarkably preserved.",
                    HasItem = true,
                    ItemId = "preserved_items"
                }
            }
        };

        // ── EXTRAVAGANT PALACE ────────────────────

        private static List<ObjectPair> MainHall()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_desk_normal",
                    Name = "Reception Desk",
                    Description =
                        "A grand reception desk, " +
                        "papers scattered across it.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_desk_special",
                    Name = "Desk with Secret Drawer",
                    IsSpecial = true,
                    Description =
                        "A hidden lever under the " +
                        "desktop edge catches your finger.",
                    SpecialDescription =
                        "A concealed drawer releases. " +
                        "A registry of names inside.",
                    HasItem = true,
                    ItemId = "name_registry"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_bookcase_normal",
                    Name = "Ornate Bookcase",
                    Description =
                        "Floor to ceiling shelves, " +
                        "books still in place but ruined.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_bookcase_special",
                    Name = "Bookcase with Hidden Door",
                    IsSpecial = true,
                    Description =
                        "One section of the bookcase " +
                        "does not collect dust on its edge.",
                    SpecialDescription =
                        "The bookcase swings inward. " +
                        "A narrow passage behind.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_displaycab_normal",
                    Name = "Display Cabinet",
                    Description =
                        "Glass-fronted cabinet, some " +
                        "panes cracked. Contents removed.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_displaycab_special",
                    Name = "Cabinet with Locked Compartment",
                    IsSpecial = true,
                    Description =
                        "The lower half of the cabinet " +
                        "is separately locked.",
                    SpecialDescription =
                        "A key from elsewhere in the " +
                        "palace might open this.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_pillar_normal",
                    Name = "Stone Pillar",
                    Description =
                        "One of four ornate pillars " +
                        "supporting the hall ceiling.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_pillar_special",
                    Name = "Pillar with Carved Symbol",
                    IsSpecial = true,
                    Description =
                        "This pillar has a symbol " +
                        "carved at eye level. Recent.",
                    SpecialDescription =
                        "The symbol matches others " +
                        "found near the city entrance.",
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_painting_normal",
                    Name = "Painting",
                    Description =
                        "A large oil painting, canvas " +
                        "warped. A landscape, barely visible.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_painting_special",
                    Name = "Painting concealing Wall Safe",
                    IsSpecial = true,
                    Description =
                        "The painting hangs unevenly, " +
                        "as if mounted over something.",
                    SpecialDescription =
                        "A wall safe behind the painting. " +
                        "The combination is unknown.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_chest_normal",
                    Name = "Decorative Chest",
                    Description =
                        "An ornate chest, lid open. " +
                        "Completely stripped of contents.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_chest_special",
                    Name = "Chest with False Bottom",
                    IsSpecial = true,
                    Description =
                        "The chest seems shallower " +
                        "on the inside than outside.",
                    SpecialDescription =
                        "A false bottom lifts away. " +
                        "Documents stored beneath.",
                    HasItem = true,
                    ItemId = "hidden_documents"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_fireplace_normal",
                    Name = "Fireplace",
                    Description =
                        "A grand fireplace, cold for " +
                        "decades. Ash and char remain.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_fireplace_special",
                    Name = "Fireplace with Hidden Cavity",
                    IsSpecial = true,
                    Description =
                        "One of the fireback panels " +
                        "is loose and hinged.",
                    SpecialDescription =
                        "Behind it — a fireproof cavity. " +
                        "Something wrapped in asbestos cloth.",
                    HasItem = true,
                    ItemId = "fireproof_package"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_clock_normal",
                    Name = "Grandfather Clock",
                    Description =
                        "A tall clock, stopped at " +
                        "3:17. The pendulum is still.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_clock_special",
                    Name = "Clock with Compartment",
                    IsSpecial = true,
                    Description =
                        "The clock face is slightly " +
                        "misaligned, as if opened before.",
                    SpecialDescription =
                        "Behind the clock face — " +
                        "a compartment with a folded note.",
                    HasItem = true,
                    ItemId = "clock_note"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_trophy_normal",
                    Name = "Trophy Cabinet",
                    Description =
                        "A glass cabinet with empty " +
                        "stands. Trophies long removed.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_trophy_special",
                    Name = "Cabinet with Missing Trophy",
                    IsSpecial = true,
                    Description =
                        "One stand has a different " +
                        "dust pattern — something was taken.",
                    SpecialDescription =
                        "A label remains: " +
                        "AWARDED TO E. VANCE — 1983.",
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_chandelier_normal",
                    Name = "Chandelier Base",
                    Description =
                        "The base mount of a fallen " +
                        "chandelier. Crystal shards below.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_chandelier_special",
                    Name = "Base with Inscription",
                    IsSpecial = true,
                    Description =
                        "The base has text engraved " +
                        "in a language you half-recognize.",
                    SpecialDescription =
                        "A prayer. Or a warning. " +
                        "The last line is scratched out.",
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_armchair_normal",
                    Name = "Armchairs",
                    Description =
                        "A pair of high-backed chairs, " +
                        "fabric moth-eaten and stiff.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_armchair_special",
                    Name = "Chair with Stuffed Envelope",
                    IsSpecial = true,
                    Description =
                        "One chair has a bulge " +
                        "beneath the seat cushion.",
                    SpecialDescription =
                        "A sealed envelope, addressed " +
                        "to no one. Contains a photograph.",
                    HasItem = true,
                    ItemId = "sealed_envelope"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "mh_rug_normal",
                    Name = "Ornate Rug",
                    Description =
                        "A large faded rug covering " +
                        "most of the hall floor.",
                },
                Special = new LandmarkObject
                {
                    Id = "mh_rug_special",
                    Name = "Rug concealing Floor Hatch",
                    IsSpecial = true,
                    Description =
                        "The rug is nailed down " +
                        "at one corner — deliberately.",
                    SpecialDescription =
                        "A floor hatch underneath, " +
                        "leading to a lower passage.",
                    HasItem = false
                }
            }
        };

        private static List<ObjectPair> Basement()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_shelving_normal",
                    Name = "Metal Shelving",
                    Description =
                        "Industrial shelving units, " +
                        "mostly bare. A few broken jars.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_shelving_special",
                    Name = "Shelving with Hidden Section",
                    IsSpecial = true,
                    Description =
                        "One unit is bolted to the wall " +
                        "differently than the others.",
                    SpecialDescription =
                        "It pivots. A small recess " +
                        "cut into the stone wall behind.",
                    HasItem = true,
                    ItemId = "wall_recess_item"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_boxes_normal",
                    Name = "Storage Boxes",
                    Description =
                        "Cardboard boxes, collapsed " +
                        "and rotted. Nothing salvageable.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_boxes_special",
                    Name = "Sealed Metal Box",
                    IsSpecial = true,
                    Description =
                        "Among the cardboard — " +
                        "a metal box with a clasp.",
                    SpecialDescription =
                        "The clasp opens. Medical " +
                        "supplies, still sterile.",
                    HasItem = true,
                    ItemId = "medical_supplies"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_furnace_normal",
                    Name = "Old Furnace",
                    Description =
                        "A large iron furnace, long " +
                        "cold. Door hanging open.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_furnace_special",
                    Name = "Furnace with Sealed Chamber",
                    IsSpecial = true,
                    Description =
                        "Inside the furnace — a second " +
                        "door, welded shut from inside.",
                    SpecialDescription =
                        "Something is behind the inner " +
                        "door. You hear nothing. That's worse.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_winerack_normal",
                    Name = "Wine Rack",
                    Description =
                        "A wooden rack, most bottles " +
                        "broken or empty.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_winerack_special",
                    Name = "Rack with Hollow Bottle",
                    IsSpecial = true,
                    Description =
                        "One bottle is unusually light " +
                        "and does not slosh when moved.",
                    SpecialDescription =
                        "The base unscrews. " +
                        "A rolled note inside.",
                    HasItem = true,
                    ItemId = "bottle_note"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_toolcab_normal",
                    Name = "Tool Cabinet",
                    Description =
                        "A metal cabinet, drawers open " +
                        "and emptied. Rust on every surface.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_toolcab_special",
                    Name = "Cabinet with Locked Drawer",
                    IsSpecial = true,
                    Description =
                        "One drawer has a padlock " +
                        "fitted. The others were left open.",
                    SpecialDescription =
                        "Someone locked this one " +
                        "for a reason.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_barrel_normal",
                    Name = "Wooden Barrel",
                    Description =
                        "A large barrel, staves " +
                        "warped. Empty and damp inside.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_barrel_special",
                    Name = "Barrel with Hidden Document",
                    IsSpecial = true,
                    Description =
                        "Reaching inside the barrel — " +
                        "something taped to the inner wall.",
                    SpecialDescription =
                        "A document in an oilskin wrap. " +
                        "Property records for the palace.",
                    HasItem = true,
                    ItemId = "property_records"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_ledger_normal",
                    Name = "Old Ledger",
                    Description =
                        "A large accounting ledger, " +
                        "pages stuck together with damp.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_ledger_special",
                    Name = "Ledger with Torn Pages",
                    IsSpecial = true,
                    Description =
                        "Most pages are torn out. " +
                        "The remaining ones are coded.",
                    SpecialDescription =
                        "The last intact entry: " +
                        "Day 0. They know.",
                    HasItem = true,
                    ItemId = "coded_ledger"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bs_safe_normal",
                    Name = "Locked Safe",
                    Description =
                        "A floor-mounted safe, " +
                        "combination dial intact.",
                },
                Special = new LandmarkObject
                {
                    Id = "bs_safe_special",
                    Name = "Reinforced Safe",
                    IsSpecial = true,
                    Description =
                        "A heavier safe than expected. " +
                        "The dial was recently used.",
                    SpecialDescription =
                        "The last combination attempt " +
                        "scratched into the paint nearby.",
                    HasItem = false
                }
            }
        };

        private static List<ObjectPair> StorageRoom()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "sr_cabinet_normal",
                    Name = "Supply Cabinet",
                    Description =
                        "A tall cabinet, doors open. " +
                        "Shelves bare.",
                },
                Special = new LandmarkObject
                {
                    Id = "sr_cabinet_special",
                    Name = "Cabinet with Restricted Section",
                    IsSpecial = true,
                    Description =
                        "One shelf is locked behind " +
                        "a separate panel.",
                    SpecialDescription =
                        "The panel gives way. " +
                        "Restricted supplies inside.",
                    HasItem = true,
                    ItemId = "restricted_supplies"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "sr_crates_normal",
                    Name = "Crate Stack",
                    Description =
                        "A stack of wooden crates, " +
                        "top ones collapsed inward.",
                },
                Special = new LandmarkObject
                {
                    Id = "sr_crates_special",
                    Name = "Crate with Concealed Bottom",
                    IsSpecial = true,
                    Description =
                        "The bottom crate of the stack " +
                        "is heavier than it looks.",
                    SpecialDescription =
                        "A false floor in the crate. " +
                        "Items packed tightly underneath.",
                    HasItem = true,
                    ItemId = "crate_cache"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "sr_rack_normal",
                    Name = "Wall Rack",
                    Description =
                        "A rack mounted to the wall, " +
                        "hooks empty.",
                },
                Special = new LandmarkObject
                {
                    Id = "sr_rack_special",
                    Name = "Rack with Hidden Package",
                    IsSpecial = true,
                    Description =
                        "Behind the rack, flush against " +
                        "the wall — a wrapped package.",
                    SpecialDescription =
                        "Deliberately hidden here. " +
                        "Contents carefully preserved.",
                    HasItem = true,
                    ItemId = "hidden_package"
                }
            }
        };

        private static List<ObjectPair> Kitchen()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "kt_counter_normal",
                    Name = "Kitchen Counter",
                    Description =
                        "Long stone counters, stained " +
                        "and cracked. Utensils remain.",
                },
                Special = new LandmarkObject
                {
                    Id = "kt_counter_special",
                    Name = "Counter with Hidden Storage",
                    IsSpecial = true,
                    Description =
                        "A section of counter has a " +
                        "recessed panel with a latch.",
                    SpecialDescription =
                        "A small hidden storage space. " +
                        "Dry goods, still sealed.",
                    HasItem = true,
                    ItemId = "dry_goods"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "kt_pantry_normal",
                    Name = "Pantry Cabinet",
                    Description =
                        "A large pantry, doors open. " +
                        "Shelves empty, jars smashed.",
                },
                Special = new LandmarkObject
                {
                    Id = "kt_pantry_special",
                    Name = "Pantry with False Back",
                    IsSpecial = true,
                    Description =
                        "The back wall of the pantry " +
                        "sounds hollow when tapped.",
                    SpecialDescription =
                        "A panel removes. A small " +
                        "space behind, items inside.",
                    HasItem = true,
                    ItemId = "pantry_cache"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "kt_stove_normal",
                    Name = "Cooking Stove",
                    Description =
                        "A large iron stove, cold. " +
                        "Grease and ash, nothing more.",
                },
                Special = new LandmarkObject
                {
                    Id = "kt_stove_special",
                    Name = "Stove with Sealed Compartment",
                    IsSpecial = true,
                    Description =
                        "The warming compartment above " +
                        "the oven is latched shut.",
                    SpecialDescription =
                        "Inside the warming compartment — " +
                        "items stored to keep dry.",
                    HasItem = true,
                    ItemId = "warming_items"
                }
            }
        };

        private static List<ObjectPair> Bedroom()
            => new()
        {
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bd_bed_normal",
                    Name = "Bed",
                    Description =
                        "A large bed, sheets grey " +
                        "with dust. The frame still intact.",
                },
                Special = new LandmarkObject
                {
                    Id = "bd_bed_special",
                    Name = "Bed with Items Underneath",
                    IsSpecial = true,
                    Description =
                        "Something was pushed " +
                        "deliberately under the bed.",
                    SpecialDescription =
                        "A bag, packed as if for " +
                        "a quick departure. Never used.",
                    HasItem = true,
                    ItemId = "packed_bag"
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bd_wardrobe_normal",
                    Name = "Wardrobe",
                    Description =
                        "A tall wardrobe, doors open. " +
                        "Clothes still hanging inside.",
                },
                Special = new LandmarkObject
                {
                    Id = "bd_wardrobe_special",
                    Name = "Wardrobe with Hidden Passage",
                    IsSpecial = true,
                    Description =
                        "The back of the wardrobe " +
                        "shifts when pressed at the corner.",
                    SpecialDescription =
                        "A passage behind the wardrobe. " +
                        "Narrow but walkable.",
                    HasItem = false
                }
            },
            new ObjectPair
            {
                Normal = new LandmarkObject
                {
                    Id = "bd_nightstand_normal",
                    Name = "Nightstand",
                    Description =
                        "A small bedside table, " +
                        "drawer open and empty.",
                },
                Special = new LandmarkObject
                {
                    Id = "bd_nightstand_special",
                    Name = "Nightstand with Locked Drawer",
                    IsSpecial = true,
                    Description =
                        "The bottom drawer is locked. " +
                        "A personal lock, not a standard one.",
                    SpecialDescription =
                        "This one was locked by " +
                        "whoever slept here last.",
                    HasItem = false
                }
            }
        };
    }
}