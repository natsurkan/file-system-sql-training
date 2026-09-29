using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Resources;
using Npgsql;
using NpgsqlTypes;

namespace FileSystem.ConsoleApp.Import;

public sealed class DatabaseImporter
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ScenarioResourceReader _resources;

    public DatabaseImporter(
        IDbConnectionFactory connectionFactory,
        ScenarioResourceReader resources)
    {
        _connectionFactory = connectionFactory;
        _resources = resources;
    }

    public async Task<int> ImportAsync(
        ImportedFolder rootFolder,
        CancellationToken cancellationToken = default)
    {
        var insertFolderSql = await _resources.ReadSqlAsync(
            "scenario-03-insert-folder.sql");

        var insertFileSql = await _resources.ReadSqlAsync(
            "scenario-03-insert-file.sql");

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var rootFolderId = await SaveFolderAsync(
                rootFolder,
                parentFolderId: null,
                insertFolderSql,
                insertFileSql,
                connection,
                transaction,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return rootFolderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> SaveFolderAsync(
        ImportedFolder folder,
        int? parentFolderId,
        string insertFolderSql,
        string insertFileSql,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var folderId = await InsertFolderAsync(
            folder.Name,
            parentFolderId,
            insertFolderSql,
            connection,
            transaction,
            cancellationToken);

        foreach (var file in folder.Files)
        {
            await InsertFileAsync(
                file,
                folderId,
                insertFileSql,
                connection,
                transaction,
                cancellationToken);
        }

        foreach (var childFolder in folder.ChildFolders)
        {
            await SaveFolderAsync(
                childFolder,
                folderId,
                insertFolderSql,
                insertFileSql,
                connection,
                transaction,
                cancellationToken);
        }

        return folderId;
    }

    private static async Task<int> InsertFolderAsync(
        string folderName,
        int? parentFolderId,
        string sql,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            sql,
            connection,
            transaction);

        command.Parameters.AddWithValue("name", folderName);
        command.Parameters.Add(new NpgsqlParameter(
            "parent_folder_id",
            NpgsqlDbType.Integer)
        {
            Value = parentFolderId.HasValue
                ? parentFolderId.Value
                : DBNull.Value
        });
        command.Parameters.AddWithValue(
            "created_at_utc",
            DateTime.UtcNow);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static async Task InsertFileAsync(
        ImportedFile file,
        int folderId,
        string sql,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            sql,
            connection,
            transaction);

        command.Parameters.AddWithValue("folder_id", folderId);
        command.Parameters.AddWithValue("name", file.Name);
        command.Parameters.Add(new NpgsqlParameter(
            "content",
            NpgsqlDbType.Bytea)
        {
            Value = file.Content
        });
        command.Parameters.AddWithValue("size_bytes", file.SizeBytes);
        command.Parameters.AddWithValue(
            "created_at_utc",
            DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
