# FileSystem SQL Training

Учебный проект для тренировки SQL, PostgreSQL 17 и ADO.NET через `Npgsql`.

## Запуск

1. Создать базу `filesystem_training` в PostgreSQL.
2. Выполнить `database/schema/001_initial_schema.sql`.
3. Выполнить `database/schema/002_seed_data.sql`.
4. При необходимости задать `ConnectionStrings__Postgres`.
5. Запустить:

```text
dotnet run --project src/FileSystem.Console
```

Все сценарии пока являются заглушками. Через меню можно показать SQL-файл и Markdown-файл заметок.

## Запуск с Docker

1. Запустить PostgreSQL 17 и автоматически применить учебный dump:

```bash
docker compose up -d
```

2. Запустить приложение из Visual Studio или командой:

```bash
dotnet run --project src/FileSystem.Console
```

3. Выбрать пункт `1` и ввести `FolderId`. Для тестовой папки `usr` используется `1`; ожидаемый результат — `20 байт`.

SQL-dump запускается PostgreSQL автоматически только при создании нового volume. Для полного сброса учебной базы:

```bash
docker compose down -v
docker compose up -d
```