-- Задание 9. Шаг 1: добавляем Materialized Path.

ALTER TABLE folders
ADD COLUMN IF NOT EXISTS materialized_path text;

ALTER TABLE files
ADD COLUMN IF NOT EXISTS materialized_path text;
