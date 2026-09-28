INSERT INTO folders (id, name, parent_folder_id, created_at_utc) OVERRIDING SYSTEM VALUE VALUES
    (1, 'usr', NULL, now() - interval '5 days'),
    (2, 'andrii', 1, now() - interval '4 days'),
    (3, 'projects', 2, now() - interval '3 days'),
    (4, 'proj1', 3, now() - interval '2 days'),
    (5, 'docs', 3, now() - interval '1 day')
ON CONFLICT (id) DO NOTHING;

INSERT INTO files (id, folder_id, name, content, size_bytes, created_at_utc) OVERRIDING SYSTEM VALUE VALUES
    (1, 4, 'readme.txt', convert_to('12345', 'UTF8'), 5, now() - interval '2 days'),
    (2, 4, 'copy.txt', convert_to('12345', 'UTF8'), 5, now() - interval '1 day'),
    (3, 5, 'notes.md', convert_to('1234567890', 'UTF8'), 10, now())
ON CONFLICT (id) DO NOTHING;
