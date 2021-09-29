using System;
using System.Timers;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MadDoc.Modules;
using MadDoc.Settings;
using MadDoc.Extensions;

namespace MadDoc.Infrastructure
{
    internal sealed class Bot
    {
        #region Variables

        public DiscordClient Discord { get; set; }
        public CommandsNextExtension Commands { get; set; }

        private readonly DiscordColor[] colors = new DiscordColor[]
        {
            DiscordColor.Gold,
            DiscordColor.Azure,
            DiscordColor.Purple,
            DiscordColor.Red,
            DiscordColor.Teal,
            DiscordColor.Orange,
            DiscordColor.DarkRed,
            DiscordColor.Magenta,
            DiscordColor.SapGreen,
            DiscordColor.Rose,
            DiscordColor.NotQuiteBlack,
            DiscordColor.Lilac
        };

        private int colorId = 0;

        #endregion

        #region MainProcess
        public async Task RunAsync()
        {
            var config = new DiscordConfiguration()
            {
                Token = AppSettings.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
            };

            Discord = new DiscordClient(config);

            Discord.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { AppSettings.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
            };

            Commands = Discord.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<ChannelModule>();
            Commands.RegisterCommands<GeneralModule>();

            AsyncActionsHandler.InstallListeners(Discord, this);

            await Discord.ConnectAsync();

            await IsClientReady();

            await Task.Delay(-1);
        }

        #endregion

        #region Timers

        private Task IsClientReady()
        {
            // Таймер обновления цвета роли Эпилептик.
            Timer timer = new Timer(180000)
            {
                AutoReset = true
            };
            timer.Enabled = true;
            timer.Elapsed += async (sender, args) =>
            {
                try
                {
                    var guild = await Discord.GetGuildAsync(AppSettings.MadhouseGuild);

                    var role = guild.GetRole(AppSettings.CaducaRole);

                    await role.ModifyAsync(x => x.Color = colors[colorId]);

                    colorId++;

                    if (colorId == 12)
                        colorId = 0;
                }
                catch (Exception) { }
            };

            // Таймер обновления активности бота. 5 минут.
            Timer activity = new Timer(300000)
            {
                AutoReset = true
            };
            activity.Enabled = true;
            activity.Elapsed += async (sender, args) =>
            {
                try
                {
                    var guild = await Discord.GetGuildAsync(AppSettings.MadhouseGuild);
                    var activity = new DiscordActivity($"за пациентами. Всего пациентов и медперсонала: {guild.MemberCount}", ActivityType.Watching);
                    await Discord.UpdateStatusAsync(activity, UserStatus.Online);
                }
                catch (Exception) { }
            };

            return Task.CompletedTask;
        }

        #endregion
    }
}