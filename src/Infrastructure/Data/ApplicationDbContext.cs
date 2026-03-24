using System.Reflection;
using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Domain.Entities;
using ebr_powerbi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace ebr_powerbi.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<LegacyAspNetUser> AspNetUsers => Set<LegacyAspNetUser>();
    public DbSet<LegacyAspNetUserRole> AspNetUserRoles => Set<LegacyAspNetUserRole>();
    public DbSet<LegacyAspNetRole> AspNetRoles => Set<LegacyAspNetRole>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<LegacyAspNetUser>().ToTable("AspNetUsers").HasKey(x => x.Id);
        builder.Entity<LegacyAspNetUserRole>().ToTable("AspNetUserRoles").HasKey(x => new { x.UserId, x.RoleId });
        builder.Entity<LegacyAspNetRole>().ToTable("AspNetRoles").HasKey(x => x.Id);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
