using System;
using api.Data;

namespace api.Extensions;

public static class AppContextExtensionService
{
  public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
  {
    string connectionString = configuration.GetConnectionString("Database") ?? throw new ArgumentNullException(nameof(configuration));
    services.AddSqlite<AppDbContext>(connectionString);
    return services;
  }

}
