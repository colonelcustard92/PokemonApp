using Newtonsoft.Json;
using PokemonApp.RequestModels;
using PokemonApp.Models;

namespace PokemonApp.Services
{
    public class PokemonServices
    {
        public async Task<PokemonResponseModel> GetPokemon(PokemonRequestModel requestParams)
        { 
            var pokemonData = await GetPokemonData(new PokemonRequestModel { ID = requestParams.ID, Name = requestParams.Name });          
            return pokemonData;
        }

        private readonly HttpClient _httpClient;

        public PokemonServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PokemonResponseModel> GetPokemonData(PokemonRequestModel request)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{request.ID.ToString() ?? request.Name ?? throw new ArgumentNullException()}"); // ID, Name - or throw exception if both are null, handle this in the top-level request handler
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var pokemon = JsonConvert.DeserializeObject<PokemonResponseModel>(jsonString) ?? new PokemonResponseModel();
            return pokemon;
        }
    }
}