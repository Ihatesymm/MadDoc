using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using MadDoc.Settings;
using MadDoc.Extensions;
using MadDoc.Entities;

namespace MadDoc.Modules
{
    [Group("channel")]
    [Aliases("c")]
    public class ChannelModule : BaseCommandModule
    {
        [Command("set")]
        public async Task Set(CommandContext ctx, [RemainingText] string text)
        {
            await ctx.Message.DeleteAsync();

            if (ctx.Channel.Id != AppSettings.CommandsChannel && ctx.Channel.Id != AppSettings.PrivateChannel) return;

            if (!int.TryParse(text, out int number))
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу c ошибкой", 
                    "Пожалуйста, введите число.\n!c set {число}", content: ctx.User.Mention);
                return;
            }

            if (number > 8 || number < 2)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу c ошибкой", 
                    "Количество пациентов должно быть от 2 до 8.", content: ctx.User.Mention);
                return;
            }

            DiscordMember member = null;
            
            try
            {
                member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            }
            catch (Exception) { }

            var voicechannel = member.VoiceState?.Channel;

            if (voicechannel == null ||
                voicechannel.ParentId != AppSettings.AutoCreateCategory ||
                voicechannel.Id == AppSettings.AutoCreate ||
                voicechannel.Id == AppSettings.WaitingRoom)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой", 
                    "Пожалуйста, войдите в голосовой канал групповой терапии.", content: ctx.User.Mention);
                return;
            }

            if (number < voicechannel.Users.Count())
            {
                number = voicechannel.Users.Count();
            }

            var channelInfo = ChannelSQL.GetInfo(voicechannel.Id);

            if (member.Id == channelInfo.UserId)
            {
                await member.VoiceState?.Channel.ModifyAsync(x => x.Userlimit = number);
                await ctx.RespondSuccessAndDeleteAsync("Команда завершена успешно", 
                    $"Максимальное число человек в голосовом канале: {number}.", content: ctx.User.Mention);
            }
            else
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой", 
                    "Вы не являетесь владельцем голосового канала", content: ctx.User.Mention);
            }
         }

        [Command("votekick")]
        [Aliases("vote", "kick")]
        public async Task Votekick(CommandContext ctx, DiscordMember member)
        {
            await ctx.Message.DeleteAsync();

            if (ctx.Channel.Id != AppSettings.CommandsChannel && ctx.Channel.Id != AppSettings.PrivateChannel) return;

            if (member.Id == ctx.User.Id)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой",
                    "Вы не можете голосовать против себя.", content: ctx.User.Mention);
                return;
            }

            if (member.Roles.FirstOrDefault(x => x.Permissions.HasPermission(Permissions.Administrator)) != null)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой",
                    "Вы не можете кикнуть данного пользователя.", content: ctx.User.Mention);
                return;
            }

            var channel = member.VoiceState?.Channel;

            if (channel == null ||
                channel.Id != ctx.Member.VoiceState?.Channel?.Id)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой",
                    "Пользователь не найден или не в вашем канале.", content: ctx.User.Mention);
                return;
            }

            if (channel.Users.Where(x => !x.IsBot).Count() == 2)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой",
                    "Вы не можете проголосовать в канале в котором только 2 пользователя. Боты не учитываются.", content: ctx.User.Mention);
                return;
            }

            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

            var interactivity = ctx.Client.GetInteractivity();

            var votesNeeded =  Math.Round((channel.Users.Where(x => !x.IsBot).Count() - 1) * 0.5 + 1, MidpointRounding.AwayFromZero); //50% + 1 голосов

            var embed = new DiscordEmbedBuilder()
                .WithDescription($"Участники канала {channel.Name} могут проголосовать за кик пользователя.")
                .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl)
                .AddField($"**{member.Username}#{member.Discriminator}**", $"Голосование будет длиться 30 секунд. Нужно {votesNeeded} голос(а/ов).")
                .WithColor(DiscordColor.DarkRed)
                .WithFooter($"ID канала: {member.VoiceState?.Channel?.Id}")
                .WithTimestamp(DateTime.Now);

            var mentions = channel.Users.Where(x => !x.IsBot).Select(x => x.Mention);

            var msg = await ctx.RespondAndDeleteAsync(embed, TimeSpan.FromSeconds(45), string.Join(' ', mentions));

            await msg.CreateReactionAsync(emoji);

            var pollResult = await interactivity.CollectReactionsAsync(msg, TimeSpan.FromSeconds(30));

            var votedUsers = await msg.GetReactionsAsync(emoji);

            await msg.DeleteAllReactionsAsync();

            var votedMembers = new List<DiscordMember>();
            foreach (var votedUser in votedUsers)
                votedMembers.Add(await ctx.Guild.GetMemberAsync(votedUser.Id));

            var votesCount = votedMembers.Where(x => x.VoiceState?.Channel.Id == channel.Id && !x.IsBot && x.Id != member.Id).Count();

            var resultEmbed = new DiscordEmbedBuilder()
                .WithTitle("Madhouse Doctor | Голосование за кик")
                .WithDescription($"Голосование окончено. Голосов за кик: {votesCount}.")
                .WithFooter($"ID канала: {member.VoiceState?.Channel?.Id}")
                .WithColor(DiscordColor.DarkRed)
                .WithTimestamp(DateTime.Now);

            if (votesCount >= votesNeeded)
            {
                resultEmbed.AddField($"**{ctx.User.Username}#{ctx.User.Discriminator}**",
                    $"Участник был перемещен в комнату с белыми подушками и ему был заблокирован доступ в канал: {channel.Name}.");

                await channel.AddOverwriteAsync(member, deny: Permissions.UseVoice);

                if (member.VoiceState?.Channel != null && member.VoiceState?.Channel.Id == channel.Id)
                    await member.PlaceInAsync(ctx.Guild.AfkChannel);
            }
            else
            {
                resultEmbed.AddField($"**{ctx.User.Username}#{ctx.User.Discriminator}**",
                    $"Недостаточно голосов за кик пользователя с канала: {channel.Name}. Нужно {votesNeeded} голос(а/ов)");
            }

            await msg.ModifyAsync(embed: resultEmbed.Build());
        }

        [Command("del")]
        [RequirePermissions(Permissions.Administrator)]
        [Cooldown(1, 5, CooldownBucketType.User)]
        public async Task Delete(CommandContext ctx, [RemainingText] string text)
        {
            await ctx.Message.DeleteAsync();

            if (!int.TryParse(text, out int number))
                return;
            
            if (number > 50)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой", "Максимум 50 сообщений.");
                return;
            }

            if (number < 5)
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу ошибкой", "Минимум 5 сообщений.");
                return;
            }
            
            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(number));
        }
    }
}
