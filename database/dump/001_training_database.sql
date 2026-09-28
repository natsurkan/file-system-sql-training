-- Учебный dump: схема и минимальные данные для тренажёра.

CREATE TABLE folders
(
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name             varchar(255) NOT NULL,
    parent_folder_id integer NULL REFERENCES folders(id) ON DELETE CASCADE,
    created_at_utc   timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_folders_parent_name UNIQUE (parent_folder_id, name)
);

CREATE TABLE files
(
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    folder_id      integer NOT NULL REFERENCES folders(id) ON DELETE CASCADE,
    name           varchar(255) NOT NULL,
    content        bytea NOT NULL,
    size_bytes     bigint NOT NULL CHECK (size_bytes >= 0),
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_files_folder_name UNIQUE (folder_id, name)
);

INSERT INTO folders (id, name, parent_folder_id, created_at_utc) OVERRIDING SYSTEM VALUE VALUES
    (1, 'usr', NULL, now() - interval '5 days'),
    (2, 'andrii', 1, now() - interval '4 days'),
    (3, 'projects', 2, now() - interval '3 days'),
    (4, 'proj1', 3, now() - interval '2 days'),
    (5, 'docs', 3, now() - interval '1 day');

INSERT INTO files (id, folder_id, name, content, size_bytes, created_at_utc) OVERRIDING SYSTEM VALUE VALUES
    (1, 4, 'readme.txt', convert_to('12345', 'UTF8'), 5, now() - interval '2 days'),
    (2, 4, 'copy.txt', convert_to('12345', 'UTF8'), 5, now() - interval '1 day'),
    (3, 5, 'notes.md', convert_to('1234567890', 'UTF8'), 10, now());

SELECT setval(pg_get_serial_sequence('folders', 'id'), (SELECT max(id) FROM folders));
SELECT setval(pg_get_serial_sequence('files', 'id'), (SELECT max(id) FROM files));