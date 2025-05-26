using LetterService.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace LetterService.Database;


public class ApplicationContext : DbContext
{
    #region DatabaseCollections
    
    public DbSet<Letter> Letters { get; set; }
    public DbSet<FavouritesInfo> Favourites { get; set; }
    
    #endregion

    #region Constructor
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    #endregion
    
}
