using Microsoft.EntityFrameworkCore;
using PipelineService.Models.Database;

namespace PipelineService.Database;


public class ApplicationContext : DbContext
{
    #region DatabaseCollections
    
    public DbSet<PipelineItem> Letters { get; set; }
    private readonly IConfiguration builder;
    #endregion

    #region Constructor
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options, IConfiguration builder) : base(options)
    {
        this.builder = builder;
        Database.EnsureCreated();
    }
    public ApplicationContext(IConfiguration builder) : base()
    {
        this.builder = builder;
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbSettings =  builder.GetSection("DatabaseSettings");
        var hostname = dbSettings["Hostname"] ?? "localhost";
        var port = dbSettings["Port"] ?? "5432";
        var name = dbSettings["Name"] ?? "postgres";
        var username = dbSettings["Username"] ?? "postgres";
        var password = dbSettings["Password"] ?? "postgres";
        optionsBuilder.UseNpgsql($"Server={hostname}:{port};Database={name};Uid={username};Pwd={password};");
    }

    #endregion
    
}
