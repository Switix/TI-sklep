using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TISklep.Service.Application;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog(); 

builder.Services.AddDbContext<BoardGamesContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/TI-sklep";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var scopeClaim = context.User.FindFirst("scope")?.Value;
            if (string.IsNullOrEmpty(scopeClaim))
                return false;

            var scopes = scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return scopes.Contains("TI-sklep:api");
        });
    });

builder.Services.AddGrpc();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<CatalogGrpcService>().RequireAuthorization("ApiScope");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BoardGamesContext>();
    try
    {
        dbContext.Database.Migrate();
        Log.Information("Migracje bazy danych zostały nałożone pomyślnie.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Błąd podczas nakładania migracji bazy danych.");
    }
}

app.Run();