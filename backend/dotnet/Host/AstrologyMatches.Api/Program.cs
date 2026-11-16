using AstrologyMatches.Api.Extensions;
using AstrologyMatches.Api.Middleware;
using AstrologyMatches.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // По правилам проекта действия называются с суффиксом Async. Без этой настройки MVC отрезает суффикс,
    // и CreatedAtAction(nameof(GetMeAsync), ...) не находит действие
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddHealthChecks();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

// Ключ лицензии AutoMapper задаём один раз здесь, а профили маппинга добавляет каждый модуль у себя
builder.Services.AddAutoMapper(config => config.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"]);

builder.Services.AddUsersModule(builder.Configuration);

var app = builder.Build();

// Первым в конвейере, чтобы ловить исключения из всего, что идёт после него
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
