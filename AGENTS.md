# AstrologyMatches — правила для ИИ-агентов

Правила для всех ИИ-агентов команды

## О проекте

Веб-приложение для знакомств с расчётом астрологической совместимости. Пользователь регистрируется, заполняет профиль и данные рождения, получает натальную карту, смотрит карточки кандидатов с % совместимости, свайпает, переписывается с мэтчами, получает гороскоп и уведомления.

Проект учебный и командный.

## Главное правило: код уровня junior .NET

Код должен быть понятен junior .NET-разработчику, которому предстоит его читать, поддерживать и защищать на ревью.

- Простые явные конструкции: обычные классы и методы, `async/await`, понятный LINQ. Без рефлексии, кодогенерации, хитрых дженериков и «магии».
- Понятные имена вместо комментариев. Комментарий пишем только там, где без него не понять, *почему* сделано так.
- Маленькие шаги: одна задача или user story за раз. Соседний код без просьбы не рефакторим.
- После изменений коротко объясни, что сделал и почему: это часть обучения команды.

### Сложные решения — только после объяснения

Если, по-твоему, нужен паттерн, библиотека или подход сложнее описанного здесь, сначала объясни:
1. какую проблему он решает именно в нашем проекте;
2. как выглядит простое решение без него и чем оно хуже;
3. чего это будет стоить: новые понятия, файлы, зависимости.

Внедряй только после согласия.

Без согласования не используем: MediatR / CQRS-пайплайны, generic Repository / Unit of Work поверх EF Core, доменные события и Outbox, брокеры сообщений, Redis, gRPC, FluentValidation, Result-библиотеки, Testcontainers, генерацию клиентов по OpenAPI. Любой новый пакет NuGet / npm тоже добавляем только после объяснения.

## Архитектура

```
WebUI (React + TS) ──HTTP + SignalR──▶ App (ASP.NET Core, модульный монолит) ──HTTP──▶ AstroService (Python)
                                           │                    │
                                       PostgreSQL           MinIO (фото)
```

| Часть | Путь | Стек | Отвечает за |
|---|---|---|---|
| App | `backend/dotnet` | .NET 10, ASP.NET Core Web API, EF Core + Npgsql, SignalR | весь бэкенд и хранение данных, включая астрорезультаты |
| AstroService | `src/AstroService` | Python-микросервис | астрологические расчёты |
| WebUI | `src/WebUI` | React + TypeScript (Vite) | интерфейс |

### AstroService

Отдельный микросервис, его ведёт другой участник команды. Код в `src/AstroService` без явной просьбы не меняем.
App обращается к нему только по HTTP из модуля Astrology: typed `HttpClient` в `Astrology.Infrastructure` за интерфейсом из `Astrology.Application`. Результаты расчётов сохраняет модуль Astrology.
Контракт (эндпоинты, форматы запросов и ответов) согласуется с владельцем сервиса. Не придумывай его сам.

### Модули App

| Модуль | Отвечает за | Таблицы |
|---|---|---|
| Users | регистрация, вход, аккаунт | `users` |
| Profiles | профиль, фото (MinIO), данные рождения | `profiles`, `birth_data`, `profile_photos` |
| Astrology | натальные карты, совместимость, гороскопы (считает через AstroService) | `natal_charts`, `natal_positions`, `compatibility_reports`, `compatibility_aspects`, `compatibility_scores`, `horoscopes` |
| Matching | лента кандидатов, свайпы, мэтчи, размэтч | `swipes`, `matches` |
| Chat | чаты и сообщения (SignalR) | `chats`, `messages` |
| Notifications | уведомления | `notifications` |

### Структура решения (Clean Architecture)

```
backend/dotnet/
  AstrologyMatches.slnx
  Directory.Build.props                 общие настройки всех csproj (net10.0, Nullable, XML-документация)
  Host/AstrologyMatches.Api/            Program.cs, подключение модулей, middleware, Swagger, appsettings
  Shared/AstrologyMatches.Shared/       только действительно общее (базовые исключения и т.п.)
  Modules/
    Profiles/
      AstrologyMatches.Profiles.Domain/
      AstrologyMatches.Profiles.Application/
      AstrologyMatches.Profiles.Infrastructure/
    Users/, Astrology/, Matching/, Chat/, Notifications/   — по той же схеме
  tests/
    AstrologyMatches.Profiles.Tests/    — по тест-проекту на модуль
```

