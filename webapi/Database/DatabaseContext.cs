using Dapper;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using Npgsql;

using OtusClasses.DataClasses.Dtos;

using OtusSocialNetwork.Database.Entities;
using OtusSocialNetwork.DataClasses.Dtos;

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
        // Create postId
        var postId = Guid.NewGuid();
        // Create post
        await using var cmd = new NpgsqlCommand(@"INSERT INTO public.posts
    (id, author_user_id, post_text, ""timestamp"")
    VALUES(@id, @userId, @text, @timestamp);
    ", con)
        {
            Parameters =
                {
                    new("id", postId),
                    new("userId", userId),
                    new("text", text),
                    new("timestamp", DateTime.UtcNow)
                }
        };

        await cmd.ExecuteNonQueryAsync();

        return (true, postId.ToString()) ;
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

    public async Task<(bool isSuccess, string msg, PostView post)> GetPost(string id)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = "select p.id as PostId," +
            "p.post_text as PostText," +
            "p.\"timestamp\" as  Timestamp," +
            "p.author_user_id as FriendId, " +
            "concat(u.first_name , ' ' , u.second_name) as FriendName" +
            " from public.posts p " +
            " inner join \"user\" u on p.author_user_id = u.id " +
            " WHERE p.id = @id";
        var item = await con.QueryFirstOrDefaultAsync<PostView>(sql, new { id });
        if (item != null)
            return (true, "OK", item);

        return (false, "Not found", null);
    }

    public async Task<(bool isSuccess, string msg, List<PostEntity> posts)> GetPosts(string userId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = """
                        SELECT
                        id as Id,
                        author_user_id as AuthorUserId,
                        post_text as Text,
                        "timestamp" as TimeStamp
            FROM public.posts
            WHERE author_user_id = @userId
            """;

        var items = await con.QueryAsync<PostEntity>(sql, new { userId = userId });
        return (true, "OK", items.ToList());
    }

    public async Task<(bool isSuccess, string msg)> UpdatePost(string id, string userId, string text)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT id from public.posts where id = @id and author_user_id = @userId";
        var postId = await con.ExecuteScalarAsync<string>(sql, new { id = id, userId = userId } );
        if (string.IsNullOrEmpty(postId)) return (false, "Not found");

        // Create account
        await using var cmd = new NpgsqlCommand(@"UPDATE public.posts
    SET  post_text=@text, timestamp=@timestamp
    WHERE id = @id and author_user_id = @userId;

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
        var sql = "SELECT id from public.posts where id = @id and author_user_id = @userId";
        var postId = await con.ExecuteScalarAsync<string>(sql, new { id = id, userId = userId});
        if (string.IsNullOrEmpty(postId)) return (false, "Not found");

        await using var cmd = new NpgsqlCommand(@"DELETE FROM public.posts 
    WHERE id = @id and author_user_id = @userId;
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

    #region Friends
    public async Task<(bool isSuccess, string msg)> AddFriend(string userId, string friendId)
    {
        await using var con = await db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"INSERT INTO public.friends
                                                    (user_id, friend_id)
                                                    VALUES(@userId,@friendId);
                                                    ;
    ", con)
        {
            Parameters =
                {
                    new("friendId", friendId),
                    new("userId", userId)
                }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");
    }

    public async Task<(bool isSuccess, string msg)> DeleteFriend(string userId, string friendId)
    {
        await using var con = await db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"DELETE FROM public.friends WHERE user_id=@userId AND friend_id=@friendId;", con)
        {
            Parameters =
                {
                    new("userId", userId),
                    new("friendId", friendId)
                }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");
        
    }

    public async Task<(bool isSuccess, string msg, List<FriendView> data)> GetFriends(string userId)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "select f.friend_id  as id, concat(u.first_name , ' ' , u.second_name) as fullname " +
            "from friends f " +
            "inner join \"user\" u on f.friend_id  = u.id " +
            "where user_id = @userId;";
        var items = await con.QueryAsync<FriendView>(sql, new { userId = userId });
        return (true, "OK", items.ToList());
    }

    public async Task<(bool isSuccess, string msg, List<FriendView> data)> SearchFriends(string query)
    {
        await using var con = await db.OpenConnectionAsync();
      
        var sql = "SELECT id, concat(first_name , ' ' , second_name) as fullname " +
            "FROM public.\"user\"" +
            "where (concat(first_name , ' ' , second_name) ilike '%" + query + "%' ) " +
            "order by concat(first_name , ' ' , second_name);";
        var items = await con.QueryAsync<FriendView>(sql, new { query = query });
        return (true, "OK", items.ToList());
    }

    public async Task<(bool isSuccess, string msg, List<PostView> posts)> GetFeed(string userId, int limit)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = """
                        select p.id as PostId,
            p.post_text as PostText,
            p."timestamp" as  Timestamp,
            f.friend_id as FriendId, 
            concat(u.first_name , ' ' , u.second_name) as FriendName
            from public.posts p 
            inner join friends f on f.friend_id  = p.author_user_id 
            inner join "user" u on f.friend_id  = u.id  
            where f.user_id  = @userId
            order by p."timestamp" desc limit @limit
            """;

        var items = await con.QueryAsync<PostView>(sql, new { userId = userId, limit = limit });
        return (true, "OK", items.ToList());
    }
    #endregion


    #region Chats

    public async Task<(bool isSuccess, string msg, List<ChatView> chats)> GetChats(string userId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = """
            select 
            c.id as ChatId, 
            c.name as ChatName,
            u.id as UserId,
            concat(u.first_name || ' ' || u.second_name  ) as UserName
            from chats c 
            inner join chat_user cu on cu.chat_id = c.id 
            inner join "user" u  on u.id = cu.user_id 
            inner join (
            	select chat_id, user_id from public.chat_user where user_id = @user_id
            ) chat_view on chat_view.chat_id = c.id and chat_view.user_id <> cu.user_id
            """;

        var items = await con.QueryAsync<ChatView>(sql, new { user_id = userId });
        return (true, "OK", items.ToList());
    }

    public async Task<(bool isSuccess, string msg)> CreateChat(string ownerId, string userId)
    {
        await using var con = await db.OpenConnectionAsync();
        // Create chatId
        var chatId = Guid.NewGuid();
        // Create chat
        await using var cmd = new NpgsqlCommand(@"INSERT INTO public.chats
            (id, ""name"")
            VALUES(@id, @name);
            ", con)
        {
            Parameters =
                {
                    new("id", chatId.ToString()),
                    new("name", $"Chat {DateTime.Now.ToShortDateString()}")
                }
        };
        await cmd.ExecuteNonQueryAsync();

        // Create Chat Members
        await using var cmd2 = new NpgsqlCommand(@"
                                        INSERT INTO public.chat_user
                                        (chat_id, user_id)
                                        VALUES(@chat_id, @user1), (@chat_id, @user2);
                                        ", con)
        {
            Parameters =
            {
                new("chat_id", chatId),
                new("user1",  ownerId),
                new("user2", userId)
            }
        };
        await cmd2.ExecuteNonQueryAsync();

        return (true, chatId.ToString());
    }

    public async Task<(bool isSuccess, string msg)> DeleteChat(string chatId, string userId)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT chat_id from public.chat_user where chat_id = @chat_id and user_id = @userId";
        var id = await con.ExecuteScalarAsync<string>(sql, new { chat_id = chatId, user_id = userId });
        if (string.IsNullOrEmpty(id)) return (false, "Not found");

        await using var cmd = new NpgsqlCommand(@"DELETE FROM public.chats WHERE id=@chat_id;", con)
        {
            Parameters =              {    new("chat_id", chatId)                }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");
    }

    public async Task<(bool isSuccess, string msg, List<ChatMessageView> messages)> GetMessages(string userId, string chatId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = """
            SELECT 
            m.id, 
            m.chat_id as ChatId, 
            m.user_id as UserId, 
            concat(u.first_name || ' ' || u.second_name  ) as UserName,
            message_text as MessageText, 
            is_new as IsNew, 
            "timestamp" as Timestamp
            FROM public.messages m
            inner join public."user" u on u.id = m.user_id 
            inner join chat_user cu on cu.chat_id  = m.chat_id 
            where cu.user_id = @user_id
            order by "timestamp";
            """;

        var items = await con.QueryAsync<ChatMessageView>(sql, new { user_id = userId });
        return (true, "OK", items.ToList());
    }

    public async Task<(bool isSuccess, string msg)> CreateMessage(string chatId, string userId, string message)
    {
        await using var con = await db.OpenConnectionAsync();
        // Create chatId
        var messageId = Guid.NewGuid();
        // Create chat
        await using var cmd = new NpgsqlCommand(@"INSERT INTO public.messages
            (id, chat_id, user_id, message_text, is_new, ""timestamp"")
            VALUES(@id, @chatId, @userId, @message, true, @date);
            ", con)
        {
            Parameters =
                {
                    new("id", messageId),
                    new("chatId", chatId),
                    new("userId", userId),
                    new("message", message),
                    new("date", DateTime.Now)
                }
        };
        await cmd.ExecuteNonQueryAsync();

        return (true, messageId.ToString());
    }

    public async Task<(bool isSuccess, string msg)> DeleteMessage(string chatId, string userId, string messageId)
    {
        await using var con = await db.OpenConnectionAsync();
        var sql = "SELECT chat_id from public.chat_user where chat_id = @chat_id and user_id = @user_id";
        var id = await con.ExecuteScalarAsync<string>(sql, new { chat_id = chatId, user_id = userId });
        if (string.IsNullOrEmpty(id)) return (false, "Not found");

        await using var cmd = new NpgsqlCommand(@"DELETE FROM public.messages WHERE id=@message_id and chat_id = @chat_id;", con)
        {
            Parameters = { new("chat_id", chatId), new ("message_id", messageId) }
        };

        await cmd.ExecuteNonQueryAsync();
        return (true, "OK");
    }

    public async Task<(bool isSuccess, string msg, ChatView chat)> GetChat(string chatId, string userId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = """
            select
            c.id as ChatId, 
            c.name as ChatName,
            u.id as UserId,
            concat(u.first_name || ' ' || u.second_name  ) as UserName
            from chats c 
            inner join chat_user cu on cu.chat_id = c.id 
            inner join "user" u  on u.id = cu.user_id 
            inner join (
            	select chat_id, user_id from public.chat_user where user_id = @user_id and chat_id = @chat_id
            ) chat_view on chat_view.chat_id = c.id and chat_view.user_id <> cu.user_id
            """;

        var items = await con.QueryAsync<ChatView>(sql, new { chat_id = chatId, user_id = userId });
        return (true, "OK", items.FirstOrDefault());
    }


    #endregion

    #region Dialog
    public async Task<(bool isSuccess, string msg)> SendDialogMessage(string fromId, string toId, string message)
    {
        await using var con = await db.OpenConnectionAsync();
        // Create chatId
        var messageId = Guid.NewGuid();
        // Create chat
        await using var cmd = new NpgsqlCommand("""
            INSERT INTO public.messages
            (id, from_user_id, to_user_id, message_text, "timestamp")
            VALUES(@id, @fromId, @toId, @message, @timestamp);
            
            """, con)
        {
            Parameters =
                {
                    new("id", messageId),
                    new("fromId", fromId),
                    new("toId", toId),
                    new("message", message),
                    new("timestamp", DateTime.UtcNow)
                }
        };
        await cmd.ExecuteNonQueryAsync();

        return (true, messageId.ToString());
    }

    public async Task<(bool isSuccess, string msg, List<DialogMessageDTO> messages)> GetDialogMessages(string fromId, string toId)
    {
        await using var con = await db.OpenConnectionAsync();

        var sql = """
            SELECT id, from_user_id as FromUserId, to_user_id as ToUserId, message_text as MessageText, "timestamp" as Timestamp
            FROM public.messages
            where 
            (from_user_id = @user1 and to_user_id = @user2)
            or 
            (from_user_id = @user2 and to_user_id = @user1)
            order by "timestamp" 
            """;

        var items = await con.QueryAsync<DialogMessageEntity>(sql, new { @user1 = fromId, @user2 = toId });
        
        var res = items.Select(o => new DialogMessageDTO(o.FromUserId.ToString(), o.ToUserId.ToString(), o.MessageText, o.Timestamp)).ToList();
        return (true, "OK", res);
    }
    #endregion
}
