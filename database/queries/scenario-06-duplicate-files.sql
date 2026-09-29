-- Задание 6: поиск файлов с одинаковым содержимым.
-- Дубликаты ищутся среди всех файлов таблицы Files.

WITH duplicate_contents AS
(
    SELECT
        content,
        COUNT(*) AS duplicate_count
    FROM files
    GROUP BY content
    HAVING COUNT(*) > 1
)
SELECT
    files.name,
    duplicate_contents.duplicate_count
FROM files
INNER JOIN duplicate_contents
    ON files.content = duplicate_contents.content
ORDER BY
    duplicate_contents.duplicate_count DESC,
    files.name;
