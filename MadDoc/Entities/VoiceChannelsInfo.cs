using MadDoc.Settings;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace MadDoc.Entities
{
    public class VoiceChannelsInfo
    {
        private ulong _userId;
        private ulong _channelId;
        private ulong _time;
        private DateTimeOffset _lastJoin;

        public ulong UserId
        {
            get => _userId;
        }

        public ulong ChannelId
        {
            get => _channelId;
        }

        public ulong Time
        {
            get => _time;

            set
            {
                using var connection = new MySqlConnection(AppSettings.ConnectionString);
                using var cmd = new MySqlCommand();
                var statement = "UPDATE voice_channels SET time = @value WHERE channel_id = @channelid AND user_id = @userid";

                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@userid", _userId);
                cmd.Parameters.AddWithValue("@channelid", _channelId);

                cmd.CommandText = statement;
                cmd.Connection = connection;
                cmd.Connection.Open();

                cmd.ExecuteNonQuery();

                _time = value;
            }
        }

        public DateTimeOffset LastJoin
        {
            get => _lastJoin;

            set
            {
                using var connection = new MySqlConnection(AppSettings.ConnectionString);
                using var cmd = new MySqlCommand();
                var statement = "UPDATE voice_channels SET last_join = @value WHERE user_id = @userid AND channel_id = @channelid";

                cmd.Parameters.AddWithValue("@value", value.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@userid", _userId);
                cmd.Parameters.AddWithValue("@channelid", _channelId);

                cmd.CommandText = statement;
                cmd.Connection = connection;
                cmd.Connection.Open();

                cmd.ExecuteNonQuery();

                _lastJoin = value;
            }
        }

        public VoiceChannelsInfo(ulong channelId, ulong userId)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM voice_channels WHERE channel_id = @channelid AND user_id = @userid";

            cmd.Parameters.AddWithValue("@channelid", channelId);
            cmd.Parameters.AddWithValue("@userid", userId);

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                CreateInfo(channelId, userId);
            }
            else
            {
                _userId = userId;
                _channelId = channelId;
                _time = reader.GetUInt64("time");
                _lastJoin = reader.GetDateTime("last_join");
            }
        }

        private VoiceChannelsInfo(ulong channelId, ulong userId, ulong time, DateTimeOffset lastJoin)
        {
            _channelId = channelId;
            _userId = userId;
            _time = time;
            _lastJoin = lastJoin;
        }

        private void CreateInfo(ulong channelId, ulong userId)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "INSERT INTO voice_channels(channel_id, user_id, last_join) VALUES (@channelid, @userid, @lastjoin);";

            cmd.Parameters.AddWithValue("@channelid", channelId);
            cmd.Parameters.AddWithValue("@userid", userId);
            cmd.Parameters.AddWithValue("@lastjoin", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();

            _userId = userId;
            _channelId = channelId;
            _time = 0;
            _lastJoin = DateTimeOffset.Now;
        }

        public static List<VoiceChannelsInfo> GetAllVoices(ulong userId)
        {
            using var connection = new MySqlConnection(AppSettings.ConnectionString);
            using var cmd = new MySqlCommand();
            var statement = "SELECT * FROM voice_channels WHERE user_id = @userid";

            cmd.Parameters.AddWithValue("@userid", userId);

            cmd.CommandText = statement;
            cmd.Connection = connection;
            cmd.Connection.Open();

            var reader = cmd.ExecuteReader();

            List<VoiceChannelsInfo> voices = new List<VoiceChannelsInfo>();

            while (reader.Read())
            {
                voices.Add(new VoiceChannelsInfo(
                    reader.GetUInt64("channel_id"),
                    userId,
                    reader.GetUInt64("time"),
                    reader.GetDateTime("last_join")
                   ));
            }

            return voices;
        }
    }
}
