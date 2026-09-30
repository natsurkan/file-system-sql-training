-- После миграции files остаётся одной логической таблицей.
-- tableoid показывает физическую партицию, в которой хранится строка.
-- Условие по created_at_utc позволяет PostgreSQL читать только нужный год.

SELECT
    tableoid::regclass::text AS physical_partition,
    COUNT(*) AS file_count,
    COALESCE(SUM(size_bytes), 0) AS total_size_bytes
FROM files
WHERE created_at_utc >= @period_start
  AND created_at_utc < @period_end
GROUP BY tableoid
ORDER BY tableoid::regclass::text;


