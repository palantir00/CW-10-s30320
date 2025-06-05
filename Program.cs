using CW_10_s30320.Data;                      
using Microsoft.EntityFrameworkCore;          
using Microsoft.OpenApi.Models;               

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MasterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CW-10-s30320 API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CW-10-s30320 API V1");
    c.RoutePrefix = "swagger"; 
});

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
