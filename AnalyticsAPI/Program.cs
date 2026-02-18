using AnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<FunewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Memory Cache (dùng cho dashboard caching)
builder.Services.AddMemoryCache();

// CORS – cho phép FE gọi
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFE");
app.UseAuthorization();
app.MapControllers();
app.Run();
