using System.Security.Claims;

namespace PokemonApp.DomainModels
{
    public class User
    {
        public int Id { get; set; } // Primary key
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Store a hashed password, not plaintext!
        public List<UserClaim> Claims { get; set; } = new List<UserClaim>();
    }

}
