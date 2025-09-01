namespace CrowdWordle.Data;
using Microsoft.Data.Sqlite;
public sealed class DbService : IDisposable
{
    private readonly SqliteConnection _connection;
    public SqliteConnection Connection => _connection;

    public DbService(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        OptimizeConnection();
    }

    private void OptimizeConnection()
    {
        ExecuteNonQuery("PRAGMA journal_mode=WAL");
        ExecuteNonQuery("PRAGMA synchronous=NORMAL");
        ExecuteNonQuery("PRAGMA cache_size=10000");
        ExecuteNonQuery("PRAGMA temp_store=MEMORY");
        ExecuteNonQuery("PRAGMA mmap_size=268435456");
        ExecuteNonQuery("PRAGMA optimize");
    }

    public void ExecuteNonQuery(string sql, params (string name, object value)[] parameters)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        AddParameters(cmd, parameters);
        cmd.ExecuteNonQuery();
    }

    public async Task ExecuteNonQueryAsync(string sql, params (string name, object value)[] parameters)
    {
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        AddParameters(cmd, parameters);
        await cmd.ExecuteNonQueryAsync();
    }

    public T? QuerySingle<T>(string sql, Func<SqliteDataReader, T> map, params (string name, object value)[] parameters)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        AddParameters(cmd, parameters);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return map(reader);

        return default;
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Func<SqliteDataReader, T> map, params (string name, object value)[] parameters)
    {
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        AddParameters(cmd, parameters);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return map(reader);

        return default;
    }

    private static void AddParameters(SqliteCommand cmd, params (string name, object value)[] parameters)
    {
        foreach (var (name, value) in parameters)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }

    public void Dispose() => _connection.Dispose();
}