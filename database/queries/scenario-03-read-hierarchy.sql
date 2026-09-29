-- Читаем из БД папки и файлы импортированной иерархии.

WITH RECURSIVE imported_folders AS
(
    SELECT
        folders.id,
        folders.name,
        folders.parent_folder_id
    FROM folders
    WHERE folders.id = @root_folder_id

    UNION ALL

    SELECT
        child_folders.id,
        child_folders.name,
        child_folders.parent_folder_id
    FROM folders AS child_folders
    INNER JOIN imported_folders
        ON child_folders.parent_folder_id = imported_folders.id
)
SELECT
    imported_folders.id AS folder_id,
    imported_folders.name AS folder_name,
    imported_folders.parent_folder_id,
    files.name AS file_name,
    files.size_bytes
FROM imported_folders
LEFT JOIN files
    ON files.folder_id = imported_folders.id
ORDER BY
    imported_folders.id,
    files.id;
