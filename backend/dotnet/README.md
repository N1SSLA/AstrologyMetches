# AstrologyMatches — backend

ASP.NET Core Web API (.NET 10), модульный монолит. Правила и архитектура — в корневом `AGENTS.md`.

## Что нужно

- .NET SDK 10
- Docker (для PostgreSQL и MinIO)

## Локальные PostgreSQL и MinIO

Пока devops не добавил `docker-compose.yml`, поднимаем контейнеры вручную. Пароли ниже подходят только для локальной разработки.

Используемые Docker-образы (версии проверены 2026-11-16):

| Сервис | Образ | Контейнер | Порты | Версия внутри |
|---|---|---|---|---|
| PostgreSQL | `postgres:17.11` (Docker Hub) | `astro-db` | 5432 | PostgreSQL 17.11 |
| MinIO | `cgr.dev/chainguard/minio@sha256:9158c67aa441627c366442e84656f689e16bec4809851ec64318561a365b69e4` (Chainguard) | `astro-minio` | 9000 (S3 API), 9001 (консоль) | `RELEASE.2026-11-12T01-15-23Z` |

Версии образов фиксированные, чтобы у всей команды были одинаковые PostgreSQL и MinIO:
- **PostgreSQL** — тег с минорной версией `17.11`, а не плавающий `17`.
- **MinIO** — по digest. Chainguard бесплатно публикует только тег `latest`, тегов с версией у него нет, а digest всегда указывает на один и тот же образ.

Обновляем версии осознанно: меняем тег или digest в этой таблице и в командах ниже, затем пересоздаём контейнер.

```
docker run -d --name astro-db -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=astrology_matches postgres:17.11
docker run -d --name astro-minio --user root -p 9000:9000 -p 9001:9001 -v astro-minio-data:/data -e MINIO_ROOT_USER=minioadmin -e MINIO_ROOT_PASSWORD=minioadmin cgr.dev/chainguard/minio@sha256:9158c67aa441627c366442e84656f689e16bec4809851ec64318561a365b69e4 server /data --console-address ":9001"
```

Консоль MinIO: http://localhost:9001.

Про образ MinIO:
- Официальные образы больше не скачиваются: `minio/minio` удалён с Docker Hub, а `quay.io/minio/minio` требует авторизации. Поэтому берём сборку Chainguard (`cgr.dev/chainguard/minio`).
- Внутри неё MinIO работает не от root и не может писать в том. Для локальной разработки запускаем с `--user root`.
- Файлы лежат в томе `astro-minio-data` и не пропадают при пересоздании контейнера.

Если MinIO отвечает 403 с `RequestTimeTooSkewed`, значит часы Docker разошлись с часами Windows (так бывает после сна компьютера). Перезапустите Docker Desktop.

## Секреты

Секреты не коммитим, задаём через user-secrets (из корня репозитория):

```
dotnet user-secrets set "Jwt:Key" "<случайная строка не короче 32 символов>" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=5432;Database=astrology_matches;Username=postgres;Password=postgres" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "Minio:AccessKey" "minioadmin" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "Minio:SecretKey" "minioadmin" --project backend/dotnet/Host/AstrologyMatches.Api
```

Без `Jwt:Key`, `ConnectionStrings:Database` или ключей MinIO приложение не запустится и напишет, чего не хватает.

Адрес MinIO (`Minio:Endpoint`, по умолчанию `localhost:9000`) задан в `appsettings.json`. По этому же адресу браузер открывает ссылки на фото (presigned URL), поэтому MinIO должен быть доступен и из браузера. Bucket `profile-photos` создаётся сам при первой загрузке.

Необязательно: `AutoMapper:LicenseKey` — ключ бесплатной Community-лицензии AutoMapper. Без него AutoMapper работает и только пишет в лог предупреждение (для разработки это допустимо).

## Миграции БД

`dotnet-ef` подключён как локальный инструмент (`dotnet-tools.json` в корне репозитория). Один раз после клонирования:

```
dotnet tool restore
```

Применить миграции модуля (из корня репозитория, для каждого модуля своя команда):

```
dotnet ef database update --context UsersDbContext --project backend/dotnet/Modules/Users/AstrologyMatches.Users.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api
dotnet ef database update --context ProfilesDbContext --project backend/dotnet/Modules/Profiles/AstrologyMatches.Profiles.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api
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
Modules/Profiles/...             пока только фото профиля в MinIO (api/profiles/.../photos); анкету добавит автор модуля
tests/AstrologyMatches.<Модуль>.Tests
```
