-- Одноразовая миграция: делаем files партиционированной по created_at_utc.
-- В PostgreSQL уникальный ключ на партиционированной таблице обязан включать
-- ключ партиционирования. Поэтому прежнее правило уникальности (folder_id, name)
-- переносится в trigger ниже.

ALTER TABLE files RENAME TO files_before_partitioning;

CREATE TABLE files
(
    id             integer GENERATED ALWAYS AS IDENTITY,
    folder_id      integer NOT NULL REFERENCES folders(id) ON DELETE CASCADE,
    name           varchar(255) NOT NULL,
    content        bytea NOT NULL,
    size_bytes     bigint NOT NULL CHECK (size_bytes >= 0),
    created_at_utc timestamptz NOT NULL DEFAULT now(),
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
    created_at_utc)
OVERRIDING SYSTEM VALUE
SELECT
    id,
    folder_id,
    name,
    content,
    size_bytes,
    created_at_utc
FROM files_before_partitioning;

SELECT setval(
    pg_get_serial_sequence('files', 'id'),
    COALESCE((SELECT MAX(id) FROM files), 1),
    true);

DROP TABLE files_before_partitioning;

CREATE INDEX ix_files_created_at_utc
    ON files (created_at_utc);

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
