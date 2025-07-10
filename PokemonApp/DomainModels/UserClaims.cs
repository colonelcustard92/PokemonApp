namespace PokemonApp.DomainModels
{
        public class UserClaim
        {
            public int Id { get; set; }
            public string Type { get; set; }    // e.g. "role", "email", "permission"
            public string Value { get; set; }
            public int UserId { get; set; }
            public User User { get; set; }
        }
}
