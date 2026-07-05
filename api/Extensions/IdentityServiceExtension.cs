using System;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Extensions;

public static class IdentityServiceExtension
{
  public static IServiceCollection AddIdentityServices(this IServiceCollection services)
  {
    services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

    return services;
  }
}
