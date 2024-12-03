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

namespace c_tier.src.backend.server
{
    public class Database
    {
        private SQLiteConnection dbConnection;

        public SQLiteConnection InitDatabase(string dbPath)
        {
            var connectionString = $"Data Source={dbPath};Version=3;";
            dbConnection = new SQLiteConnection(connectionString);
            dbConnection.Open();

            CreateTables();
            return dbConnection;
        }

        private void ExecuteNonQuery(string sql)
        {
            using var command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }

        private void CreateTables()
        {
            CreateUsersTable();
            CreateChannelsTable();
            CreateMessageTable();
            CreateUserChannelsTable();
        }

        private void CreateUsersTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS users (
                userid INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                timestamp INTEGER,
                password TEXT,
                email TEXT UNIQUE
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine("Created users table.");
        }

        private void CreateChannelsTable()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS channels (
                channelid INTEGER PRIMARY KEY,
                channelname TEXT,
                channelowner INTEGER,
                description TEXT,
                timestamp INTEGER,
                FOREIGN KEY (channelowner) REFERENCES users(userid)
            );";

            ExecuteNonQuery(sql);
            Console.WriteLine("Created channels table.");
        }

        private void CreateMessageTable()
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
            Console.WriteLine("Created messages table.");
        }

        private void CreateUserChannelsTable()
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
            Console.WriteLine("Created user_channels table.");
        }

        public UInt64 CreateUser(string username, string password)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UInt64 userID = Utils.GenerateID(18);

            string sql = @"
            INSERT INTO users (userid, name, password, timestamp)
            VALUES (@userid, @name, @password, @timestamp)";

            using var command = new SQLiteCommand(@sql, dbConnection);
            command.Parameters.AddWithValue("@userid", userID);
            command.Parameters.AddWithValue("@named", username);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@timestamp", timestamp);

            command.ExecuteNonQuery();
            return userID;
        }

        public UInt64 SendMessage(UInt64 authorID, string content, UInt64 channelID)
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


        public List<int> GetUserChannels (UInt64 userID)
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

        public List<int> GetChannelUsers(UInt64 channelID)
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

        public List<(int messageID, string content, int authorID, string authorName)> GetChannelMessages(int channelId, int limit, int offset)
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
    }
}
