using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordManager.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем базу данных SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=passwords.db"));

// JWT аутентификация
var jwtKey = "very_long_secret_key_for_jwt_token_authentication_12345!!!!!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// ВАЖНО: UseStaticFiles должен быть до UseRouting
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Маршрут для главной страницы
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "auth.html");
    if (File.Exists(filePath))
    {
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        await context.Response.WriteAsync("Frontend files not found. Place auth.html in wwwroot folder.");
    }
});

// Создаем базу данных при запуске
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        Console.WriteLine("✅ База данных создана успешно!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка создания БД: {ex.Message}");
    }
}

Console.WriteLine($"🚀 Сервер запущен: {app.Urls.FirstOrDefault()}");
Console.WriteLine($"📁 Статические файлы: {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")}");

app.Run();