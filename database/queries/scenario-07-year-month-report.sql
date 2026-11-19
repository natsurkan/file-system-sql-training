-- Функция из миграции 005 сама получает годы и строит crosstab.
-- Сначала выполнить database/migrations/005_add_year_month_report.sql.
-- Функция создаёт временную таблицу сумм, удаляемую при завершении транзакции.
-- Для ручного запуска в SQL-клиенте:
-- BEGIN ISOLATION LEVEL REPEATABLE READ;
-- SELECT open_year_month_report('year_month_report');
-- FETCH ALL FROM year_month_report;
-- COMMIT;
SELECT open_year_month_report('year_month_report');