| Проект | Что внутри | Может ссылаться на |
|---|---|---|
| `*.Domain` | сущности, enum'ы, бизнес-правила | только Shared |
| `*.Application` | сервисы сценариев (`IProfileService` / `ProfileService`), DTO, интерфейс `IProfilesDbContext`, интерфейсы внешних сервисов (`IPhotoStorage`, клиент AstroService); папка `Contracts/` — публичный API модуля для других модулей | Domain своего модуля; Application других модулей (только типы из их `Contracts/`) |
| `*.Infrastructure` | `ProfilesDbContext`, конфигурации EF, миграции, реализации внешних сервисов (MinIO, HTTP-клиенты), контроллеры и SignalR-хабы модуля, `AddProfilesModule(...)` | Application и Domain своего модуля |
| `Host/*.Api` | `Program.cs` вызывает `Add<Module>Module()` каждого модуля; middleware | Infrastructure всех модулей |

### Границы модулей

- Модуль не использует Domain, Infrastructure и DbContext другого модуля и не читает его таблицы.
- Нужны данные другого модуля — вызывай интерфейс из его `Contracts/` (например, `IProfilesModuleApi.GetCandidateCardAsync(...)`), который возвращает DTO.
- Циклические зависимости между модулями запрещены. Если без цикла не обойтись, остановись и предложи решение с объяснением.
- Между модулями храним только Id (`Guid`): без навигационных свойств EF и без FK между схемами.

## Бэкенд (.NET)

- .NET 10; `Nullable` и `ImplicitUsings` включены; file-scoped namespaces; один тип — один файл. Общие настройки проектов лежат в `backend/dotnet/Directory.Build.props`.
- Идентификаторы на английском, комментарии и XML-документация на русском.
- XML-документация краткая: `<summary>` в 1–2 строки на публичные контроллеры и их действия, интерфейсы сервисов, `Contracts`, DTO и сущности Domain. `<param>` и `<returns>` пишем, только если смысл неочевиден из имени. Пересказывать код в документации не нужно.
- API — контроллеры с `[ApiController]`, маршруты `api/<module>/...`. Контроллер тонкий: принял запрос, вызвал сервис, вернул результат.
- Бизнес-логика живёт в сервисах Application и методах сущностей Domain. В контроллерах, хабах и DbContext её нет.
- DTO — `record`. Маппинг сущность ↔ DTO делаем через AutoMapper не ниже 16.3.0 (более ранние версии уязвимы: CVE-2026-32933, CVE-2026-128081, CVE-2026-128082; ключ Community-лицензии — `AutoMapper:LicenseKey` в user-secrets, задаётся один раз в Host): у каждого модуля свой профиль в Application (`ProfilesMappingProfile : Profile`), регистрируется в `Add<Module>Module(...)` через `services.AddAutoMapper(config => config.AddProfile<...>())`. В профилях только перекладывание полей, бизнес-логику туда не прячем. Если маппинг поля требует нетривиального `ForMember`, лучше посчитать значение в сервисе.
- Валидация: DataAnnotations на DTO запросов, проверки бизнес-правил — в сервисах.
- Ошибки: свои исключения (`NotFoundException`, `ValidationException`, `ForbiddenException`, `ConflictException`) лежат в `Shared/Exceptions`. В Host есть `ExceptionHandlingMiddleware`: он ловит исключение, по его типу выбирает статус-код (404 / 400 / 403 / 409, остальное — 500), пишет ошибку в лог и отвечает `ProblemDetails`. Стектрейс наружу не отдаём, в контроллерах `try/catch` ради статус-кодов не пишем.
- Методы с I/O — `async`, с суффиксом `Async`; `CancellationToken` передаём от контроллера до EF. Действия контроллеров тоже с `Async` (в Host выключен `SuppressAsyncSuffixInActionNames`, поэтому `CreatedAtAction(nameof(GetMeAsync), ...)` работает).
- Id текущего пользователя в контроллере — `User.GetUserId()` (Shared/Authentication, claim `sub`). Id передаём в сервис параметром.
- DI: каждый модуль регистрируется своим методом `Add<Module>Module(this IServiceCollection, IConfiguration)`. Сервисы по умолчанию `Scoped`.
- Логирование — `ILogger<T>`.

