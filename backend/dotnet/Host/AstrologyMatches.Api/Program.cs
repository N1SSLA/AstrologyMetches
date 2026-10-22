using AstrologyMatches.Api.Extensions;
using AstrologyMatches.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

// Сюда по мере готовности добавляются модули: builder.Services.AddUsersModule(builder.Configuration) и т.д.

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
