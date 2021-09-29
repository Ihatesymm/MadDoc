using Newtonsoft.Json;

namespace MadDoc.Infrastructure
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("connection")]
        public string ConnectionString { get; private set; }
        [JsonProperty("madhouseid")]
        public ulong MadhouseGuild { get; private set; } 
        [JsonProperty("commandchannel")]
        public ulong CommandsChannel { get; private set; } 
        [JsonProperty("privatechannel")]
        public ulong PrivateChannel { get; private set; } 
        [JsonProperty("autocreatecooldown")]
        public int AutoCreateCooldown { get; private set; }
        [JsonProperty("autocreate")]
        public ulong AutoCreate { get; private set; } 
        [JsonProperty("autocreatecategory")]
        public ulong AutoCreateCategory { get; private set; }
        [JsonProperty("infocooldown")]
        public int InfoCooldown { get; private set; }
        [JsonProperty("waitingroom")]
        public ulong WaitingRoom { get; private set; } 
        [JsonProperty("caducarole")]
        public ulong CaducaRole { get; private set; } 
        [JsonProperty("tempchannel")]
        public ulong TempChannel { get; private set; } 
    }
}
