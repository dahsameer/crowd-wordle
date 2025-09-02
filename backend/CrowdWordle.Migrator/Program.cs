using CrowdWordle.Migrator;
using CrowdWordle.Shared;

var dbPath = Environment.GetEnvironmentVariable("WordleDbConnection")
             ?? "/var/lib/crowdwordle/game.db";

var connectionString = $"Data Source={dbPath};Cache=Shared;";
var dbService = new DbService(connectionString);
MigrationRunner.RunMigrations(dbService);