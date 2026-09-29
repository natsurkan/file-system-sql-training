-- Задание 7: месяцы строками, годы колонками.
-- В ячейках находится сумма SizeBytes файлов,
-- созданных в соответствующем году и месяце.

WITH monthly_totals AS
(
    -- Сначала обычная группировка по году и месяцу.
    SELECT
        EXTRACT(
            YEAR FROM files.created_at_utc
        )::int AS file_year,

        EXTRACT(
            MONTH FROM files.created_at_utc
        )::int AS file_month,

        SUM(files.size_bytes) AS total_size_bytes
    FROM files
    GROUP BY
        file_year,
        file_month
)
-- Затем разворачиваем годы в отдельные колонки.
SELECT
    file_month AS month,

    COALESCE(
        SUM(total_size_bytes)
            FILTER (WHERE file_year = 2025),
        0
    ) AS "2025",

    COALESCE(
        SUM(total_size_bytes)
            FILTER (WHERE file_year = 2026),
        0
    ) AS "2026",

    COALESCE(
        SUM(total_size_bytes)
            FILTER (WHERE file_year = 2027),
        0
    ) AS "2027"
FROM monthly_totals
GROUP BY file_month
ORDER BY file_month;
