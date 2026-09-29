-- SQL-шаблон вставки одной папки.
-- @name и @parent_folder_id передаются из C# через NpgsqlParameter.

INSERT INTO folders
(
    name,
    parent_folder_id,
    created_at_utc
)
VALUES
(
    @name,
    @parent_folder_id,
    @created_at_utc
)
RETURNING id;
