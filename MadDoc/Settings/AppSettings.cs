using System.IO;
using System.Text;
using Newtonsoft.Json;
using MadDoc.Infrastructure;

namespace MadDoc.Settings
{
    public static class AppSettings
    {
        public static string Prefix             { get; private set; }
        public static string Token              { get; private set; }
        public static string ConnectionString   { get; private set; }
        public static ulong  MadhouseGuild      { get; private set; } 
        public static ulong  CommandsChannel    { get; private set; }
        public static ulong  PrivateChannel     { get; private set; } 
        public static int    AutoCreateCooldown { get; private set; } 
        public static ulong  AutoCreate         { get; private set; } 
        public static ulong  AutoCreateCategory { get; private set; }
        public static int    InfoCooldown       { get; private set; }
        public static ulong  WaitingRoom        { get; private set; } 
        public static ulong  CaducaRole         { get; private set; } 
        public static ulong  TempChannel        { get; private set; } 
        public static void Initializate()
        {
            var json = string.Empty;
            using (var file = File.OpenRead("config.json"))
            using (var stream = new StreamReader(file, new UTF8Encoding(false)))
                json = stream.ReadToEnd();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            Prefix = configJson.Prefix;
            Token = configJson.Token;
            ConnectionString = configJson.ConnectionString;
            MadhouseGuild = configJson.MadhouseGuild;
            CommandsChannel = configJson.CommandsChannel;
            PrivateChannel = configJson.PrivateChannel;
            AutoCreateCooldown = configJson.AutoCreateCooldown;
            AutoCreate = configJson.AutoCreate;
            AutoCreateCategory = configJson.AutoCreateCategory;
            WaitingRoom = configJson.WaitingRoom;
            CaducaRole = configJson.CaducaRole;
            TempChannel = configJson.TempChannel;
            InfoCooldown = configJson.InfoCooldown;
         }
    }
}
