using Dapper;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using Npgsql;

using OtusSocialNetwork.Database.Entities;

using System.Data;
using static System.Net.Mime.MediaTypeNames;

namespace OtusSocialNetwork.Database;

public class DatabaseContext : IDatabaseContext, IDisposable
{
    public DatabaseContext(IOptions<DatabaseSettings> settings)
    {
        connStr = settings.Value.ConnStr;
        // connStrReplica = settings.Value.ConnStrReplica;

        db = NpgsqlDataSource.Create(connStr);
        // dbReplica = NpgsqlDataSource.Create(connStrReplica);
    }
    private readonly string connStr;
    // private readonly string connStrReplica;
    private readonly NpgsqlDataSource db;
    // private readonly NpgsqlDataSource dbReplica;
    public async void Dispose()
    {
        await this.db.DisposeAsync();
        // await this.dbReplica.DisposeAsync();
    }

    #region Accounts
    public async Task<(bool isSuccess, string msg, AccountEntity? account)> GetLoginAsync(string id)
    {
        await using var con = await db.OpenConnectionAsync();
        await using var cmd = db.CreateCommand("SELECT id, \"password\" FROM public.account WHERE id = @id LIMIT 1;");
        var sql = "SELECT id, \"password\" FROM public.account WHERE id = @id LIMIT 1;";
        var items = con.Query<AccountEntity>(sql, new { id = id });
        if (items.Count() > 0) { return (true, "OK", items.First()); }

        return (false, "Not found", null);
    }

    public async Task<(bool isSuccess, string msg, string userId)> AddNewTableRecordAsync(NewTableEntity user)
    {
        // Check connection to master 
        try
        {
            await using var con = await db.OpenConnectionAsync();
            await using var cmdAccount = new NpgsqlCommand("INSERT INTO public.newtable\r\n(person, age, city)\r\nVALUES(@person, @age, @city);\r\n", con)
            {
                Parameters =    {
                    new("person", user.Person),
                    new("age", user.Age),
                    new("city", user.City)
                }
            };
            await cmdAccount.ExecuteNonQueryAsync();
            return (true, "OK", user.Person);
        }
        catch (Exception ex)
        {
            await using var con = await db.OpenConnectionAsync();
            await using var cmdAccount = new NpgsqlCommand("INSERT INTO public.newtable\r\n(person, age, city)\r\nVALUES(@person, @age, @city);\r\n", con)
            {
                Parameters =    {
                    new("person", user.Person),
                    new("age", user.Age),
                    new("city", user.City)
                }
            };
            await cmdAccount.ExecuteNonQueryAsync();
            return (true, "OK", user.Person);
        }
    }
    public async Task<(bool isSuccess, string msg, string userId)> RegisterAsync(UserEntity user, string password)
    {
        await using var con = await db.OpenConnectionAsync();
        // Create account
        await using var cmdAccount = new NpgsqlCommand("INSERT INTO public.account\r\n(id, \"password\")\r\nVALUES(@id, @password);\r\n", con)
        {
            Parameters =    {
                    new("id", user.Id),
                    new("password", password)
                }
        };
        await cmdAccount.ExecuteNonQueryAsync();


        // Create user
        await using var cmdUser = new NpgsqlCommand("INSERT INTO public.\"user\"\r\n(id, first_name, second_name, sex, age, city, biography)\r\n" +
            "VALUES(@id, @firstname, @secondname, @sex, @age, @city, @biography);\r\n", con)
        {
            Parameters =    {
                    new("id", user.Id),
                    new("firstname", user.First_name),
                    new("secondname", user.Second_name),
                    new("sex", user.Sex),
                    new("age", user.Age),
                    new("city", user.City),
                    new("biography", user.Biography)
                }
        };
        await cmdUser.ExecuteNonQueryAsync();

        return (true, "OK", user.Id);
    }

