using AI_API.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FUNews – AI API",
        Version = "v1",
        Description = "Gợi ý tags dựa trên nội dung bài viết (keyword extraction + learning cache)"
    });
});

// ── AI Services (Singleton để Learning Cache tồn tại suốt vòng đời app) ──────
builder.Services.AddSingleton<LearningCacheService>();
builder.Services.AddSingleton<ITagExtractorService, KeywordExtractorService>();

// ── CORS – cho phép Frontend gọi trực tiếp ───────────────────────────────────
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Logging.AddConsole();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

