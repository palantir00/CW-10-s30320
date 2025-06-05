using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CW_10_s30320.Data;   // Upewnij się, że folder z MasterContext znajduje się w tym namespace

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1) Rejestracja DbContext (Entity Framework Core + SQL Server)
//    Jeśli chcesz używać in-memory lub innej bazy, zmień poniżej.
builder.Services.AddDbContext<MasterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// -----------------------------------------------------------------------------
// 2) Rejestracja controllerów
builder.Services.AddControllers();

// -----------------------------------------------------------------------------
// 3) Konfiguracja Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CW-10-s30320 API",
        Version = "v1"
    });
    // Jeśli masz komentarze XML do endpointów, odkomentuj i ustaw ścieżkę do pliku .xml:
    /*
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    */
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// 4) W środowisku Development włączamy middleware Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CW-10-s30320 API V1");
        // Jeśli chcesz, aby Swagger UI był dostępny pod “/” (root), odkomentuj poniższy wiersz:
        // c.RoutePrefix = "";
    });
}

// -----------------------------------------------------------------------------
// 5) (Opcjonalnie) wymuś przekierowanie HTTP → HTTPS. Jeśli nie potrzebujesz, usuń linię:
app.UseHttpsRedirection();

// -----------------------------------------------------------------------------
// 6) Routing → mapowanie controllerów
app.MapControllers();

app.Run();
