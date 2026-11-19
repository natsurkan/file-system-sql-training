CREATE EXTENSION IF NOT EXISTS tablefunc;

CREATE OR REPLACE FUNCTION open_year_month_report(report_cursor refcursor)
RETURNS refcursor
LANGUAGE plpgsql
AS $function$
DECLARE
    years integer[];
    column_definitions text;
    output_columns text;
    source_sql text;
    category_sql text;
BEGIN
    -- Один проход по files: сохраняем суммы для каждой пары месяц + год.
    CREATE TEMP TABLE year_month_totals ON COMMIT DROP AS
    SELECT
        EXTRACT(MONTH FROM created_at_utc)::integer AS month_number,
        EXTRACT(YEAR FROM created_at_utc)::integer AS year,
        SUM(size_bytes) AS total_size
    FROM files
    GROUP BY 1, 2;

    -- Один список годов используется и для категорий, и для выходных колонок.
    SELECT array_agg(file_year ORDER BY file_year)
    INTO years
    FROM (
        SELECT DISTINCT year AS file_year
        FROM pg_temp.year_month_totals
    ) AS existing_years;

    IF years IS NULL THEN
        OPEN report_cursor FOR SELECT ''::text AS month WHERE false;
        RETURN report_cursor;
    END IF;

    SELECT
        string_agg(format('%I numeric', file_year::text), ', ' ORDER BY file_year),
        string_agg(format('COALESCE(%I, 0) AS %I', file_year::text, file_year::text), ', ' ORDER BY file_year)
    INTO column_definitions, output_columns
    FROM unnest(years) AS year_values(file_year);

    source_sql := $source$
        SELECT month_number, year, total_size
        FROM pg_temp.year_month_totals
        ORDER BY 1, 2
    $source$;
    category_sql := format('SELECT unnest(%L::integer[])', years::text);

    OPEN report_cursor FOR EXECUTE format(
        $report$
        SELECT to_char(make_date(2000, report.month, 1), 'FMMonth') AS month,
               %s
        FROM crosstab(%L, %L) AS report(month integer, %s)
        ORDER BY report.month
        $report$,
        output_columns, source_sql, category_sql, column_definitions);
    RETURN report_cursor;
END;
$function$;
