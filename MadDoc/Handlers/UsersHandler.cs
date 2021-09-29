using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MadDoc.Settings;
using MadDoc.Entities;
using MadDoc.Extensions;

namespace MadDoc.Handlers
{
    public static class UsersHandler
    {
        [AsyncActions(EventTypes.GuildMemberUpdated)]
        public static async Task NicknameChanges(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            var voiceState = e.Member?.VoiceState?.Channel;

            if (e.Member == null ||
                voiceState == null)
                return;

            var channelInfo = ChannelSQL.GetForUser(e.Member.Id);

            if (channelInfo == null)
                return;

            if (e.NicknameAfter != e.NicknameBefore &&
                voiceState.Id == channelInfo.Id)
            {
                await voiceState.ModifyAsync(x => x.Name = $"Групповая терапия {e.NicknameAfter}");
                channelInfo.UserName = e.NicknameAfter ?? e.Member.Username;
                channelInfo.UserId = e.Member.Id;
            }
        }

        [AsyncActions(EventTypes.UserUpdated)]
        public static async Task UsernameChanges(DiscordClient sender, UserUpdateEventArgs e)
        {
            if (e.UserAfter.Username == e.UserBefore.Username)
                return;

            var guild = await sender.GetGuildAsync(AppSettings.MadhouseGuild);

            var member = await guild.GetMemberAsync(e.UserBefore.Id);

            var channelInfo = ChannelSQL.GetForUser(member.Id);

            if (channelInfo == null)
                return;

            var voiceState = member.VoiceState?.Channel;

            if (voiceState == null)
                return;

            if (e.UserAfter.Username != e.UserBefore.Username &&
                member.Nickname == null &&
                voiceState.Id == channelInfo.Id)
            {
                await voiceState.ModifyAsync(x => x.Name = $"Групповая терапия {e.UserAfter.Username}");
                channelInfo.UserName = e.UserAfter.Username;
                channelInfo.UserId = e.UserAfter.Id;
            }
        }
    }
}
