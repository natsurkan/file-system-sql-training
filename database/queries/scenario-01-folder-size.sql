-- Задание 1: суммарный размер файлов в заданной папке со всей вложенностью.
-- @folder_id — id исходной папки.
WITH RECURSIVE folder_hierarchy(folder_id) AS
(
    SELECT folders.id
    FROM folders
    WHERE folders.id = @folder_id

    UNION ALL

    SELECT folders.id
    FROM folders
    INNER JOIN folder_hierarchy
        ON folders.parent_folder_id = folder_hierarchy.folder_id
)
SELECT COALESCE(SUM(files.size_bytes), 0)::bigint AS total_size_bytes
FROM files
INNER JOIN folder_hierarchy
    ON files.folder_id = folder_hierarchy.folder_id;