-- Задание 8: папки, где количество файлов со всей вложенностью
-- больше общей медианы количества файлов по всем папкам.

WITH RECURSIVE folder_tree AS
(
    -- Каждая папка входит сама в себя.
    SELECT
        folders.id AS source_folder_id,
        folders.id AS folder_id
    FROM folders

    UNION ALL

    -- Для каждой исходной папки добавляем дочерние папки.
    SELECT
        folder_tree.source_folder_id,
        child_folders.id AS folder_id
    FROM folder_tree
    INNER JOIN folders AS child_folders
        ON child_folders.parent_folder_id =
           folder_tree.folder_id
),
folder_file_counts AS
(
    -- Для каждой исходной папки считаем файлы
    -- самой папки и всех её потомков.
    SELECT
        folders.id AS folder_id,
        folders.name AS folder_name,
        COUNT(files.id) AS file_count
    FROM folders
    LEFT JOIN folder_tree
        ON folder_tree.source_folder_id =
           folders.id
    LEFT JOIN files
        ON files.folder_id =
           folder_tree.folder_id
    GROUP BY
        folders.id,
        folders.name
),
median_file_count AS
(
    -- Получаем одну общую медиану всех file_count.
    SELECT
        percentile_cont(0.5)
            WITHIN GROUP
            (
                ORDER BY file_count
            ) AS median_count
    FROM folder_file_counts
)
-- Оставляем папки, где количество файлов
-- больше общей медианы.
SELECT
    folder_file_counts.folder_id,
    folder_file_counts.folder_name,
    folder_file_counts.file_count
FROM folder_file_counts
WHERE folder_file_counts.file_count >
(
    SELECT median_file_count.median_count
    FROM median_file_count
)
ORDER BY
    folder_file_counts.file_count DESC;