### Swagger

- Swashbuckle (`AddSwaggerGen` + `UseSwaggerUI`) настраивается в Host и включён только в Development.
- XML-документация каждой сборки модуля подключается через `IncludeXmlComments`, поэтому `<summary>` контроллеров и DTO видны в Swagger UI.
- В Swagger настроена JWT-авторизация (кнопка Authorize), чтобы закрытые эндпоинты можно было проверить прямо из UI.
- У каждого действия контроллера есть `[ProducesResponseType]` на все возможные коды ответа (200/201/400/404/…).
- Новый или изменённый эндпоинт после правки должен корректно отображаться в Swagger.

### Чат (SignalR)

- Хаб `ChatHub` лежит в `Chat.Infrastructure`. Он тонкий: вызывает `ChatService`, тот сохраняет сообщение в БД, и только после этого хаб рассылает его участникам чата.
- Хаб доступен только авторизованным (`[Authorize]`). JWT приходит в query-параметре `access_token`: WebSocket не умеет передавать заголовок.
- Перед отправкой проверяем, что пользователь — участник чата и мэтч активен.
- История переписки отдаётся обычным REST-запросом, а не через хаб.

## Хранение данных

### PostgreSQL + EF Core

- Одна БД. У каждого модуля своя схема (`users`, `profiles`, `astrology`, `matching`, `chat`, `notifications`) и свой DbContext; миграции лежат в его Infrastructure-проекте.
- Таблицы и колонки в snake_case (`UseSnakeCaseNamingConvention()`), PK — `id` типа `uuid` (`Guid`).
- Моменты времени — `DateTime` в UTC (`timestamptz`); дата рождения — `DateOnly`; время рождения — `TimeOnly`; часовой пояс — IANA id (`Europe/Moscow`).
- Схему меняем только миграциями EF, сгенерированные миграции без причины руками не правим. Изменение схемы сначала согласуй.
- У каждого DbContext своя таблица истории миграций в своей схеме: `MigrationsHistoryTable("__ef_migrations_history", <схема>)`.
- `dotnet-ef` — локальный инструмент (`dotnet tool restore`). Миграция (из корня репозитория):
  ```
  dotnet ef migrations add <Имя> --context ProfilesDbContext --project backend/dotnet/Modules/Profiles/AstrologyMatches.Profiles.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api --output-dir Persistence/Migrations
  ```

### MinIO (изображения)

- Файлы фотографий лежат в MinIO, а в БД (`profile_photos`) — только метаданные.
- Ключ объекта — `{profileId}/{photoId}`.
- Работаем через интерфейс `IPhotoStorage` (Profiles.Application), реализация на пакете `Minio` — в Profiles.Infrastructure. Остальной код про MinIO не знает.
- На сервере проверяем тип (`image/jpeg`, `image/png`, `image/webp`) и размер файла. Тип определяем по первым байтам файла (`ImageFormat.DetectContentType`), а не по Content-Type из запроса. Лимиты: 5 МБ на файл, 6 фото на профиль (`ProfilePhoto`).
- Загрузка идёт через API (`POST api/profiles/me/photos`, multipart). Клиенту фото отдаём presigned URL из MinIO (поле `Url` в DTO, живёт `Minio:UrlLifetimeMinutes`, по умолчанию 60 мин). Когда ссылка истечёт, клиент заново запрашивает список фото.
- Настройки — секция `Minio` (`Endpoint`, `BucketName`, `UseSsl`); `AccessKey` и `SecretKey` лежат в user-secrets. MinIO должен быть доступен из браузера по `Minio:Endpoint`: на этот адрес указывают presigned URL.

## WebUI (React + TypeScript)

