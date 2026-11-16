# AstrologyMatches — backend

ASP.NET Core Web API (.NET 10), модульный монолит. Правила и архитектура — в корневом `AGENTS.md`.

## Что нужно

- .NET SDK 10
- Docker (для PostgreSQL и MinIO)

## Локальные PostgreSQL и MinIO

Пока devops не добавил `docker-compose.yml`, поднимаем контейнеры вручную. Пароли ниже подходят только для локальной разработки.

```
docker run -d --name astro-db -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=astrology_matches postgres:17
docker run -d --name astro-minio -p 9000:9000 -p 9001:9001 -e MINIO_ROOT_USER=minioadmin -e MINIO_ROOT_PASSWORD=minioadmin minio/minio server /data --console-address ":9001"
```

Консоль MinIO: http://localhost:9001.

## Секреты

Секреты не коммитим, задаём через user-secrets (из корня репозитория):

```
dotnet user-secrets set "Jwt:Key" "<случайная строка не короче 32 символов>" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=5432;Database=astrology_matches;Username=postgres;Password=postgres" --project backend/dotnet/Host/AstrologyMatches.Api
```

Без `Jwt:Key` или `ConnectionStrings:Database` приложение не запустится и напишет, чего не хватает.

Необязательно: `AutoMapper:LicenseKey` — ключ бесплатной Community-лицензии AutoMapper. Без него AutoMapper работает и только пишет в лог предупреждение (для разработки это допустимо).

## Миграции БД

`dotnet-ef` подключён как локальный инструмент (`dotnet-tools.json` в корне репозитория). Один раз после клонирования:

```
dotnet tool restore
```

Применить миграции модуля (из корня репозитория, для каждого модуля своя команда):

```
dotnet ef database update --context UsersDbContext --project backend/dotnet/Modules/Users/AstrologyMatches.Users.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api
```

Новая миграция:

```
dotnet ef migrations add <Имя> --context UsersDbContext --project backend/dotnet/Modules/Users/AstrologyMatches.Users.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api --output-dir Persistence/Migrations
```

## Запуск

```
dotnet build backend/dotnet
dotnet test backend/dotnet
dotnet run --project backend/dotnet/Host/AstrologyMatches.Api --launch-profile http
```

- Swagger UI: http://localhost:5198/swagger (только в Development)
- Проверка, что сервис жив: http://localhost:5198/health

## Структура

```
Directory.Build.props            общие настройки проектов
Host/AstrologyMatches.Api        Program.cs, middleware, Swagger, JWT
Shared/AstrologyMatches.Shared   общие исключения, настройки JWT, GetUserId()
Modules/Users/...                регистрация, вход, аккаунт (api/users)
tests/AstrologyMatches.Users.Tests
```
