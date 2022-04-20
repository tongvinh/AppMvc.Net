using System.Linq.Expressions;
using App.Models.Contacts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace App.Models
{
  public class AppDbContext : IdentityDbContext<AppUser>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      foreach (var entityType in modelBuilder.Model.GetEntityTypes())
      {
        var tableName = entityType.GetTableName();
        if (tableName.StartsWith("AspNet"))
        {
          entityType.SetTableName(tableName.Substring(6));
        }
      }
    }

    public DbSet<Contact> Contacts { get; set; }
  }
}