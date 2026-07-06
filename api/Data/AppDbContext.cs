using System;
using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
  public DbSet<Game> Games { get; set; }
  public DbSet<Genre> Genres { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.Entity<Genre>().HasData(
      new { Id = 1, Name = "Action" },
      new { Id = 2, Name = "Adventure" },
      new { Id = 3, Name = "RPG" },
      new { Id = 4, Name = "Arcade" },
      new { Id = 5, Name = "Simulation" }
    );
    base.OnModelCreating(builder);
  }

}
