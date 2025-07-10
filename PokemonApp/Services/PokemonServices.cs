using Newtonsoft.Json;
using PokemonApp.RequestModels;
using PokemonApp.Models;

namespace PokemonApp.Services
{
    public class PokemonServices
    {
        public async Task<PokemonResponseModel> GetPokemon(PokemonRequestModel requestParams)
        { 
            var pokemonData = await GetPokemonData(new PokemonRequestModel { Id = requestParams.Id, Name = requestParams.Name });          
            return pokemonData;
        }

        private readonly HttpClient _httpClient;

        public PokemonServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PokemonResponseModel> GetPokemonData(PokemonRequestModel request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Determine the identifier to use in the URL
            string identifier = request.Id.HasValue
                ? request.Id.Value.ToString()
                : !string.IsNullOrWhiteSpace(request.Name)
                    ? request.Name.ToLower()
                    : throw new ArgumentNullException("Either Id or Name must be provided.");

            // Make the API request
            HttpResponseMessage response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{identifier}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserialize into your expected model
            var pokemon = JsonConvert.DeserializeObject<PokemonResponseModel>(jsonString);

            return pokemon;
        }
    }
}