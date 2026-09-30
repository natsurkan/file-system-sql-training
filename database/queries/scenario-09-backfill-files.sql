-- Задание 9. Шаг 3: заполняем Materialized Path для всех файлов.

UPDATE files
SET materialized_path =
    folders.materialized_path
    || '/'
    || files.name
FROM folders
WHERE folders.id = files.folder_id;
