using System.Collections.Generic;

namespace LostInAForgottenCity.Engine
{
    public class DialogueLine
    {
        public string Speaker { get; set; } = "";
        public string Text { get; set; } = "";
        public bool IsNarration { get; set; } = false;
        public bool IsGameline { get; set; } = false;
    }

    public class DialogueChoice
    {
        public string Text { get; set; } = "";
        public string NextSceneId { get; set; } = "";
    }

    public class DialogueScene
    {
        public string Id { get; set; } = "";
        public List<DialogueLine> Lines { get; set; } = new();
        public List<DialogueChoice> Choices { get; set; } = new();
        public string AutoNextId { get; set; } = "";
    }

    public static class DialogueData
    {
        public static Dictionary<string, DialogueScene>
            GetTutorialDialogue()
        {
            return new Dictionary<string, DialogueScene>
            {
                ["fortuneteller_arrival"] = new DialogueScene
                {
                    Id = "fortuneteller_arrival",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You arrive at the specified location.", IsNarration = true },
                        new() { Text = "For some reason, the higher-ups were insistent that you meet her. They claimed she would be helpful.", IsNarration = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Get closer to the entrance.", NextSceneId = "ft_entrance" },
                        new() { Text = "Take a look around.", NextSceneId = "ft_look_around" }
                    }
                },

                ["ft_entrance"] = new DialogueScene
                {
                    Id = "ft_entrance",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You stop in front of the entrance.", IsNarration = true },
                        new() { Text = "There is no door. Only thick curtains hanging from the frame.", IsNarration = true },
                        new() { Text = "This is not what you expected when they sent you here.", IsNarration = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Go inside.", NextSceneId = "ft_go_inside" },
                        new() { Text = "Knock on the frame.", NextSceneId = "ft_knock" }
                    }
                },

                ["ft_look_around"] = new DialogueScene
                {
                    Id = "ft_look_around",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You decide to look around before starting your assignment.", IsNarration = true },
                        new() { Text = "The area is filled with tents and RVs. It seems people here dislike staying in one place for too long.", IsNarration = true },
                        new() { Text = "You are not sure whether that is caution or paranoia.", IsNarration = true },
                        new() { Text = "The person you were sent to meet stays in a tent as well, though hers is much larger than the others. Its fabric carries a faint purplish-pink tint.", IsNarration = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Return to your objective.", NextSceneId = "ft_return_objective" },
                        new() { Text = "Find someone outside.", NextSceneId = "ft_find_someone" }
                    }
                },

                ["ft_return_objective"] = new DialogueScene
                {
                    Id = "ft_return_objective",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You decide not to waste any more time and return to your original purpose.", IsNarration = true },
                        new() { Text = "Fortunately, you did not arrive late.", IsNarration = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Go inside.", NextSceneId = "ft_go_inside" },
                        new() { Text = "Knock on the frame.", NextSceneId = "ft_knock" }
                    }
                },

                ["ft_find_someone"] = new DialogueScene
                {
                    Id = "ft_find_someone",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You decide to gather more information about the person you were sent to meet.", IsNarration = true },
                        new() { Text = "Perhaps the locals know something about her.", IsNarration = true },
                        new() { Text = "Unfortunately, you fail to find anyone nearby.", IsNarration = true },
                        new() { Text = "Not wanting to waste more time, you return to the tent. Thankfully, only a few minutes have passed.", IsNarration = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Go inside.", NextSceneId = "ft_go_inside" },
                        new() { Text = "Knock on the frame.", NextSceneId = "ft_knock" }
                    }
                },

                ["ft_go_inside"] = new DialogueScene
                {
                    Id = "ft_go_inside",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You enter without announcing yourself and head toward the only room with lights.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Entering without permission and not even bothering to announce yourself… quite rude, don't you think?" }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Apologize.", NextSceneId = "ft_apologize" },
                        new() { Text = "Introduce yourself.", NextSceneId = "ft_introduce" }
                    }
                },

                ["ft_knock"] = new DialogueScene
                {
                    Id = "ft_knock",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "Since there is no door, you decide to knock on the frame of the tent.", IsNarration = true },
                        new() { Text = "After all, it is someone else's home.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "My, my… a new visitor. And a polite one at that." },
                        new() { Speaker = "Fortuneteller", Text = "Since you are being so considerate, you may come in. I will be waiting in the room with the lights." },
                        new() { Text = "She sounds to be in a good mood.", IsNarration = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Enter slowly.", NextSceneId = "ft_enter_slow" },
                        new() { Text = "Enter quickly.", NextSceneId = "ft_enter_quick" }
                    }
                },

                ["ft_apologize"] = new DialogueScene
                {
                    Id = "ft_apologize",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You pause for a moment and realize how inconsiderate your actions were.", IsNarration = true },
                        new() { Speaker = "You", Text = "I apologize for the intrusion. I have a lot on my mind at the moment and did not think before entering." },
                        new() { Speaker = "Fortuneteller", Text = "You sound sincere, so I will not dwell on it further." },
                        new() { Speaker = "Fortuneteller", Text = "After all, you came here for a reason, didn't you?" }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Get to business.", NextSceneId = "ft_business" }
                    }
                },

