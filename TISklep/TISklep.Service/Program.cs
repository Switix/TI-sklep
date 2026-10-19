using Microsoft.EntityFrameworkCore;
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


var app = builder.Build();

app.UseSerilogRequestLogging();

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