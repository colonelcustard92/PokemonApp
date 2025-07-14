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

            bool hasId = request.Id.HasValue;
            bool hasName = !string.IsNullOrWhiteSpace(request.Name);

            if (hasId == hasName) // both provided or neither
                throw new BadHttpRequestException("Either Id or Name must be provided, but not both.");

            string identifier = hasId ? request.Id.Value.ToString() : request.Name!.ToLower();

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