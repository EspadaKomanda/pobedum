using AuthService.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Database;

public class ApplicationContext : IdentityDbContext<User>
{

    #region DatabaseCollections

    public DbSet<User> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }

    #endregion

    #region Constructor

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    #endregion

    #region OnModelCreating
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var userRoleId = Guid.NewGuid().ToString();
        var adminRoleId = Guid.NewGuid().ToString();

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            }
        );

        builder.Entity<IdentityRoleClaim<string>>().HasData(
            
            new IdentityRoleClaim<string>
            {
                Id = 1,
                RoleId = userRoleId,
                ClaimType = "Permission",
                ClaimValue = "Read"
            },
            new IdentityRoleClaim<string>
            {
                Id = 2,
                RoleId = adminRoleId,
                ClaimType = "Permission",
                ClaimValue = "Modify"
            },
            new IdentityRoleClaim<string>
            {
                Id = 3,
                RoleId = adminRoleId,
                ClaimType = "Permission",
                ClaimValue = "History"
            },
            new IdentityRoleClaim<string>
            {
                Id = 4,
                RoleId = adminRoleId,
                ClaimType = "Permission",
                ClaimValue = "Read"
            },
            new IdentityRoleClaim<string>
            {
                Id = 5,
                RoleId = adminRoleId,
                ClaimType = "Permission",
                ClaimValue = "Modify"
            },
            new IdentityRoleClaim<string>
            {
                Id = 6,
                RoleId = adminRoleId,
                ClaimType = "Permission",
                ClaimValue = "History"
            }
        );
    }

    #endregion
}