- Vite + React + TypeScript в strict-режиме, функциональные компоненты и хуки.
- Структура: `src/pages/` (экраны), `src/components/` (переиспользуемое), `src/api/` (запросы), `src/types/` (типы).
- В API ходим только через `src/api/` (обёртка над `fetch`). Типы ответов пишем руками, они повторяют DTO бэкенда.
- Чат — через `@microsoft/signalr`; подключение создаётся в одном месте (`src/api/chatConnection.ts`).
- Состояние — `useState` / `useContext`. Redux, React Query, UI-киты и CSS-фреймворки подключаем только после объяснения.

## Безопасность и приватность

- Пароли храним только как хеш (`PasswordHasher<T>`); аутентификация — JWT Bearer.
- Пароли, токены и персональные данные не логируем.
- Если пользователь скрыл время рождения (`hide_birth_time`), другие пользователи не получают его через API.
- Пользователь читает и меняет только свои данные; владельца проверяем на сервере.
- Секреты (строки подключения, ключ JWT, ключи MinIO) не коммитим: `dotnet user-secrets` или переменные окружения.

## Тесты

- xUnit, по тест-проекту на модуль; тестируем Domain и сервисы Application; моки — Moq. DbContext в тестах сервисов — EF Core InMemory (своя БД на каждый тест), внешние зависимости (JWT, AstroService, MinIO) мокаем.
- На маппинг каждого модуля есть тест `MapperConfiguration.AssertConfigurationIsValid()`: он ловит поля DTO, которые AutoMapper не смог заполнить. В AutoMapper 16 конструктору нужен логгер: `new MapperConfiguration(config => ..., NullLoggerFactory.Instance)`.
- Новая логика или исправление бага идёт вместе с тестом. Прежде чем сказать «готово», собери проект и прогони тесты.

## Команды

Backend (из корня репозитория; подробности, в том числе запуск PostgreSQL и MinIO в Docker, — в `backend/dotnet/README.md`):

```
dotnet tool restore
dotnet user-secrets set "Jwt:Key" "<строка от 32 символов>" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "ConnectionStrings:Database" "<строка подключения>" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "Minio:AccessKey" "<логин MinIO>" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet user-secrets set "Minio:SecretKey" "<пароль MinIO>" --project backend/dotnet/Host/AstrologyMatches.Api
dotnet ef database update --context UsersDbContext --project backend/dotnet/Modules/Users/AstrologyMatches.Users.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api
dotnet ef database update --context ProfilesDbContext --project backend/dotnet/Modules/Profiles/AstrologyMatches.Profiles.Infrastructure --startup-project backend/dotnet/Host/AstrologyMatches.Api
dotnet build backend/dotnet
dotnet test backend/dotnet
dotnet run --project backend/dotnet/Host/AstrologyMatches.Api --launch-profile http
```

Swagger UI: http://localhost:5198/swagger, проверка живости: http://localhost:5198/health.

Docker-образы указываем только с фиксированной версией: тег с точной версией (`postgres:17.11`) или digest (`image@sha256:...`), если у образа нет тегов с версией. Плавающие теги (`latest`, `17`) не используем. Актуальный список образов — в `backend/dotnet/README.md`.

> Целевые команды, которых ещё нет: `docker compose up -d db minio` (ждём compose от devops), `cd src/WebUI && npm run dev`, `cd src/WebUI && npm run lint && npm test`.

## Git

- Ветки: `main` — стабильная, `develop` — интеграция, `feature/<имя>` — рабочие. PR открываем в `develop`.
- Коммиты короткие, на русском языке. Заголовок начинается с глагола в инфинитиве и описывает, что делает коммит (`Добавить API профиля`). Детали, если нужны, пишем списком после пустой строки.
- Агент не коммитит, не пушит и не мержит без явной просьбы и никогда не коммитит напрямую в `main` или `develop`.
- `.github/workflows/` и `DockerConf/` ведёт devops-участник команды, `src/AstroService/` — другой участник. Без просьбы их не трогаем: наша зона — Web API (`backend/dotnet`).

## Открытые вопросы — не решай сам

Если задача упирается в один из них, предложи 2–3 варианта с плюсами и минусами и дождись решения:
- push-уведомления;
- контракт с AstroService (согласуется с его владельцем).

Когда вопрос решён, перенеси решение в соответствующий раздел этого файла.
