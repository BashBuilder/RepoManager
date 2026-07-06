using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.ViewModels.Auth;

public class LoginVM
{
  [Required(ErrorMessage = "Email is requierd")]
  [EmailAddress(ErrorMessage = "Email not valid")]
  public required string Email { get; set; }

  [Required(ErrorMessage = "")]
  public required string Password { get; set; }
}
