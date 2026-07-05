using System;

namespace api.Extensions;

public static class CorsServiceExtension
{
  public const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
  public static IServiceCollection AddCorsServices(this IServiceCollection services)
  {
    services.AddCors(options =>
    {
      options.AddPolicy(name: MyAllowSpecificOrigins,
                        policy =>
                        {
                          policy.WithOrigins("*", "")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
    });
    return services;
  }
}
