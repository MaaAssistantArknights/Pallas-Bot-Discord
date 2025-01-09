using Microsoft.EntityFrameworkCore;

namespace PallasBot.EntityFrameworkCore;

public class PallasBotDbContext : DbContext
{
    public PallasBotDbContext(DbContextOptions<PallasBotDbContext> options) : base(options)
    {
    }
}
