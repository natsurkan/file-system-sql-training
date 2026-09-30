-- Проверяем заполненные Materialized Path для папок и файлов.

SELECT
    'folder' AS object_type,
    folders.id AS object_id,
    folders.name AS object_name,
    folders.materialized_path
FROM folders

UNION ALL

SELECT
    'file' AS object_type,
    files.id AS object_id,
    files.name AS object_name,
    files.materialized_path
FROM files

ORDER BY
    object_type,
    object_id;
