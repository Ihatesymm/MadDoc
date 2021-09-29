using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MadDoc.Settings;
using MadDoc.Extensions;
using MadDoc.Entities;

namespace MadDoc.Handlers
{
    public static class VoiceChannelHandler
    {
        public static Dictionary<DiscordUser, DateTime> AutoCreateCooldowns = new Dictionary<DiscordUser, DateTime>();

        [AsyncActions(EventTypes.VoiceStateUpdated)]
        public static async Task UserJoined(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            try
            {
                if (e.Channel.Id == AppSettings.AutoCreate)
                {
                    if (AutoCreateCooldowns.ContainsKey(e.User))
                    {
                        if ((AutoCreateCooldowns[e.User] - DateTime.Now).Seconds > 0)
                        {
                            var m = await e.Guild.GetMemberAsync(e.User.Id);
                            await m.PlaceInAsync(e.Guild.GetChannel(AppSettings.WaitingRoom));
                            var embed = new DiscordEmbedBuilder()
                                       .WithColor(DiscordColor.DarkRed)
                                       .WithAuthor("Не удается создать голосовой канал", iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/1024/sign-error-icon.png")
                                       .WithDescription($"Вам нужно подождать **{(AutoCreateCooldowns[e.User] - DateTime.Now).Seconds}** "
                                       + "секунд прежде чем создавать групповую терапию.");
                            await m.SendMessageAsync(embed: embed);
                            return;
                        }
                    }

                    AutoCreateCooldowns[e.User] = DateTime.Now.AddSeconds(AppSettings.AutoCreateCooldown);

                    var member = await e.Guild.GetMemberAsync(e.User.Id);

                    var autoCreateGroupCategory = e.Guild.GetChannel(AppSettings.AutoCreateCategory);

                    string name = member.Nickname ?? member.Username;

                    var channelName = $"Групповая терапия {name}";

                    DiscordChannel created = null;

                    created = await e.Guild.CreateVoiceChannelAsync(channelName, autoCreateGroupCategory, user_limit: 3);

                    ChannelSQL.Create(created.Id, member.Id, name);

                    await member.PlaceInAsync(created);
                }
            }
            catch (NullReferenceException)
            {
                if (e.Before != null && e.Before.Channel != null)
                {
                    var leftChannel = e.Before.Channel;

                    if (leftChannel.ParentId == AppSettings.AutoCreateCategory &&
                        leftChannel.Id != AppSettings.AutoCreate &&
                        leftChannel.Id != AppSettings.WaitingRoom)
                    {
                        if (leftChannel.Users.Count() == 0)
                        {
                            await leftChannel.DeleteAsync();
                            ChannelSQL.Delete(leftChannel.Id);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        [AsyncActions(EventTypes.VoiceStateUpdated)]
        public static async Task UserLeft(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            try
            {
                if (e.Before != null && e.Before.Channel != null)
                {
                    var leftChannel = e.Before.Channel;

                    if (leftChannel.ParentId == AppSettings.AutoCreateCategory &&
                        leftChannel.Id != AppSettings.AutoCreate &&
                        leftChannel.Id != AppSettings.WaitingRoom)
                    {
                        if (leftChannel.Users.Count() == 0)
                        {
                            await leftChannel.DeleteAsync();
                            ChannelSQL.Delete(leftChannel.Id);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        [AsyncActions(EventTypes.VoiceStateUpdated)]
        public static async Task CheckNameOfChannel(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            try
            {
                if (e.Before != null && e.Before.Channel != null)
                {
                    var leftChannel = e.Before.Channel;

                    var member = await e.Guild.GetMemberAsync(e.User.Id);

                    var channelInfo = ChannelSQL.GetInfo(leftChannel.Id);

                    if (leftChannel.ParentId == AppSettings.AutoCreateCategory &&
                        leftChannel.Id != AppSettings.AutoCreate &&
                        leftChannel.Id != AppSettings.WaitingRoom &&
                        e.User.Id == channelInfo.UserId &&
                        leftChannel.Users.Count() > 0)
                    {
                        string name = leftChannel.Users.First().Nickname ?? leftChannel.Users.First().Username;
                        ulong userid = leftChannel.Users.First().Id;

                        await leftChannel.ModifyAsync(x =>
                        {
                            x.Name = $"Групповая терапия {name}";
                        });

                        channelInfo.UserName = name;
                        channelInfo.UserId = userid;
                    }
                }
            }
            catch (Exception) { }
        }

        [AsyncActions(EventTypes.VoiceStateUpdated)]
        public static Task TimeUpdate(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e.After?.Channel?.ParentId == AppSettings.AutoCreateCategory &&
                e.After?.Channel?.Id != AppSettings.AutoCreate &&
                e.After?.Channel?.Id != AppSettings.WaitingRoom)
            {
                VoiceChannelsInfo vci = new VoiceChannelsInfo(AppSettings.AutoCreate, e.User.Id);
                vci.LastJoin = DateTimeOffset.Now;
                return Task.CompletedTask;
            }

            if (e.Before?.Channel?.ParentId == AppSettings.AutoCreateCategory &&
                e.Before?.Channel?.Id != AppSettings.AutoCreate &&
                e.Before?.Channel?.Id != AppSettings.WaitingRoom)
            {
                VoiceChannelsInfo vci = new VoiceChannelsInfo(AppSettings.AutoCreate, e.User.Id);
                vci.Time += (ulong)Math.Round((DateTimeOffset.Now - vci.LastJoin).TotalSeconds);
                vci.LastJoin = DateTimeOffset.Now;
                return Task.CompletedTask;
            }
            
            if (e.Before?.Channel == null &&
                e.After?.Channel != null)
            {
                VoiceChannelsInfo vci = new VoiceChannelsInfo(e.After.Channel.Id, e.User.Id);
                vci.LastJoin = DateTimeOffset.Now;
                return Task.CompletedTask;
            }

            if (e.Before?.Channel != null &&
                e.After?.Channel != null)
            {
                VoiceChannelsInfo vci = new VoiceChannelsInfo(e.Before.Channel.Id, e.User.Id);
                vci.Time += (ulong)Math.Round((DateTimeOffset.Now - vci.LastJoin).TotalSeconds);
                vci.LastJoin = DateTimeOffset.Now;

                VoiceChannelsInfo newVci = new VoiceChannelsInfo(e.After.Channel.Id, e.User.Id);
                newVci.LastJoin = DateTimeOffset.Now;
                return Task.CompletedTask;
            }

            if (e.After?.Channel == null &&
                e.Before?.Channel != null)
            {
                VoiceChannelsInfo vci = new VoiceChannelsInfo(e.Before.Channel.Id, e.User.Id);
                vci.Time += (ulong)Math.Round((DateTimeOffset.Now - vci.LastJoin).TotalSeconds);
                vci.LastJoin = DateTimeOffset.Now;
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}