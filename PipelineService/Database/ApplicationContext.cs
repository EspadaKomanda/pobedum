using Microsoft.EntityFrameworkCore;
using PipelineService.Models.Database;

namespace PipelineService.Database;


public class ApplicationContext : DbContext
{
    #region DatabaseCollections
    
    public DbSet<PipelineItem> Letters { get; set; }
    
    #endregion

    #region Constructor
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    #endregion
    
}
