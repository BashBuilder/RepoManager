using api.Data;
using api.Extensions;
using api.Services;
var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddControllers();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddCorsServices();
builder.Services.AddJwtAuthenticationService(builder.Configuration);
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// add Middlewares
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(CorsServiceExtension.MyAllowSpecificOrigins);

// map routes
app.MapGet("/", () => "Hello World!");
app.MapControllers();

app.SeedRolesAsync().Wait();
app.SeedGenreAsync().Wait();
// perform migrations if any

app.Run();
