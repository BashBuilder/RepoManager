using System;
using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
  public DbSet<Game> Games { get; set; }
  public DbSet<Genre> Genres { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.Entity<Genre>().HasData(
      new { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Action" },
      new { Id = Guid.Parse("11111111-2222-1111-1111-111111111111"), Name = "Adventure" },
      new { Id = Guid.Parse("11111111-3333-1111-1111-111111111111"), Name = "RPG" },
      new { Id = Guid.Parse("11111111-4444-1111-1111-111111111111"), Name = "Arcade" },
      new { Id = Guid.Parse("11111111-5555-1111-1111-111111111111"), Name = "Simulation" }
    );
    base.OnModelCreating(builder);
  }

}
