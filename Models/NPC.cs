using System.Collections.Generic;

namespace LostInAForgottenCity.Models
{
    public class NPC
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<DialogueLine> Dialogues { get; set; } = new();
    }

    public class DialogueLine
    {
        public string? Text { get; set; }
        public int SanityEffect { get; set; } = 0;
        public int DangerEffect { get; set; } = 0;
    }
}