                ["ft_introduce"] = new DialogueScene
                {
                    Id = "ft_introduce",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You begin introducing yourself and explaining why you came here, but she interrupts you before you can finish.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "I already know why you are here." },
                        new() { Speaker = "Fortuneteller", Text = "And since you are apparently not planning to apologize, I suppose we should begin our conversation." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Get to business.", NextSceneId = "ft_business" }
                    }
                },

                ["ft_enter_slow"] = new DialogueScene
                {
                    Id = "ft_enter_slow",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "As you approach the lit room, your attention drifts across the tent.", IsNarration = true },
                        new() { Text = "You notice all kinds of fortune-telling tools, including several astronomical instruments.", IsNarration = true },
                        new() { Text = "Judging by the condition of the equipment, she has likely been doing this for a very long time.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "I see you admiring my workspace." },
                        new() { Speaker = "Fortuneteller", Text = "It is rare to meet people interested in my craft." },
                        new() { Speaker = "Fortuneteller", Text = "Unfortunately, we do not have time for such discussions today." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Get to business.", NextSceneId = "ft_business" }
                    }
                },

                ["ft_enter_quick"] = new DialogueScene
                {
                    Id = "ft_enter_quick",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You ignore your surroundings and head directly toward the lit room.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "You seem to be in quite a hurry." },
                        new() { Speaker = "Fortuneteller", Text = "There is no need to worry. We have enough time." },
                        new() { Speaker = "Fortuneteller", Text = "Still, I would rather not inconvenience you further, so let us begin." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Get to business.", NextSceneId = "ft_business" }
                    }
                },

                ["ft_business"] = new DialogueScene
                {
                    Id = "ft_business",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "Your conversation finally reaches the subject that matters most.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Because you are not the first person they have sent here." },
                        new() { Speaker = "Fortuneteller", Text = "And judging by your attitude, I doubt you will be the last." },
                        new() { Speaker = "Fortuneteller", Text = "My usefulness has already been proven. They would not allow one of their most reliable sources to disappear because of someone's ignorance." },
                        new() { Speaker = "Fortuneteller", Text = "Still, there are limits to what I can tell you." },
                        new() { Speaker = "Fortuneteller", Text = "If you want clearer answers, you should seek the Founder of your Organization." },
                        new() { Speaker = "Fortuneteller", Text = "Though… judging by that look, you were never told who he is." },
                        new() { Speaker = "Fortuneteller", Text = "I suppose that is to be expected, considering the nature of your Organization." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "What do you offer to be valued this much?", NextSceneId = "ft_what_offer" },
                        new() { Text = "How much do you know about our work?", NextSceneId = "ft_how_much" }
                    }
                },

                ["ft_what_offer"] = new DialogueScene
                {
                    Id = "ft_what_offer",
                    Lines = new List<DialogueLine>
                    {
                        new() { Speaker = "Fortuneteller", Text = "Considering why you were sent here, I imagine your interest concerns Illumination." },
                        new() { Speaker = "Fortuneteller", Text = "My role is simple." },
                        new() { Speaker = "Fortuneteller", Text = "I help people understand what awaits them there." },
                        new() { Text = "Your blood runs cold for a brief moment.", IsNarration = true },
                        new() { Text = "She notices immediately.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "So they did brief you on the subject." },
                        new() { Speaker = "Fortuneteller", Text = "Good. That means they still consider you valuable." },
                        new() { Speaker = "Fortuneteller", Text = "Organizations like yours do not waste elite operatives carelessly. Losses of that caliber are… inconvenient." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Continue.", NextSceneId = "ft_continue" }
                    }
                },

                ["ft_how_much"] = new DialogueScene
                {
                    Id = "ft_how_much",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "A faint trace of amusement crosses her face before she answers carefully.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Oh, I assure you… I know more than enough." },
                        new() { Speaker = "Fortuneteller", Text = "Organizations such as yours rarely rise to prominence without assistance." },
                        new() { Text = "You give her a skeptical look.", IsNarration = true },
                        new() { Text = "She notices.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "I will neither confirm nor deny your suspicions." },
                        new() { Speaker = "Fortuneteller", Text = "If you truly wish to understand more, you would be better off searching for the answers yourself." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Continue.", NextSceneId = "ft_continue" }
                    }
                },

                ["ft_continue"] = new DialogueScene
                {
                    Id = "ft_continue",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "Your conversation finally reaches the subject that matters most.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "The city has already claimed many lives." },
                        new() { Speaker = "Fortuneteller", Text = "Some vanished without a trace." },
                        new() { Speaker = "Fortuneteller", Text = "Others returned… changed beyond recognition." },
                        new() { Speaker = "Fortuneteller", Text = "And the few who escaped intact refuse to ever go back." },
                        new() { Text = "She watches you carefully.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Though judging by your reaction, you were already warned about such things." },
                        new() { Text = "She leans back slightly.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "So tell me." },
                        new() { Speaker = "Fortuneteller", Text = "Would you prefer a simple reminder…" },
                        new() { Speaker = "Fortuneteller", Text = "Or are you searching for something more specific?" },
                        new() { Text = "You can now choose what kind of guidance you want.", IsGameline = true },
                        new() { Text = "The Introduction Tutorial is recommended for new players.", IsGameline = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Better be prepared.", NextSceneId = "tutorial_introduction_start" },
                        new() { Text = "I want to focus on something.", NextSceneId = "tutorial_scenarios_check" }
                    }
                },

                ["tutorial_introduction_start"] = new DialogueScene
                {
                    Id = "tutorial_introduction_start",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "She seems pleased by your answer.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Caution is good." },
                        new() { Speaker = "Fortuneteller", Text = "If you manage to hold onto that mentality, you may survive longer than most." },
                        new() { Text = "A faint smile appears on her face.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Fortunately for you, I have the perfect vision in mind." },
                        new() { Text = "She places her hand against the crystal ball and whispers something under her breath in a language you do not recognize.", IsNarration = true },
                        new() { Text = "The surface of the crystal slowly begins to glow.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "This man was talented." },
                        new() { Speaker = "Fortuneteller", Text = "Far more talented than many who entered that city." },
                        new() { Speaker = "Fortuneteller", Text = "He survived for quite some time." },
                        new() { Text = "Her expression darkens slightly.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Unfortunately… surviving is not the same as understanding." },
                        new() { Speaker = "Fortuneteller", Text = "In the end, the city claimed him as well." },
                        new() { Text = "The light inside the crystal shifts unnaturally.", IsNarration = true },
                        new() { Text = "For a brief moment, you think you see a human silhouette staring back at you from within the glow.", IsNarration = true },
                        new() { Text = "Then it disappears.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Now." },
                        new() { Speaker = "Fortuneteller", Text = "Look closely." },
                        new() { Speaker = "Fortuneteller", Text = "And witness the struggles of someone who learned too little too late." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Proceed.", NextSceneId = "intro_tutorial_begin" }
                    }
                },

                ["tutorial_scenarios_check"] = new DialogueScene
                {
                    Id = "tutorial_scenarios_check",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                    // Handled in code — checks if intro tutorial done
                },

                ["tutorial_scenarios_locked"] = new DialogueScene
                {
                    Id = "tutorial_scenarios_locked",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "You attempt to say something, but she interrupts you before you can finish.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "No." },
                        new() { Speaker = "Fortuneteller", Text = "You are not prepared to witness those visions yet." },
                        new() { Speaker = "Fortuneteller", Text = "A person who lacks understanding will only misinterpret what they see." },
                        new() { Text = "Her gaze narrows slightly.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Return once you have gained a better understanding of your situation." }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Go back.", NextSceneId = "tutorial_go_back" }
                    }
                },

                ["tutorial_scenarios_unlocked"] = new DialogueScene
                {
                    Id = "tutorial_scenarios_unlocked",
                    Lines = new List<DialogueLine>
                    {
                        new() { Text = "As you make your choice, the crystal ball slowly begins to glow once more.", IsNarration = true },
                        new() { Text = "Pale light moves beneath its surface like drifting fog.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Very well." },
                        new() { Speaker = "Fortuneteller", Text = "Then choose the vision that interests you most." },
                        new() { Text = "She lightly gestures toward the crystal ball.", IsNarration = true },
                        new() { Speaker = "Fortuneteller", Text = "Slide your hand across its surface and focus on what you wish to understand." },
                        new() { Text = "Tutorial Scenarios Unlocked.", IsGameline = true },
                        new() { Text = "Opening Scenario Selection.", IsGameline = true }
                    },
                    Choices = new List<DialogueChoice>
                    {
                        new() { Text = "Decide.", NextSceneId = "scenario_selection" }
                    }
                },

                ["tutorial_go_back"] = new DialogueScene
                {
                    Id = "tutorial_go_back",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                },

                ["intro_tutorial_begin"] = new DialogueScene
                {
                    Id = "intro_tutorial_begin",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                },

                ["scenario_selection"] = new DialogueScene
                {
                    Id = "scenario_selection",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                },

                ["fortuneteller_return"] = new DialogueScene
                {
                    Id = "fortuneteller_return",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You return to the tent.",
            IsNarration = true
        },
        new() {
            Text = "She is already looking at you " +
                   "as you enter.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "You are back."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "What is it that you need?"
        },
        new() {
            Text = "The Introduction Tutorial is recommended " +
                   "for new players.",
            IsGameline = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Better be prepared.",
            NextSceneId = "tutorial_introduction_start"
        },
        new() {
            Text = "I want to focus on something.",
            NextSceneId = "tutorial_scenarios_check"
        }
    }
                },

                ["intro_tutorial_begin"] = new DialogueScene
                {
                    Id = "intro_tutorial_begin",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "A hiker is traveling through the mountains.",
            IsNarration = true
        },
        new() {
            Text = "He has been doing this for days, " +
                   "and he is slowly running out of supplies.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Why didn't I bring more supplies? " +
                   "At this rate, I won't even be able to move " +
                   "my legs in a day. I have to find somewhere " +
                   "to rest and restock, or I will become " +
                   "the next missing corpse."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Climb higher.",
            NextSceneId = "intro_climb_higher"
        },
        new() {
            Text = "Look for a sign.",
            NextSceneId = "intro_look_sign"
        }
    }
                },

                ["intro_climb_higher"] = new DialogueScene
                {
                    Id = "intro_climb_higher",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "The hiker climbs higher to see his surroundings.",
            IsNarration = true
        },
        new() {
            Text = "In the distance he sees the silhouettes of buildings.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "There is a city there, and it looks like " +
                   "it's covered in fog. This will make it harder " +
                   "to find, but there should be people nearby. " +
                   "I just have to find them."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Head towards the city.",
            NextSceneId = "intro_towards_city"
        },
        new() {
            Text = "Look closer.",
            NextSceneId = "intro_look_closer"
        }
    }
                },

                ["intro_look_closer"] = new DialogueScene
                {
                    Id = "intro_look_closer",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "He tries to look a bit harder at the " +
                   "silhouette of the city.",
            IsNarration = true
        },
        new() {
            Text = "But suddenly he finds something weird about it, " +
                   "like it was frozen in time.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Weird, something about that city doesn't feel " +
                   "right. Maybe it's because of the fog. " +
                   "I shouldn't dwell on it any longer."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Head towards the city.",
            NextSceneId = "intro_towards_city"
        }
    }
                },

                ["intro_look_sign"] = new DialogueScene
                {
                    Id = "intro_look_sign",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "The hiker starts looking side to side " +
                   "while walking on the dirt path.",
            IsNarration = true
        },
        new() {
            Text = "Suddenly he sees a sign covered in vines.",
            IsNarration = true
        },
        new() {
            Text = "It points to a city and shows the distance, " +
                   "but the name of the city is no longer visible " +
                   "due to long time without maintenance.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Good, there is a city nearby, and there should " +
                   "be more signs that I can follow. If everything " +
                   "goes well I should reach it in half a day."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Head towards the city.",
            NextSceneId = "intro_towards_city"
        },
        new() {
            Text = "Examine the base of the sign.",
            NextSceneId = "intro_examine_sign"
        }
    }
                },

                ["intro_examine_sign"] = new DialogueScene
                {
                    Id = "intro_examine_sign",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "When he was about to leave, he suddenly " +
                   "saw a note held down by a rock.",
            IsNarration = true
        },
        new() {
            Text = "In it he read: \"I may slowly be losing my mind, " +
                   "but these things seem so real, and I am not " +
                   "planning to approach them, " +
                   "and neither should you.\"",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Is he talking about a wild animal? " +
                   "I don't have time to think about it."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Head towards the city.",
            NextSceneId = "intro_towards_city"
        }
    }
                },

                ["intro_towards_city"] = new DialogueScene
                {
                    Id = "intro_towards_city",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "As he goes on his way, eventually he " +
                   "sees the city in the distance.",
            IsNarration = true
        },
        new() {
            Text = "But before reaching it, he sees a pit, " +
                   "and the only path forward is a suspended bridge.",
            IsNarration = true
        },
        new() {
            Text = "The bridge is old and slowly crumbling.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Damn, I can't reach the city like this. " +
                   "I should have guessed that not many people " +
                   "go through here. But if I try to turn back, " +
                   "I don't think I will get out of this. " +
                   "What should I do?"
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Cross the bridge.",
            NextSceneId = "intro_cross_bridge"
        },
        new() {
            Text = "Leave your backpack behind.",
            NextSceneId = "intro_leave_backpack"
        },
        new() {
            Text = "Try finding another way around.",
            NextSceneId = "intro_find_another_way"
        }
    }
                },

                ["intro_cross_bridge"] = new DialogueScene
                {
                    Id = "intro_cross_bridge",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "As the hiker crosses the bridge, he " +
                   "suddenly hears a ripping sound.",
            IsNarration = true
        },
        new() {
            Text = "The strain on the ropes and the planks " +
                   "is too high, and they snap under the pressure.",
            IsNarration = true
        },
        new() {
            Text = "The hiker was too far from any side " +
                   "and he fallen into the pit.",
            IsNarration = true
        },
        new() {
            Text = "His journey ends here.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Understood.",
            NextSceneId = "intro_death_bridge"
        }
    }
                },

                ["intro_death_bridge"] = new DialogueScene
                {
                    Id = "intro_death_bridge",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                },

                ["intro_find_another_way"] = new DialogueScene
                {
                    Id = "intro_find_another_way",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "He decides it is too dangerous to cross " +
                   "the bridge, and turning back would be a " +
                   "death sentence. So without any choice " +
                   "he starts walking around it.",
            IsNarration = true
        },
        new() {
            Text = "As he walks, time passes and passes.",
            IsNarration = true
        },
        new() {
            Text = "Eventually he managed to bypass the pit, " +
                   "but at this point it no longer mattered. " +
                   "He was too exhausted to keep moving.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Why was I so stupid. Now I can't do anything. " +
                   "Maybe someone will find me."
        },
        new() {
            Text = "But nobody came.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Understood.",
            NextSceneId = "intro_death_exhaustion"
        }
    }
                },

                ["intro_death_exhaustion"] = new DialogueScene
                {
                    Id = "intro_death_exhaustion",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                },

                ["intro_leave_backpack"] = new DialogueScene
                {
                    Id = "intro_leave_backpack",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "The hiker decides to risk the crossing " +
                   "and puts down all his equipment.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "It hurts to leave my stuff behind but " +
                   "survival is more important at the moment. " +
                   "It won't be of any use to a dead man. " +
                   "Besides, I will come back for it later."
        },
        new() {
            Text = "As he crosses, the bridge cracks and " +
                   "stretches but still holds.",
            IsNarration = true
        },
        new() {
            Text = "When he reaches the other side he takes " +
                   "a big sigh of relief. " +
                   "Now he is closer to his goal.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Continue on your way.",
            NextSceneId = "intro_continue_path"
        }
    }
                },

                ["intro_continue_path"] = new DialogueScene
                {
                    Id = "intro_continue_path",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "After the bridge, the hiker already " +
                   "started descending down the path " +
                   "and out of the mountains.",
            IsNarration = true
        },
        new() {
            Text = "On the way he finds edible roots.",
            IsNarration = true
        },
        new() {
            Speaker = "Hiker",
            Text = "Not the best meal but it will keep " +
                   "me going for another few days."
        },
        new() {
            Text = "Eventually he reaches the edge " +
                   "of the mountains.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Confirm.",
            NextSceneId = "intro_mountain_edge"
        }
    }
                },

                ["intro_mountain_edge"] = new DialogueScene
                {
                    Id = "intro_mountain_edge",
                    Lines = new List<DialogueLine>(),
                    Choices = new List<DialogueChoice>()
                },
            };
        }
    }
}