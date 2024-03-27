using Microsoft.EntityFrameworkCore;

namespace CatTok.Infrastructure;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    
}