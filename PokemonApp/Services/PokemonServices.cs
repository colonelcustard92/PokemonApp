using PokemonApp.Models;
using PokemonApp.DTOs;
using Newtonsoft.Json.Linq;

namespace PokemonApp.Services
{
    public class PokemonServices
    {

        public async Task<Pokemon> GetPokemon(PokemonDTO requestParams)
        {
            // Validate the request body
            if (requestParams == null || string.IsNullOrEmpty(requestParams.Name) && requestParams.ID == null)
            {
                throw new ArgumentException("Invalid request. Either Name or ID must be provided.");
            }

            // Call the external API to get the Pokemon data
            var pokemonData = await GetPokemonData(new PokemonDTO { ID = requestParams.ID, Name = requestParams.Name });

            var resultData = new Pokemon
            {
                
                Name = pokemonData.Name,
                Sprite = pokemonData.Sprite,
                Height = pokemonData.Height,
                Weight = pokemonData.Weight,
                Abilities = pokemonData.Abilities,
                Types = pokemonData.Types
            };

            //Map the API response to the Pokemon model
            return resultData;
        }


            private readonly HttpClient _httpClient;

        public PokemonServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Pokemon> GetPokemonData(PokemonDTO request)
        {
            // Replace with actual API call to fetch Pokemon data
            var response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{request.Name?.ToLower()}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var pokemon = new Pokemon
            {
                Name = data["name"]?.ToString()??"",
                Height = data["height"]?.Value<int>() ?? 0,
                Weight = data["weight"]?.Value<int>() ?? 0,
                Sprite = data["sprites"]?["front_default"]?.ToString() ??"",
                Abilities = data["abilities"].Select(a => a["ability"]?["name"]?.ToString())
                    .Where(name => name != null)
                    .ToList(),
                Types = data["types"]
                    ?.Select(t => t["type"]?["name"]?.ToString())
                    .Where(name => name != null)
                    .ToList()
            };
           
            return pokemon;
        }
    }
}

