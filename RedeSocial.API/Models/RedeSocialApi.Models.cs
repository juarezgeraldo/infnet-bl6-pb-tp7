using Microsoft.EntityFrameworkCore;

namespace RedeSocial.API.Models
{
    public class RedeSocialApiContext : DbContext
    {
        public RedeSocialApiContext(DbContextOptions<RedeSocialApiContext> options) : base(options) { }
        public DbSet<RedeSocialApiContext> RedeSocialApiContexts { get; set; }
    }
}
