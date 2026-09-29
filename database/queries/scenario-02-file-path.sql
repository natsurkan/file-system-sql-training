-- Задание 2: полный путь от папки файла до корня.
-- @file_id — id файла.
WITH RECURSIVE file_folder_hierarchy AS
(
    -- Находим папку, в которой лежит файл
    SELECT
        folders.id,
        folders.name,
        folders.parent_folder_id,
        ARRAY[folders.name]::varchar[] AS folder_names_from_file
    FROM files
    INNER JOIN folders
        ON folders.id = files.folder_id
    WHERE files.id = @file_id

    UNION ALL

    -- Поднимаемся к родительской папке
    SELECT
        parent_folders.id,
        parent_folders.name,
        parent_folders.parent_folder_id,
        ARRAY[parent_folders.name] || current_folder.folder_names_from_file
    FROM folders AS parent_folders
    INNER JOIN file_folder_hierarchy AS current_folder
        ON parent_folders.id = current_folder.parent_folder_id
), root_path AS
(
    -- В строке корневой папки уже собраны все имена от корня до файла
    SELECT folder_names_from_file AS folder_names
    FROM file_folder_hierarchy
    WHERE parent_folder_id IS NULL
)
SELECT
    '/' || array_to_string(root_path.folder_names[1:path_length], '/') AS full_path
FROM root_path
CROSS JOIN LATERAL generate_series(
    array_length(root_path.folder_names, 1),
    1,
    -1
) AS path_lengths(path_length)

UNION ALL

SELECT '/' AS full_path
FROM root_path

ORDER BY full_path DESC;