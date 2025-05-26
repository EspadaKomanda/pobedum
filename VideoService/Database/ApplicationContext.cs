using Microsoft.EntityFrameworkCore;
using VideoService.Models.Database;

namespace VideoService.Database;


public class ApplicationContext : DbContext
{
    #region DatabaseCollections
    
    public DbSet<Video> Videos { get; set; }
    
    #endregion

    #region Constructor
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    #endregion
    
}
