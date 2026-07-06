using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Game
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public decimal Price { get; set; }
  public DateOnly ExpirationDate { get; set; }
  public required int GenreId { get; set; }

  [ForeignKey(nameof(GenreId))]
  public required Genre Genre { get; set; }
}
