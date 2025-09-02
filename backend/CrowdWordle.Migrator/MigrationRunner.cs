using CrowdWordle.Shared;

namespace CrowdWordle.Migrator;
public static class MigrationRunner
{
    public static void RunMigrations(DbService db)
    {
        db.ExecuteNonQuery("""
            CREATE TABLE IF NOT EXISTS Migrations (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                AppliedAt TEXT NOT NULL
            );
        """);

        var applied = new HashSet<string>();

        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "SELECT Name FROM Migrations";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            applied.Add(reader.GetString(0));
        }

        var migrationFiles = Directory.GetFiles("Data/Migrations", "*.sql")
            .OrderBy(f => f);

        foreach (var file in migrationFiles)
        {
            var name = Path.GetFileName(file);
            if (applied.Contains(name))
                continue;

            var sql = File.ReadAllText(file);
            db.ExecuteNonQuery(sql);

            db.ExecuteNonQuery(
                "INSERT INTO Migrations (Name, AppliedAt) VALUES (@name, @appliedAt)",
                ("@name", name),
                ("@appliedAt", DateTimeOffset.UtcNow.ToString("o"))
            );

            Console.WriteLine($"[MIGRATION] Applied {name}");
        }
    }
}
