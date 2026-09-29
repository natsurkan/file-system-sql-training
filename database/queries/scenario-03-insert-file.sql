-- SQL-шаблон вставки одного файла.
-- Значения передаются из C# через NpgsqlParameter.

INSERT INTO files
(
    folder_id,
    name,
    content,
    size_bytes,
    created_at_utc
)
VALUES
(
    @folder_id,
    @name,
    @content,
    @size_bytes,
    @created_at_utc
);
