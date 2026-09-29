-- Проверка результата импорта.
-- @root_folder_id передаётся из C# после вставки корневой папки.

WITH RECURSIVE imported_folders AS
(
    SELECT folders.id
    FROM folders
    WHERE folders.id = @root_folder_id

    UNION ALL

    SELECT child_folders.id
    FROM folders AS child_folders
    INNER JOIN imported_folders
        ON child_folders.parent_folder_id = imported_folders.id
)
SELECT
    COUNT(DISTINCT imported_folders.id)::int AS folder_count,
    COUNT(files.id)::int AS file_count
FROM imported_folders
LEFT JOIN files
    ON files.folder_id = imported_folders.id;
