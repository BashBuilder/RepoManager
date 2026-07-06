using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.ViewModels.Auth;

public class RefreshTokenVM
{
  [Required(ErrorMessage = "Token is requried")]
  public required string Token { get; set; }
  [Required(ErrorMessage = "Refresh Token is requried")]
  public required string RefreshToken { get; set; }
}
