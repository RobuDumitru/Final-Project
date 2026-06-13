using System.Collections.Generic;

namespace LostInAForgottenCity.Engine
{
    // ── A single object in a landmark ────────────
    public class LandmarkObject
    {
        // Identity
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";

        // Is this the special version?
        public bool IsSpecial { get; set; } = false;

        // Has the player examined this object?
        public bool IsExamined { get; set; } = false;

        // What the player sees when examining
        public string Description { get; set; } = "";

        // Extra description shown only if special
        public string SpecialDescription { get; set; } = "";

        // Does this object contain an item?
        public bool HasItem { get; set; } = false;
        public string? ItemId { get; set; } = null;
    }

    // ── A pair: normal + special version ─────────
    // Only one will be selected during generation
    public class ObjectPair
    {
        public LandmarkObject Normal { get; set; } = new();
        public LandmarkObject Special { get; set; } = new();
    }

    // ── The generated object list for a landmark ──
    public class LandmarkObjectPool
    {
        public string LandmarkId { get; set; } = "";
        public List<LandmarkObject> Objects { get; set; }
            = new();

        // Get object by id
        public LandmarkObject? GetObject(string id)
            => Objects.Find(o => o.Id == id);

        // Get all unexamined objects
        public List<LandmarkObject> GetUnexamined()
            => Objects.FindAll(o => !o.IsExamined);

        // Did player find anything special?
        public bool HasSpecialObject
            => Objects.Exists(o => o.IsSpecial);
    }
}