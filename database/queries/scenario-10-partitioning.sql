-- После миграции files остаётся одной логической таблицей.
-- tableoid показывает физическую партицию, в которой хранится строка.
-- Условие по created_at_utc позволяет PostgreSQL читать только нужный год.
-- {0} и {1} — значения FormattableString; перед выполнением они превращаются
-- в параметры @p0 и @p1, а не вставляются в текст SQL.

SELECT
    tableoid::regclass::text AS physical_partition,
    COUNT(*) AS file_count,
    COALESCE(SUM(size_bytes), 0) AS total_size_bytes
FROM files
WHERE created_at_utc >= {0}
  AND created_at_utc < {1}
GROUP BY tableoid
ORDER BY tableoid::regclass::text;


