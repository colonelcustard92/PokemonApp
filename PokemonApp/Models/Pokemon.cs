namespace PokemonApp.Models;

public class Pokemon
{
    public string Name { get; set; }
    public string Sprite { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public List<string> Abilities { get; set; }
    public List<string> Types { get; set; }

}
