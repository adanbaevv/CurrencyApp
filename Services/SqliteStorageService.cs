using System.IO;
using CurrencyApp.Models;
using Microsoft.Data.Sqlite;

namespace CurrencyApp.Services;

public class SqliteStorageService : IStorageService
{
    private readonly string _connectionString;

    public SqliteStorageService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CurrencyApp"
        );

        Directory.CreateDirectory(folder);
        var dbPath = Path.Combine(folder, "currencies.db");
        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // SQLite has no native decimal — store as TEXT to avoid float rounding
        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Currencies (
                Id TEXT PRIMARY KEY,
                NumCode TEXT,
                CharCode TEXT NOT NULL,
                Nominal INTEGER NOT NULL,
                Name TEXT,
                Value TEXT NOT NULL,
                Previous TEXT NOT NULL,
                IsUserAdded INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public async Task<List<Currency>> LoadAsync()
    {
        var result = new List<Currency>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, NumCode, CharCode, Nominal, Name, Value, Previous, IsUserAdded FROM Currencies";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Currency
            {
                Id = reader.IsDBNull(0) ? null : reader.GetString(0),
                NumCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                CharCode = reader.GetString(2),
                Nominal = reader.GetInt32(3),
                Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                Value = decimal.Parse(reader.GetString(5), System.Globalization.CultureInfo.InvariantCulture),
                Previous = decimal.Parse(reader.GetString(6), System.Globalization.CultureInfo.InvariantCulture),
                IsUserAdded = reader.GetInt32(7) == 1
            });
        }

        return result;
    }

    public async Task SaveAsync(List<Currency> currencies)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Replace all entries: simpler than diffing, fine for our scale (~50-100 rows)
        await using var transaction = await connection.BeginTransactionAsync();

        var clearCommand = connection.CreateCommand();
        clearCommand.CommandText = "DELETE FROM Currencies";
        await clearCommand.ExecuteNonQueryAsync();

        foreach (var currency in currencies)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO Currencies (Id, NumCode, CharCode, Nominal, Name, Value, Previous, IsUserAdded)
                VALUES ($id, $numCode, $charCode, $nominal, $name, $value, $previous, $isUserAdded)
                """;

            insertCommand.Parameters.AddWithValue("$id", (object?)currency.Id ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("$numCode", (object?)currency.NumCode ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("$charCode", currency.CharCode ?? "");
            insertCommand.Parameters.AddWithValue("$nominal", currency.Nominal);
            insertCommand.Parameters.AddWithValue("$name", (object?)currency.Name ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("$value", currency.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            insertCommand.Parameters.AddWithValue("$previous", currency.Previous.ToString(System.Globalization.CultureInfo.InvariantCulture));
            insertCommand.Parameters.AddWithValue("$isUserAdded", currency.IsUserAdded ? 1 : 0);

            await insertCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}