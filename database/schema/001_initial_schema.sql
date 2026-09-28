CREATE TABLE IF NOT EXISTS folders
(
    id              integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(255) NOT NULL,
    parent_folder_id integer NULL REFERENCES folders(id) ON DELETE CASCADE,
    created_at_utc  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_folders_parent_name UNIQUE (parent_folder_id, name)
);

CREATE TABLE IF NOT EXISTS files
(
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    folder_id      integer NOT NULL REFERENCES folders(id) ON DELETE CASCADE,
    name           varchar(255) NOT NULL,
    content        bytea NOT NULL,
    size_bytes     bigint NOT NULL CHECK (size_bytes >= 0),
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_files_folder_name UNIQUE (folder_id, name)
);

-- Cross-table name uniqueness (file and folder cannot share a name in one folder)
CREATE OR REPLACE FUNCTION prevent_duplicate_child_name() RETURNS trigger AS $$
BEGIN
    IF TG_TABLE_NAME = 'folders' AND EXISTS (SELECT 1 FROM files WHERE folder_id = NEW.parent_folder_id AND name = NEW.name) THEN
        RAISE EXCEPTION 'A file with this name already exists in the parent folder';
    ELSIF TG_TABLE_NAME = 'files' AND EXISTS (SELECT 1 FROM folders WHERE id = NEW.folder_id AND name = NEW.name) THEN
        RAISE EXCEPTION 'A folder with this name already exists in the parent folder';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_folders_name ON folders;
CREATE TRIGGER trg_folders_name BEFORE INSERT OR UPDATE OF parent_folder_id, name ON folders FOR EACH ROW EXECUTE FUNCTION prevent_duplicate_child_name();
DROP TRIGGER IF EXISTS trg_files_name ON files;
CREATE TRIGGER trg_files_name BEFORE INSERT OR UPDATE OF folder_id, name ON files FOR EACH ROW EXECUTE FUNCTION prevent_duplicate_child_name();
