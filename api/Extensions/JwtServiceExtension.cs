using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace api.Extensions;

public static class JwtServiceExtension
{

  public static IServiceCollection AddJwtAuthenticationService(this IServiceCollection services, IConfiguration configuration)
  {
    var issuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"] ?? throw new ArgumentNullException("Jwt secret not found")));

    var tokenValidationParameters = new TokenValidationParameters()
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

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.RequireHttpsMetadata = false;
      options.SaveToken = true;
      options.TokenValidationParameters = tokenValidationParameters;
    });



    return services;
  }

}
