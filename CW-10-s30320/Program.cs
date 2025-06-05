// ---------------------------------------------------------------------
// Plik: Program.cs
// ---------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CW_10_s30320.Data; // upewnij się, że MasterContext jest w tej przestrzeni nazw

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1) Konfigurujemy DbContext (SQL Server lub inna baza – dostosuj conn. string)
// ---------------------------------------------------------------------
builder.Services.AddDbContext<MasterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ---------------------------------------------------------------------
// 2) Rejestrujemy kontrolery i Swaggera
// ---------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CW-10-s30320 API",
        Version = "v1"
    });
});

var app = builder.Build();

// ---------------------------------------------------------------------
// 3) Middleware (HTTPS, autoryzacja, Swagger UI)
// ---------------------------------------------------------------------
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CW-10-s30320 API V1");
    // Jeżeli chcesz, by Swagger był dostępny pod https://localhost:5001/ zamiast /swagger/index.html,
    // odkomentuj poniższą linię:
    // c.RoutePrefix = string.Empty;
});

app.MapControllers();

app.Run();


// ---------------------------------------------------------------------
// DOKŁADNIE TĘ KLASĘ DODAJEMY PONIŻEJ! (aby WebApplicationFactory<Program> mógł znaleźć Program)
// ---------------------------------------------------------------------
public partial class Program { }
