using Newtonsoft.Json;

namespace PokemonApp.Models;

public class PokemonResponseModel
{
    public string Name { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public Sprites Sprites { get; set; }
    public List<AbilityEntry>? Abilities { get; set; }
    public List<TypeEntry>? Types { get; set; }
    public string Sprite => Sprites?.FrontDefault;
}

// ------------------------- Nested Classes -------------------------

public class Sprites
{
    [JsonProperty("front_default")]
    public string FrontDefault { get; set; }
}

public class AbilityEntry
{
    public Ability Ability { get; set; }

    [JsonProperty("is_hidden")]
    public bool IsHidden { get; set; }

    public int Slot { get; set; }
}

public class Ability
{
    public string Name { get; set; }
    public string Url { get; set; }
}

public class TypeEntry
{
    public Type Type { get; set; }
    public int Slot { get; set; }
}

public class Type
{
    public string Name { get; set; }
    public string Url { get; set; }
}