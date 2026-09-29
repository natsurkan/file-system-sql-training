-- Задание 5: сравнение количества файлов сегодня и вчера.
-- Считаются файлы заданной папки и всех её вложенных папок.
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
),
folder_files AS
(
    SELECT
        files.id,
        files.created_at_utc
    FROM files
    INNER JOIN folder_hierarchy
        ON files.folder_id = folder_hierarchy.folder_id
),
time_boundaries AS
(
    SELECT
        date_trunc(
            'day',
            now() AT TIME ZONE 'UTC'
        ) AT TIME ZONE 'UTC' AS today_start_utc
),
file_counts AS
(
    SELECT
        COUNT(*) FILTER
        (
            WHERE folder_files.created_at_utc >=
                  time_boundaries.today_start_utc
        ) AS today_count,

        COUNT(*) FILTER
        (
            WHERE folder_files.created_at_utc >=
                  time_boundaries.today_start_utc
                  - INTERVAL '1 day'
              AND folder_files.created_at_utc <
                  time_boundaries.today_start_utc
        ) AS yesterday_count
    FROM folder_files
    CROSS JOIN time_boundaries
)
SELECT
    today_count,
    yesterday_count,
    today_count - yesterday_count AS difference,
    CASE
        WHEN yesterday_count = 0 THEN NULL
        ELSE
            (today_count - yesterday_count) * 100.0
            / yesterday_count
    END AS percentage_change
FROM file_counts;
