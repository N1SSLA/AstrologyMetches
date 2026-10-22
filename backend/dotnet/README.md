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
```

Без `Jwt:Key` приложение не запустится и напишет, чего не хватает.

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
Shared/AstrologyMatches.Shared   общие исключения и настройки JWT
Modules/<Модуль>/...             модули (появятся на следующих этапах)
```
