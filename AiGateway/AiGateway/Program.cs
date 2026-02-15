using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. טעינת משתני הסביבה מקובץ ה-.env
DotNetEnv.Env.Load();

// 2. הוספת שירותי ה-API (Controllers ו-Swagger)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. הגדרת שירות הגבלת הקצב (Rate Limiter) - שלב 7
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 5; // מקסימום 5 בקשות
        options.Window = TimeSpan.FromMinutes(1); // בתוך חלון זמן של דקה אחת
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0; // ללא תור - דחייה מיידית של חריגות
    });
});

var app = builder.Build();

// 4. הפעלת ה-Rate Limiter בתוך ה-Pipeline
app.UseRateLimiter(); // חייב להופיע לפני MapControllers

// 5. הגדרות סביבת פיתוח (Swagger)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();