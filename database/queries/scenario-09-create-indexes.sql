-- Задание 9. Шаг 4: создаём индексы Materialized Path.

CREATE INDEX IF NOT EXISTS ix_folders_materialized_path
    ON folders (materialized_path text_pattern_ops);

CREATE INDEX IF NOT EXISTS ix_files_materialized_path
    ON files (materialized_path text_pattern_ops);
