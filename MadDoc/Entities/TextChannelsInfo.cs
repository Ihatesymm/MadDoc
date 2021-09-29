using MadDoc.Settings;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace MadDoc.Entities
{
    public class TextChannelsInfo
    {
        private ulong _userId;
        private ulong _channelId;
        private ulong _messagesCount;
        private DateTimeOffset _lastMessage;

        public ulong UserId
        {
            get => _userId;
        }

        public ulong ChannelId
        {
            get => _channelId;
        }

        public ulong MessagesCount
        {
            get => _messagesCount;

            set
            {
                using var connection = new MySqlConnection(AppSettings.ConnectionString);
                using var cmd = new MySqlCommand();
                var statement = "UPDATE text_channels SET msg_count = @value WHERE channel_id = @channelid AND user_id = @userid";

                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@userid", _userId);
                cmd.Parameters.AddWithValue("@channelid", _channelId);

                cmd.CommandText = statement;
                cmd.Connection = connection;
                cmd.Connection.Open();

                cmd.ExecuteNonQuery();

                _messagesCount = value;
            }
        }

        public DateTimeOffset LastMessageDate
        {
            get => _lastMessage;

            set
            {
                using var connection = new MySqlConnection(AppSettings.ConnectionString);
                using var cmd = new MySqlCommand();
                var statement = "UPDATE text_channels SET last_message_date  = @value WHERE user_id = @userid AND channel_id = @channelid";

                cmd.Parameters.AddWithValue("@value", value.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@userid", _userId);
                cmd.Parameters.AddWithValue("@channelid", _channelId);

                cmd.CommandText = statement;
                cmd.Connection = connection;
                cmd.Connection.Open();

                cmd.ExecuteNonQuery();

                _lastMessage = value;
            }
        }

        public TextChannelsInfo(ulong channelId, ulong userId)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM text_channels WHERE channel_id = @channelid AND user_id = @userid";

            cmd.Parameters.AddWithValue("@channelid", channelId);
            cmd.Parameters.AddWithValue("@userid", userId);

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                CreateInfo(userId, channelId);
            }
            else
            {
                _userId = userId;
                _channelId = channelId;
                _messagesCount = reader.GetUInt64("msg_count");
                _lastMessage = reader.GetDateTime("last_message_date");
            }
        }

        private TextChannelsInfo(ulong channelId, ulong userId, ulong messagesCount, DateTimeOffset lastMessage)
        {
            _channelId = channelId;
            _userId = userId;
            _messagesCount = messagesCount;
            _lastMessage = lastMessage;
        }

        private void CreateInfo(ulong channelId, ulong userId)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "INSERT INTO text_channels(channel_id, user_id, last_message_date ) VALUES (@channelid, @userid, @lastmsg);";

            cmd.Parameters.AddWithValue("@channelid", channelId);
            cmd.Parameters.AddWithValue("@userid", userId);
            cmd.Parameters.AddWithValue("@lastmsg", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();

            _userId = userId;
            _channelId = channelId;
            _messagesCount = 0;
            _lastMessage = DateTimeOffset.Now;
        }

        public static List<TextChannelsInfo> GetAllText(ulong userId)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM text_channels WHERE user_id = @userid";

            cmd.Parameters.AddWithValue("@userid", userId);

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            List<TextChannelsInfo> texts = new List<TextChannelsInfo>();

            while (reader.Read())
            {
                texts.Add(new TextChannelsInfo(
                    reader.GetUInt64("channel_id"),
                    userId,
                    reader.GetUInt64("msg_count"),
                    reader.GetDateTime("last_message_date")
                   ));
            }

            return texts;
        }
    }
}
