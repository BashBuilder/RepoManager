using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace api.Models.ViewModels.Auth;

public class RegisterVM
{
  [Required(ErrorMessage = "First name is required")]
  [MaxLength(50, ErrorMessage = "First name is too long")]
  public required string FirstName { get; set; }

  [Required(ErrorMessage = "Last name is required")]
  [MaxLength(50, ErrorMessage = "Last name is too long")]
  public required string LastName { get; set; }

  [Required(ErrorMessage = "User name is required")]
  [MaxLength(50, ErrorMessage = "User name is too long")]
  public required string UserName { get; set; }

  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Email is not valid")]
  public required string Email { get; set; }

  [Required(ErrorMessage = "Password is required")]
  public required string Password { get; set; }

  public string Role { get; set; } = string.Empty;

}