    private async Task<bool> IsAccountExists(string id)
    {
        var res = false;
        await using var con = await db.OpenConnectionAsync();
        await using var cmd = db.CreateCommand("SELECT EXISTS(SELECT id, \"password\" FROM public.account WHERE id = @id);");
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            res = rdr.GetBoolean(0);
        }
        return res;
    }

    public async Task<(bool isSuccess, string msg, UserEntity user)> GetUserAsync(string id)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT id, first_name, second_name, sex, age, city, biography\r\nFROM public.\"user\"\r\n WHERE id = @id LIMIT 1;";
        var items = con.Query<UserEntity>(sql, new { id = id });
        if (items.Count() > 0) { return (true, "OK", items.First()); }

        return (false, "Not found", null);
    }

    public async Task<(bool isSuccess, string msg, List<UserEntity> users)> SearchUserAsync(string firstName, string lastName)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT id, first_name, second_name, sex, age, city, biography\r\nFROM public.\"user\"\r\n";
        var sqlConditions = new List<string>();
        IEnumerable<UserEntity> items;
        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        {
            sql += "WHERE first_name LIKE @firstname AND second_name LIKE @secondname ORDER BY id;";
            items = con.Query<UserEntity>(sql, new
            {
                @firstname = $"{firstName}%",
                @secondname = $"{lastName}%",
            });
        }
        else if (!string.IsNullOrEmpty(firstName))
        {
            sql += "WHERE first_name LIKE @firstname ORDER BY id;";
            items = con.Query<UserEntity>(sql, new
            {
                @firstname = $"{firstName}%"
            });
        }
        else if (!string.IsNullOrEmpty(lastName))
        {
            sql += "WHERE second_name LIKE @secondname ORDER BY id;";
            items = con.Query<UserEntity>(sql, new
            {
                @secondname = $"{lastName}%"
            });
        }
        else
        {
            sql += " ORDER BY id LIMIT 100;";
            items = con.Query<UserEntity>(sql);
        }

        return (true, "OK", items.ToList());
    }
    #endregion

    #region Posts
    public async Task<(bool isSuccess, string msg)> CreatePost(string text, string userId)
    {
        await using var con = await db.OpenConnectionAsync();
        // Create account
        await using var cmd = new NpgsqlCommand(@"INSERT INTO public.posts
    (id, author_user_id, post_text, ""timestamp"")
    VALUES(@id, @userId, @text, @timestamp);
    ", con)
        {
            Parameters =
                {
                    new("id", Guid.NewGuid()),
                    new("userId", userId),
                    new("text", text),
                    new("timestamp", DateTime.UtcNow)
                }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");
    }
    public async Task<(bool isSuccess, string msg, PostEntity post)> GetPost(string id, string userId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = "SELECT id, author_user_id, post_text, \"timestamp\"\nFROM public.posts where author_user_id = @userId and id = @id;\n";
        var item = await con.QueryFirstOrDefaultAsync<PostEntity>(sql, new { userId = userId, id = id });
        if (item != null)
        return (true, "OK", item);

        return (false, "Not found", null);
    }

    public async Task<(bool isSuccess, string msg, List<PostEntity> posts)> GetPosts(string userId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = "SELECT id, author_user_id, post_text, \"timestamp\"\nFROM public.posts where author_user_id = @userId;\n";
        var items = await con.QueryAsync<PostEntity>(sql, new { userId = userId });
        return (true, "OK", items.ToList());
    }

    public async Task<(bool isSuccess, string msg)> UpdatePost(string id, string userId, string text)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT id from pulic.posts where id = @id and author_user_id = @userId";
        var postId = await con.ExecuteScalarAsync<string>(sql);
        if (string.IsNullOrEmpty(postId)) return (false, "Not found");

        // Create account
        await using var cmd = new NpgsqlCommand(@"UPDATE public.posts
    SET  post_text=@text, ""timestamp""=@timestamp
    WHERE id = @id and account_user_id = @userId;

    ", con)
        {
            Parameters =
                {
                    new("id", id),
                    new("userId", userId),
                    new("text", text),
                    new("timestamp", DateTime.UtcNow)
                }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");

    }

    public async Task<(bool isSuccess, string msg)> DeletePost(string id, string userId)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT id from pulic.posts where id = @id and author_user_id = @userId";
        var postId = await con.ExecuteScalarAsync<string>(sql);
        if (string.IsNullOrEmpty(postId)) return (false, "Not found");

        await using var cmd = new NpgsqlCommand(@"DELETE FROM public.posts 
    WHERE id = @id and account_user_id = @userId;
    ", con)
        {
            Parameters =
                {
                    new("id", id),
                    new("userId", userId)
                }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");
    }
#endregion
}
