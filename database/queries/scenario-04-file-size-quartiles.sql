-- Задание 4: первый, второй и третий квартиль размеров файлов.
-- Берутся файлы заданной папки и всех её вложенных папок.
-- @folder_id передаётся из C# через NpgsqlParameter.

WITH RECURSIVE folder_hierarchy(folder_id) AS
(
    SELECT folders.id
    FROM folders
    WHERE folders.id = @folder_id

    UNION ALL

    SELECT child_folders.id
    FROM folders AS child_folders
    INNER JOIN folder_hierarchy
        ON child_folders.parent_folder_id =
           folder_hierarchy.folder_id
)
SELECT
    percentile_cont(0.25)
        WITHIN GROUP (ORDER BY files.size_bytes)
        AS first_quartile,

    percentile_cont(0.50)
        WITHIN GROUP (ORDER BY files.size_bytes)
        AS second_quartile,

    percentile_cont(0.75)
        WITHIN GROUP (ORDER BY files.size_bytes)
        AS third_quartile
FROM files
INNER JOIN folder_hierarchy
    ON files.folder_id = folder_hierarchy.folder_id;
