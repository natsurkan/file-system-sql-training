-- Задание 9. Шаг 2: заполняем Materialized Path для всех папок.

WITH RECURSIVE folder_paths AS
(
    SELECT
        folders.id,
        '/' || folders.name AS materialized_path
    FROM folders
    WHERE folders.parent_folder_id IS NULL

    UNION ALL

    SELECT
        child_folders.id,
        folder_paths.materialized_path
            || '/'
            || child_folders.name
    FROM folders AS child_folders
    INNER JOIN folder_paths
        ON child_folders.parent_folder_id =
           folder_paths.id
)
UPDATE folders
SET materialized_path = folder_paths.materialized_path
FROM folder_paths
WHERE folders.id = folder_paths.id;
