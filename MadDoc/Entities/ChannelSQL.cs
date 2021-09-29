using MadDoc.Settings;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace MadDoc.Entities
{
    public class ChannelSQL
    {
        public readonly ulong Id;

        private ulong _userId;
        private string _userName;

        public ulong UserId
        {
            get => _userId;

            set
            {
                using var connection = new MySqlConnection(AppSettings.ConnectionString);
                using var cmd = new MySqlCommand();
                var statement = "UPDATE channels SET user_id = @value WHERE channel_id = @channelid";

                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@channelid", Id);

                cmd.CommandText = statement;
                cmd.Connection = connection;
                cmd.Connection.Open();

                cmd.ExecuteNonQuery();

                _userId = value;
            }
        }

        public string UserName
        {
            get => _userName;

            set
            {
                using var connection = new MySqlConnection(AppSettings.ConnectionString);
                using var cmd = new MySqlCommand();
                var statement = "UPDATE channels SET user_name = @value WHERE channel_id = @channelid";

                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@channelid", Id);

                cmd.CommandText = statement;
                cmd.Connection = connection;
                cmd.Connection.Open();

                cmd.ExecuteNonQuery();

                _userName = value;
            }
        }

        private ChannelSQL(ulong channelid, ulong userid, string username)
        {
            Id = channelid;
            _userId = userid;
            _userName = username;
        }

        public static ChannelSQL Create(ulong channelid, ulong userid, string username)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();

            var statement = "INSERT INTO channels(channel_id, user_id, user_name) VALUES (@channelid, @userid, @username);";

            cmd.Parameters.AddWithValue("@channelid", channelid);
            cmd.Parameters.AddWithValue("@userId", userid);
            cmd.Parameters.AddWithValue("@userName", username);

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();

            return new ChannelSQL(channelid, userid, username);
        }

        public static void Delete(ulong channelid)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "DELETE FROM channels WHERE channel_id = @channelid";

            cmd.Parameters.AddWithValue("@channelid", channelid);

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();
        }

        public static ChannelSQL GetForUser(ulong userid)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM channels WHERE user_id = @userid";

            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;
            else
                return new ChannelSQL(reader.GetUInt64("channel_id"), reader.GetUInt64("user_id"), reader.GetString("user_name"));
        }

        public static ChannelSQL GetInfo(ulong channelid)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM channels WHERE channel_id = @channelid";

            cmd.Parameters.AddWithValue("@channelid", channelid);
            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;
            else
                return new ChannelSQL(reader.GetUInt64("channel_id"), reader.GetUInt64("user_id"), reader.GetString("user_name"));
        }

        public static List<ChannelSQL> GetAll()
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM channels";

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            List<ChannelSQL> channels = new List<ChannelSQL>();

            while (reader.Read())
                channels.Add(new ChannelSQL(reader.GetUInt64("channel_id"), reader.GetUInt64("user_id"), reader.GetString("user_name")));
            
            return channels;
        }
    }
}
