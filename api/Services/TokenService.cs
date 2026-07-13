using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using api.Data;
using api.Models;
using api.Models.ViewModels;
using api.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

public class TokenService(
    AppDbContext dbContext,
    UserManager<AppUser> userManager,
    IConfiguration configuration
) : ITokenService
{
  private readonly IConfiguration _configuration = configuration;
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly AppDbContext _dbContext = dbContext;

  public async Task<TokenResponseVM> GenerateTokenAsync(
    AppUser user, string? existingRefreshToken
  )
  {
    try
    {
      // declare all the related user claims
      var authClaims = new List<Claim>()
      {
        new (ClaimTypes.Name, user.UserName ?? throw new InvalidOperationException("UserName is required.")),
        new (ClaimTypes.NameIdentifier, user.Id ),
        new (JwtRegisteredClaimNames.Email, user.Email ?? throw new InvalidOperationException("Email is required")),
        new (JwtRegisteredClaimNames.Sub, user.Email),
        new (JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
      };
      //add user roles to claim
      var userRoles = await _userManager.GetRolesAsync(user);
      if (userRoles.Contains(RolesVM.Admin))
      {
        foreach (var role in RolesVM.All) authClaims.Add(new(ClaimTypes.Role, role));
      }
      else
      {
        foreach (var role in userRoles) authClaims.Add(new(ClaimTypes.Role, role));
      }

      // register the signing key here
      var authSignKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? throw new ArgumentNullException("Jwt secret not found")));

      // create the jwt token here
      var token = new JwtSecurityToken(
        issuer: _configuration["JWT:Issuer"],
        audience: _configuration["JWT:Audience"],
        claims: authClaims,
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256)
      );
      // serialize the JWT and prepare the refresh token record
      var accessTokenString = new JwtSecurityTokenHandler().WriteToken(token);
      RefreshToken refreshToken = new()
      {
        Token = accessTokenString,
        UserId = user.Id,
        JwtId = token.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        User = user,
        DateExpire = DateTime.UtcNow.AddMonths(6)
      };
      // check appropriate validation in db and throw error appropriately
      if (string.IsNullOrEmpty(existingRefreshToken))
      {
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();
      }
      // return the token response
      TokenResponseVM response = new(
        accessTokenString,
        string.IsNullOrEmpty(existingRefreshToken) ? accessTokenString : existingRefreshToken,
        token.ValidTo
      );
      return response;
    }
    catch (Exception ex)
    {
      throw new Exception(ex.Message ?? "Erro generating user token");
    }
  }

  public async Task<TokenResponseVM?> VerifyAndValidateTokenAsync(RefreshTokenVM payload)
  {
    try
    {
      var jwtTokenHandler = new JwtSecurityTokenHandler();
      var issuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"] ?? throw new ArgumentNullException("Jwt secret not found")));
      var validationParameters = new TokenValidationParameters()
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = issuerSigningKey,
        ValidateIssuer = true,
        ValidIssuer = configuration["JWT:Issuer"] ?? throw new ArgumentNullException("Jwt secret not found"),
        ValidateAudience = true,
        ValidAudience = configuration["JWT:Audience"] ?? throw new ArgumentNullException("Jwt secret not found"),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      };
      //check jwt format
      var tokenToValidate = jwtTokenHandler.ValidateToken(payload.Token, validationParameters, out var validatedToken);
      // validate encryption algorithm
      if (validatedToken is JwtSecurityToken jwtSecurityToken)
      {
        var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        if (result == false) return null;
      }
      // validate expirations
      var expiryDateClaim = tokenToValidate.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
      if (expiryDateClaim == null) return null;
      var utcExpiryDate = long.Parse(expiryDateClaim.Value);

      var expiryDate = UnixTimeStampToDateTimeInUTC(utcExpiryDate);
      if (expiryDate > DateTime.UtcNow) throw new Exception("Token has not expired yet");
      // check for refresh token in db
      var dbRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(refreshToken => refreshToken.Token == payload.RefreshToken);
      if (dbRefreshToken is null) throw new Exception("Refresh token does not exists");
      else
      {
        // check 5 - Validate Id
        var jtiClaim = tokenToValidate.Claims.FirstOrDefault(token => token.Type == JwtRegisteredClaimNames.Jti);
        if (jtiClaim is null) throw new Exception("Token does not contain jti");
        var jti = jtiClaim.Value;

        if (dbRefreshToken.JwtId != jti) throw new Exception("Token does not match");

        // check 6 - Refresh token expiration
        if (dbRefreshToken.DateExpire <= DateTime.UtcNow) throw new Exception("Your refresh token has expired, please login");

        // check 7 - Refresh token revoked
        if (dbRefreshToken.IsRevoked) throw new Exception("Refresh token is revoked");

        // Generate new token (with existing refresh token)
        var dbUserData = await _userManager.FindByIdAsync(dbRefreshToken.UserId);
        if (dbUserData is null) throw new Exception("User not found");
        var newTokenResponse = await GenerateTokenAsync(dbUserData, payload.RefreshToken);
        return newTokenResponse;
      }

    }
    catch (SecurityTokenException)
    {
      var dbRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(refreshToken => refreshToken.Token == payload.RefreshToken);
      if (dbRefreshToken is null) throw new Exception("Refresh token does not exist");
      var dbUserData = await _userManager.FindByIdAsync(dbRefreshToken.UserId);
      if (dbUserData is null) throw new Exception("User not found");
      var newTokenResponse = await GenerateTokenAsync(dbUserData, payload.RefreshToken);
      return newTokenResponse;
    }
    catch (Exception ex)
    {
      throw new Exception(ex.Message ?? "Error while validating token");
    }
  }

  private static DateTime UnixTimeStampToDateTimeInUTC(long unixTimeStamp)
  {
    var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp);
    return dateTimeVal;
  }

}
