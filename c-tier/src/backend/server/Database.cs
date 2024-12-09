using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using System.Data;
using c_tier.src;
using System.Threading.Channels;
using System.Drawing.Printing;

namespace c_tier.src.backend.server
{
    public static class Database
    {
        private static SQLiteConnection dbConnection;

        public static SQLiteConnection InitDatabase(string dbPath)
        {
            var connectionString = $"Data Source={dbPath};Version=3;";
            dbConnection = new SQLiteConnection(connectionString);
            dbConnection.Open();
            Console.WriteLine(Utils.GREEN+"DATABASE: Up and running!");
            CreateTables();
            Console.WriteLine(Utils.GREEN+"DATABASE: Tables created!");
            return dbConnection;
        }

        private static void ExecuteNonQuery(string sql)
        {
            using var command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }

        private static void CreateTables()
        {
            CreateUsersTable();
            CreateChannelsTable();
            CreateMessageTable();
            CreateUserChannelsTable();
            CreateDMsTable();
            CreateDirectMessagesTable();
            CreateRolesTable();
            CreateUserRolesTable();
        }

        private static void CreateUsersTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS users (
                userid INTEGER PRIMARY KEY,
                name TEXT UNIQUE,
                timestamp INTEGER,
                password TEXT
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created users table.");
        }

        private static void CreateChannelsTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS channels (
                channelid INTEGER PRIMARY KEY,
                channelname TEXT,
                channelowner INTEGER,
                description TEXT,
                timestamp INTEGER,
                requiredlevel INTEGER,
                FOREIGN KEY (channelowner) REFERENCES users(userid),
                FOREIGN KEY (requiredlevel) REFERENCES roles(roleid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created channels table.");
        }

        private static void CreateMessageTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS messages (
                messageid INTEGER PRIMARY KEY,
                authorid INTEGER,
                channelid INTEGER,
                content TEXT,
                timestamp INTEGER,
                FOREIGN KEY (authorid) REFERENCES users(userid),
                FOREIGN KEY (channelid) REFERENCES channels(channelid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created messages table.");
        }

        private static void CreateUserChannelsTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS user_channels (
                userid INTEGER,
                channelid INTEGER,
                PRIMARY KEY (userid, channelid),
                FOREIGN KEY (userid) REFERENCES users(userid),
                FOREIGN KEY (channelid) REFERENCES channels(channelid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created user_channels table.");
        }

        private static void CreateDMsTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS direct_messages (
                dmid INTEGER PRIMARY KEY,
                user1id INTEGER,
                user2id INTEGER,
                FOREIGN KEY (user1id) REFERENCES users(userid),
                FOREIGN KEY (user2id) REFERENCES users(userid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created direct_messages table.");
        }

        private static void CreateDirectMessagesTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS messages_dms (
                messageid INTEGER PRIMARY KEY,
                authorid INTEGER,
                dmid INTEGER,
                content TEXT,
                timestamp INTEGER,
                FOREIGN KEY (authorid) REFERENCES users(userid),
                FOREIGN KEY (dmid) REFERENCES direct_messages(dmid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created messages_dms table.");
        }

        private static void CreateRolesTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS roles (
                roleid INTEGER PRIMARY KEY,
                rolename TEXT,
                permissionlevel INTEGER
            );";
            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created roles table.");
        }

        private static void CreateUserRolesTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS user_roles (
                userid INTEGER,
                roleid INTEGER,
                PRIMARY KEY (userid, roleid),
                FOREIGN KEY (userid) REFERENCES users(userid),
                FOREIGN KEY (roleid) REFERENCES roles(roleid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine(Utils.GREEN + "DATABASE: Created user_roles table.");
        }


        public static UInt64 CreateUser(string username, string password)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UInt64 userID = Utils.GenerateID(18);
            string hashedPassword = Auth.HashPassword(password);

            string sql = @"
            INSERT INTO users (userid, name, password, timestamp)
            VALUES (@userid, @name, @password, @timestamp)";

            using var command = new SQLiteCommand(@sql, dbConnection);
            command.Parameters.AddWithValue("@userid", userID);
            command.Parameters.AddWithValue("@name", username);
            command.Parameters.AddWithValue("@password", hashedPassword);
            command.Parameters.AddWithValue("@timestamp", timestamp);

            command.ExecuteNonQuery();
            return userID;
        }

        public static UInt64 SendMessage(UInt64 authorID, string content, UInt64 channelID)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UInt64 messageID = Utils.GenerateID(18);

            string sql = @"
            INSERT INTO messages (authorid, content, channelid, timestamp, messageid)
            VALUES (@authorid, @content, @channelid, @timestamp, @messageid)";

            using var command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@authorid", authorID);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@channelid", channelID);
            command.Parameters.AddWithValue("@timestamp", timestamp);
            command.Parameters.AddWithValue("@messageid", messageID);
            
            command.ExecuteNonQuery();
            return messageID;
        }


        public static List<int> GetUserChannels (UInt64 userID)
        {
            string sql = "SELECT channelid FROM user_channels WHERE userid = @userid";
            using var command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@userid", userID);

            var channels = new List<int>();
            using var reader = command.ExecuteReader();
            while(reader.Read()) { 
                channels.Add(reader.GetInt32(0));
            }

            return channels;
        }

        public static List<int> GetChannelUsers(UInt64 channelID)
        {
            string sql = "SELECT userid FROM user_channels WHERE channel = @channelid";
            using var command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@channelid", channelID);

            var channels = new List<int>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                channels.Add(reader.GetInt32(0));
            }

            return channels;
        }

        public static List<(UInt64 userID, string username, int createdAt)> GetUser(UInt64 userID)
        {
            string sql = @"
            SELECT 
            users.userid,
            users.username,
            users.timestamp
            FROM users WHERE userid == @userid";

            using var command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@userid", userID);

            var user = new List<(UInt64 userID, string username, int createdAt)>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) { 
                var username = reader.GetString(1);
                var createdAt = reader.GetInt32(2);

                user.Add((userID, username, createdAt));
            }

            return user;
        }



        public static List<(int messageID, string content, int authorID, string authorName)> GetChannelMessages(int channelId, int limit, int offset)
        {
            string sql = @"
            SELECT 
            messages.messageid, 
            messages.content, 
            messages.authorid, 
            users.name AS authorName
            FROM messages
            INNER JOIN users ON messages.authorid = users.userid
            WHERE messages.channelid = @channelid
            ORDER BY messages.timestamp DESC
            LIMIT @limit OFFSET @offset";

            using var command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@channelid", channelId);
            command.Parameters.AddWithValue("@limit", limit);
            command.Parameters.AddWithValue("@offset", offset);

            var messages = new List<(int messageID, string content, int authorID, string authorName)>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var messageID = reader.GetInt32(0);
                var content = reader.GetString(1);
                var authorID = reader.GetInt32(2);
                var authorName = reader.GetString(3);

                messages.Add((messageID, content, authorID, authorName));
            }

            return messages;
        }
        public static string GetHashedPassword(string username)
        {
            string sql = @"SELECT password FROM users WHERE name = @name";

            using var command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@name", username);

            object result = command.ExecuteScalar();

            // Handle null or non-existent usernames
            if (result == null || result == DBNull.Value)
            {
                throw new NullReferenceException("Database value for userpassword is null");
            }

            return result.ToString();
        }

    }
}
