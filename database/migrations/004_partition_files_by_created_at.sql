-- Одноразовая миграция: делаем files партиционированной по created_at_utc.
-- В PostgreSQL уникальный ключ на партиционированной таблице обязан включать
-- ключ партиционирования. Поэтому прежнее правило уникальности (folder_id, name)
-- переносится в trigger ниже.

-- Сценарий партиционирования может запускаться как до, так и после сценария
-- Materialized Path. Если столбца ещё нет, создаём его пустым; если он уже есть,
-- сохранённые пути будут перенесены в новую таблицу ниже.
ALTER TABLE files
    ADD COLUMN IF NOT EXISTS materialized_path text;

ALTER TABLE files RENAME TO files_before_partitioning;

CREATE TABLE files
(
    id             integer GENERATED ALWAYS AS IDENTITY,
    folder_id      integer NOT NULL REFERENCES folders(id) ON DELETE CASCADE,
    name           varchar(255) NOT NULL,
    content        bytea NOT NULL,
    size_bytes     bigint NOT NULL CHECK (size_bytes >= 0),
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    materialized_path text,
    PRIMARY KEY (id, created_at_utc)
) PARTITION BY RANGE (created_at_utc);

CREATE TABLE files_2025
    PARTITION OF files
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');

CREATE TABLE files_2026
    PARTITION OF files
    FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');

CREATE TABLE files_2027
    PARTITION OF files
    FOR VALUES FROM ('2027-01-01') TO ('2028-01-01');

-- Временная защита для дат вне учебных границ.
-- Перед началом 2028 года вместо неё нужно создать files_2028.
CREATE TABLE files_other_dates
    PARTITION OF files DEFAULT;

INSERT INTO files (
    id,
    folder_id,
    name,
    content,
    size_bytes,
    created_at_utc,
    materialized_path)
OVERRIDING SYSTEM VALUE
SELECT
    id,
    folder_id,
    name,
    content,
    size_bytes,
    created_at_utc,
    materialized_path
FROM files_before_partitioning;

SELECT setval(
    pg_get_serial_sequence('files', 'id'),
    COALESCE((SELECT MAX(id) FROM files), 1),
    true);

DROP TABLE files_before_partitioning;

CREATE INDEX ix_files_created_at_utc
    ON files (created_at_utc);

-- Ускоряет проверку (folder_id, name) в trigger во всех партициях.
-- UNIQUE здесь быть не может без created_at_utc, поэтому глобальное правило
-- уникальности по-прежнему обеспечивает trigger ниже.
CREATE INDEX ix_files_folder_name
    ON files (folder_id, name);

CREATE INDEX ix_files_materialized_path
    ON files (materialized_path text_pattern_ops);

CREATE OR REPLACE FUNCTION prevent_duplicate_child_name()
RETURNS trigger AS $$
BEGIN
    IF TG_TABLE_NAME = 'folders' THEN
        IF EXISTS
        (
            SELECT 1
            FROM files
            WHERE folder_id = NEW.parent_folder_id
              AND name = NEW.name
        ) THEN
            RAISE EXCEPTION
                'A file with this name already exists in the parent folder';
        END IF;
    ELSE
        IF EXISTS
        (
            SELECT 1
            FROM folders
            WHERE id = NEW.folder_id
              AND name = NEW.name
        ) THEN
            RAISE EXCEPTION
                'A folder with this name already exists in the parent folder';
        END IF;

        IF EXISTS
        (
            SELECT 1
            FROM files
            WHERE folder_id = NEW.folder_id
              AND name = NEW.name
        ) THEN
            RAISE EXCEPTION
                'A file with this name already exists in the parent folder';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_files_name ON files;
CREATE TRIGGER trg_files_name
    BEFORE INSERT OR UPDATE OF folder_id, name ON files
    FOR EACH ROW
    EXECUTE FUNCTION prevent_duplicate_child_name();
