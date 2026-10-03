using Npgsql;

public static class DbHelper
{
    // Used when no database is configured (and to seed an empty database).
    public static readonly List<MenuItem> DefaultMenu = new()
    {
        new(1, "Chicken Kottu", 850m, "Mains"),
        new(2, "Vegetable Fried Rice", 650m, "Mains"),
        new(3, "Egg Hopper", 120m, "Snacks"),
        new(4, "Fish Cutlet", 90m, "Snacks"),
        new(5, "Iced Milo", 300m, "Drinks"),
    };

    // Accepts either a normal Npgsql connection string or a
    // postgres://user:password@host/db URL (the format Neon shows).
    public static string Normalize(string raw)
    {
        if (!raw.StartsWith("postgres://") && !raw.StartsWith("postgresql://"))
        {
            return raw;
        }

        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require,
        };
        return builder.ConnectionString;
    }

    // Creates the table and seeds it when empty (same as db/schema.sql).
    public static async Task EnsureSchemaAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using (var create = new NpgsqlCommand(
            @"CREATE TABLE IF NOT EXISTS menu_items (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                price NUMERIC(10,2) NOT NULL,
                category TEXT NOT NULL)", conn))
        {
            await create.ExecuteNonQueryAsync();
        }

        long count;
        await using (var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM menu_items", conn))
        {
            count = (long)(await countCmd.ExecuteScalarAsync())!;
        }

        if (count == 0)
        {
            foreach (var item in DefaultMenu)
            {
                await using var insert = new NpgsqlCommand(
                    "INSERT INTO menu_items (name, price, category) VALUES (@n, @p, @c)", conn);
                insert.Parameters.AddWithValue("n", item.Name);
                insert.Parameters.AddWithValue("p", item.Price);
                insert.Parameters.AddWithValue("c", item.Category);
                await insert.ExecuteNonQueryAsync();
            }
        }
    }

    public static async Task<List<MenuItem>> GetMenuAsync(string connectionString)
    {
        var items = new List<MenuItem>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, price, category FROM menu_items ORDER BY id", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new MenuItem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetString(3)));
        }
        return items;
    }
}
