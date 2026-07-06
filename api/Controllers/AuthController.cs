using System;
using api.Data;
using api.Models;
using api.Models.Mapping;
using api.Models.ViewModels;
using api.Models.ViewModels.Auth;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
  AppDbContext dbContext,
  RoleManager<IdentityRole> roleManager,
  UserManager<AppUser> userManager,
  ITokenService tokenService
  ) : ControllerBase
{
  private readonly AppDbContext _dbContext = dbContext;
  private readonly RoleManager<IdentityRole> _roleManager = roleManager;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly ITokenService _tokenService = tokenService;

  [HttpPost("register-user")]
  public async Task<IActionResult> RegisterUser([FromBody] RegisterVM payload)
  {
    try
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);

      var existingUser = await _userManager.FindByEmailAsync(payload.Email);
      if (existingUser != null) return BadRequest($"User {payload.Email} already exist");

      var results = await _userManager.CreateAsync(payload.ToEntity(), payload.Password);
      if (!results.Succeeded)
      {
        var errors = results.Errors.Select(e => e.Description);
        return BadRequest(errors);
      }

      var role = RolesVM.All.Contains(payload.Role ?? string.Empty)
        ? payload.Role!
        : RolesVM.User;

      await _userManager.AddToRoleAsync(payload.ToEntity(), role);

      return Created(nameof(RegisterUser), $"User {payload.Email} is created");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
      return StatusCode(500, "An unexpected error occurred.");
    }
  }


  [HttpPost("login-user")]
  public async Task<IActionResult> LoginUser([FromBody] LoginVM payload)
  {
    try
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);
      var user = await _userManager.FindByEmailAsync(payload.Email);
      if (user is null) return BadRequest("Invalid credentials");
      if (!await _userManager.CheckPasswordAsync(user, payload.Password)) return Unauthorized("Invalid credentials");

      var tokenValue = await _tokenService.GenerateTokenAsync(user);
      return Ok(tokenValue);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
      return StatusCode(500, "An unexpected error occurred.");
    }
  }

  [HttpPost("refresh-token")]
  public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenVM payload)
  {
    try
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);
      var result = await _tokenService.VerifyAndValidateTokenAsync(payload);

      if (result is null) return Unauthorized("Invalid request");

      return Ok(result);


    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message ?? "Something went wrong");
      return StatusCode(500, "Error while processing refresh toke");
    }

    return Ok();
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword()
  {
    return Ok();
  }
}
