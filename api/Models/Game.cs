using System;

namespace api.Models;

public class Game
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public decimal Price { get; set; }
  public DateOnly ExpirationDate { get; set; }
}
