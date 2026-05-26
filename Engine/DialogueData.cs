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
                // ── Fortuneteller intro ──────────

                ["fortuneteller_arrival"] = new DialogueScene
                {
                    Id = "fortuneteller_arrival",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You arrive at the specified location.",
            IsNarration = true
        },
        new() {
            Text = "For some reason, the higher-ups were " +
                   "insistent that you meet her. " +
                   "They claimed she would be helpful.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Get closer to the entrance.",
            NextSceneId = "ft_entrance"
        },
        new() {
            Text = "Take a look around.",
            NextSceneId = "ft_look_around"
        }
    }
                },

                ["ft_entrance"] = new DialogueScene
                {
                    Id = "ft_entrance",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You stop in front of the entrance.",
            IsNarration = true
        },
        new() {
            Text = "There is no door. " +
                   "Only thick curtains hanging from the frame.",
            IsNarration = true
        },
        new() {
            Text = "This is not what you expected " +
                   "when they sent you here.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Go inside.",
            NextSceneId = "ft_go_inside"
        },
        new() {
            Text = "Knock on the frame.",
            NextSceneId = "ft_knock"
        }
    }
                },

                ["ft_look_around"] = new DialogueScene
                {
                    Id = "ft_look_around",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You decide to look around " +
                   "before starting your assignment.",
            IsNarration = true
        },
        new() {
            Text = "The area is filled with tents and RVs. " +
                   "It seems people here dislike staying " +
                   "in one place for too long.",
            IsNarration = true
        },
        new() {
            Text = "You are not sure whether that " +
                   "is caution or paranoia.",
            IsNarration = true
        },
        new() {
            Text = "The person you were sent to meet " +
                   "stays in a tent as well, though hers " +
                   "is much larger than the others. " +
                   "Its fabric carries a faint purplish-pink tint.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Return to your objective.",
            NextSceneId = "ft_return_objective"
        },
        new() {
            Text = "Find someone outside.",
            NextSceneId = "ft_find_someone"
        }
    }
                },

                ["ft_return_objective"] = new DialogueScene
                {
                    Id = "ft_return_objective",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You decide not to waste any more time " +
                   "and return to your original purpose.",
            IsNarration = true
        },
        new() {
            Text = "Fortunately, you did not arrive late.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Go inside.",
            NextSceneId = "ft_go_inside"
        },
        new() {
            Text = "Knock on the frame.",
            NextSceneId = "ft_knock"
        }
    }
                },

                ["ft_find_someone"] = new DialogueScene
                {
                    Id = "ft_find_someone",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You decide to gather more information " +
                   "about the person you were sent to meet.",
            IsNarration = true
        },
        new() {
            Text = "Perhaps the locals know something about her.",
            IsNarration = true
        },
        new() {
            Text = "Unfortunately, you fail to find anyone nearby.",
            IsNarration = true
        },
        new() {
            Text = "Not wanting to waste more time, " +
                   "you return to the tent. " +
                   "Thankfully, only a few minutes have passed.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Go inside.",
            NextSceneId = "ft_go_inside"
        },
        new() {
            Text = "Knock on the frame.",
            NextSceneId = "ft_knock"
        }
    }
                },

                ["ft_go_inside"] = new DialogueScene
                {
                    Id = "ft_go_inside",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You enter without announcing yourself " +
                   "and head toward the only room with lights.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Entering without permission and not even " +
                   "bothering to announce yourself… " +
                   "quite rude, don't you think?"
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Apologize.",
            NextSceneId = "ft_apologize"
        },
        new() {
            Text = "Introduce yourself.",
            NextSceneId = "ft_introduce"
        }
    }
                },

                ["ft_knock"] = new DialogueScene
                {
                    Id = "ft_knock",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "Since there is no door, you decide to " +
                   "knock on the frame of the tent.",
            IsNarration = true
        },
        new() {
            Text = "After all, it is someone else's home.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "My, my… a new visitor. " +
                   "And a polite one at that."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Since you are being so considerate, " +
                   "you may come in. " +
                   "I will be waiting in the room with the lights."
        },
        new() {
            Text = "She sounds to be in a good mood.",
            IsNarration = true
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Enter slowly.",
            NextSceneId = "ft_enter_slow"
        },
        new() {
            Text = "Enter quickly.",
            NextSceneId = "ft_enter_quick"
        }
    }
                },

                ["ft_apologize"] = new DialogueScene
                {
                    Id = "ft_apologize",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You pause for a moment and realize " +
                   "how inconsiderate your actions were.",
            IsNarration = true
        },
        new() {
            Speaker = "You",
            Text = "I apologize for the intrusion. " +
                   "I have a lot on my mind at the moment " +
                   "and did not think before entering."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "You sound sincere, so I will not " +
                   "dwell on it further."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "After all, you came here for a reason, " +
                   "didn't you?"
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Get to business.",
            NextSceneId = "ft_business"
        }
    }
                },

                ["ft_introduce"] = new DialogueScene
                {
                    Id = "ft_introduce",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You begin introducing yourself and " +
                   "explaining why you came here, but she " +
                   "interrupts you before you can finish.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "I already know why you are here."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "And since you are apparently not planning " +
                   "to apologize, I suppose we should begin " +
                   "our conversation."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Get to business.",
            NextSceneId = "ft_business"
        }
    }
                },

                ["ft_enter_slow"] = new DialogueScene
                {
                    Id = "ft_enter_slow",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "As you approach the lit room, " +
                   "your attention drifts across the tent.",
            IsNarration = true
        },
        new() {
            Text = "You notice all kinds of fortune-telling tools, " +
                   "including several astronomical instruments.",
            IsNarration = true
        },
        new() {
            Text = "Judging by the condition of the equipment, " +
                   "she has likely been doing this " +
                   "for a very long time.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "I see you admiring my workspace."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "It is rare to meet people interested in my craft."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Unfortunately, we do not have time " +
                   "for such discussions today."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Get to business.",
            NextSceneId = "ft_business"
        }
    }
                },

                ["ft_enter_quick"] = new DialogueScene
                {
                    Id = "ft_enter_quick",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "You ignore your surroundings and head " +
                   "directly toward the lit room.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "You seem to be in quite a hurry."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "There is no need to worry. " +
                   "We have enough time."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Still, I would rather not inconvenience " +
                   "you further, so let us begin."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Get to business.",
            NextSceneId = "ft_business"
        }
    }
                },

                ["ft_business"] = new DialogueScene
                {
                    Id = "ft_business",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "Your conversation finally reaches " +
                   "the subject that matters most.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Because you are not the first person " +
                   "they have sent here."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "And judging by your attitude, " +
                   "I doubt you will be the last."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "My usefulness has already been proven. " +
                   "They would not allow one of their most " +
                   "reliable sources to disappear because " +
                   "of someone's ignorance."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Still, there are limits to what I can tell you."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "If you want clearer answers, " +
                   "you should seek the Founder of your Organization."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Though… judging by that look, " +
                   "you were never told who he is."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "I suppose that is to be expected, " +
                   "considering the nature of your Organization."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "What do you offer to be valued this much?",
            NextSceneId = "ft_what_offer"
        },
        new() {
            Text = "How much do you know about our work?",
            NextSceneId = "ft_how_much"
        }
    }
                },

                ["ft_what_offer"] = new DialogueScene
                {
                    Id = "ft_what_offer",
                    Lines = new List<DialogueLine>
    {
        new() {
            Speaker = "Fortuneteller",
            Text = "Considering why you were sent here, " +
                   "I imagine your interest concerns Illumination."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "My role is simple."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "I help people understand " +
                   "what awaits them there."
        },
        new() {
            Text = "Your blood runs cold for a brief moment.",
            IsNarration = true
        },
        new() {
            Text = "She notices immediately.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "So they did brief you on the subject."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Good. That means they still " +
                   "consider you valuable."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Organizations like yours do not waste " +
                   "elite operatives carelessly. " +
                   "Losses of that caliber are… inconvenient."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Continue.",
            NextSceneId = "ft_continue"
        }
    }
                },

                ["ft_how_much"] = new DialogueScene
                {
                    Id = "ft_how_much",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "A faint trace of amusement crosses " +
                   "her face before she answers carefully.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Oh, I assure you… I know more than enough."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Organizations such as yours rarely rise " +
                   "to prominence without assistance."
        },
        new() {
            Text = "You give her a skeptical look.",
            IsNarration = true
        },
        new() {
            Text = "She notices.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "I will neither confirm nor deny your suspicions."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "If you truly wish to understand more, " +
                   "you would be better off searching " +
                   "for the answers yourself."
        }
    },
                    Choices = new List<DialogueChoice>
    {
        new() {
            Text = "Continue.",
            NextSceneId = "ft_continue"
        }
    }
                },

                ["ft_continue"] = new DialogueScene
                {
                    Id = "ft_continue",
                    Lines = new List<DialogueLine>
    {
        new() {
            Text = "Your conversation finally reaches " +
                   "the subject that matters most.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "The city has already claimed many lives."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Some vanished without a trace."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Others returned… changed beyond recognition."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "And the few who escaped intact refuse " +
                   "to ever go back."
        },
        new() {
            Text = "She watches you carefully.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Though judging by your reaction, " +
                   "you were already warned about such things."
        },
        new() {
            Text = "She leans back slightly.",
            IsNarration = true
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "So tell me."
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Would you prefer a simple reminder…"
        },
        new() {
            Speaker = "Fortuneteller",
            Text = "Or are you searching for something more specific?"
        },
        new() {
            Text = "You can now choose what kind of " +
                   "guidance you want.",
            IsGameline = true
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
            NextSceneId = "tutorial_scenarios_start"
        }
    }
                },
            };
        }
    }
}