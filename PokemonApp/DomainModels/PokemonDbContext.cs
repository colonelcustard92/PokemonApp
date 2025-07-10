
using Microsoft.EntityFrameworkCore;
namespace PokemonApp.DomainModels;
public class PokemonDbContext(DbContextOptions<PokemonDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
