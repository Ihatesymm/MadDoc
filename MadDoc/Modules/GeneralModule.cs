using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MadDoc.Settings;
using MadDoc.Extensions;
using MadDoc.Entities;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace MadDoc.Modules
{
    public class GeneralModule : BaseCommandModule
    {
        public enum Emoji
        {
            Set,
            Del,
            Info,
            VoteKick
        }

        private static readonly IReadOnlyDictionary<Emoji, string> EmojiCollection = new Dictionary<Emoji, string>()
        {
            { Emoji.Set, ":red_circle:"},
            { Emoji.Del, ":blue_circle:"},
            { Emoji.Info, ":orange_circle:"},
            { Emoji.VoteKick, ":white_circle:"}
        };

        public static Dictionary<DiscordUser, DateTime> InfoCooldowns = new Dictionary<DiscordUser, DateTime>();

        [Command("info")]
        public async Task Info(CommandContext ctx, DiscordUser user = null)
        {
            await ctx.Message.DeleteAsync();

            if (ctx.Channel.Id != AppSettings.PrivateChannel && ctx.Channel.Id != AppSettings.CommandsChannel) return;

            if (InfoCooldowns.ContainsKey(ctx.User))
            {
                if ((InfoCooldowns[ctx.User] - DateTime.Now).Seconds > 0)
                {
                    await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу c ошибкой", 
                        $"Вам нужно подождать **{Math.Truncate((InfoCooldowns[ctx.User] - DateTime.Now).TotalSeconds)}** секунд.");
                    return;
                }
            }

            InfoCooldowns[ctx.User] = DateTime.Now.AddSeconds(AppSettings.InfoCooldown);

            var wait = await ctx.SendWaitMessage("Команда выполняется", "Пожалуйста, подождите.");

            user ??= ctx.User;

            DiscordMember member = null;

            try
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
            }
            catch (Exception) { return; }

            var path = await Images.CreateImageAsync(member);

            var msg = await ctx.Client.GetChannelAsync(AppSettings.TempChannel).Result.SendFileAsync(path);

            string imgurl = msg.Attachments.First().Url;

            var voices = VoiceChannelsInfo.GetAllVoices(user.Id);
            var texts = TextChannelsInfo.GetAllText(user.Id);

            ulong allTime = 0;
            ulong allMessages = 0;

            TextChannelsInfo favoriteTextChannel = null;
            TextChannelsInfo lastTextChannel = null;
            VoiceChannelsInfo favoriteVoiceChannel = null;
            VoiceChannelsInfo lastVoiceChannel = null;

            try
            {
                favoriteVoiceChannel = voices[0];
                lastVoiceChannel = voices[0];
            }
            catch (Exception) {  }

            try
            {
                favoriteTextChannel = texts[0];
                lastTextChannel = texts[0];
            }
            catch (Exception) { }

            foreach (VoiceChannelsInfo voice in voices)
            {
                if (voice.Time >= favoriteVoiceChannel.Time)
                    favoriteVoiceChannel = voice;

                if (voice.LastJoin >= lastVoiceChannel.LastJoin)
                    lastVoiceChannel = voice;

                allTime += voice.Time;
            }

            foreach (TextChannelsInfo text in texts)
            {
                if (text.MessagesCount >= favoriteTextChannel.MessagesCount)
                    favoriteTextChannel = text;

                if (text.LastMessageDate >= lastTextChannel.LastMessageDate)
                    lastTextChannel = text;

                allMessages += text.MessagesCount;
            }

            DiscordChannel discordVoiceChannel = null;
            DiscordChannel discordTextChannel = null;
            DiscordChannel discordLastVoiceChannel = null;
            DiscordChannel discordLastTextChannel = null;

            string timeInfo = null;
            string allTimeInfo = null;
            string nameTextChannel = null;
            string nameLastTextChannel = null;

            try
            {
                discordVoiceChannel = ctx.Guild.GetChannel(favoriteVoiceChannel.ChannelId);
                discordLastVoiceChannel = ctx.Guild.GetChannel(lastVoiceChannel.ChannelId);

                timeInfo = $"{Math.Truncate(TimeSpan.FromSeconds(favoriteVoiceChannel.Time).TotalDays)} Дни\n" +
                $"{TimeSpan.FromSeconds(favoriteVoiceChannel.Time).Hours} Часы\n" +
                $"{TimeSpan.FromSeconds(favoriteVoiceChannel.Time).Minutes} Минуты\n" +
                $"{TimeSpan.FromSeconds(favoriteVoiceChannel.Time).Seconds} Секунды\n";

                allTimeInfo = $"{Math.Truncate(TimeSpan.FromSeconds(allTime).TotalDays)} Дни\n" +
                    $"{TimeSpan.FromSeconds(allTime).Hours} Часы \n" +
                    $"{TimeSpan.FromSeconds(allTime).Minutes} Минуты\n" +
                    $"{TimeSpan.FromSeconds(allTime).Seconds} Секунды\n";
            }
            catch (Exception) { }

            try
            {
                discordTextChannel = ctx.Guild.GetChannel(favoriteTextChannel.ChannelId);
                discordLastTextChannel = ctx.Guild.GetChannel(lastTextChannel.ChannelId);

                nameTextChannel = discordTextChannel?.Name[0].ToString().ToUpper() + 
                    string.Join(' ', discordTextChannel?.Name.Split('-', StringSplitOptions.RemoveEmptyEntries)).Remove(0, 1);

                nameLastTextChannel = discordLastTextChannel?.Name[0].ToString().ToUpper() +
                    string.Join(' ', discordLastTextChannel?.Name.Split('-', StringSplitOptions.RemoveEmptyEntries)).Remove(0, 1);
            }
            catch (Exception) { }

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .AddField("ID пользователя:", member.Id.ToString(), true)
                .AddField("Имя пользователя:", $"{member.Username}#{member.Discriminator}", true)
                .AddField("Был создан:", member.CreationTimestamp.ToString("dd/MM/yyyy"), true)
                .AddField("Присоединился:", member?.JoinedAt.ToString("dd/MM/yyyy") ?? "Нет на сервере", true)
                .AddField("Статус:", member.Presence?.Status.ToString() ?? "Offline", true)
                .AddField("Голосовой канал:", member?.VoiceState?.Channel?.Name ?? "Нет в голосовых каналах", true)
                .AddField("Любимый голосовой канал:", discordVoiceChannel?.Name ?? "Отсутствует", true)
                .AddField("Время в любимом голосовом канале:", timeInfo ?? "Отсутствует", true)
                .AddField("Общее время в голосовых каналах:", allTimeInfo ?? "Отсутствует", true)
                .AddField("Любимый текстовый канал:", nameTextChannel ?? "Отсутствует", true)
                .AddField("Сообщений в любимом текстовом канале:", favoriteTextChannel?.MessagesCount.ToString() ?? "0", true)
                .AddField("Общее количество сообщений:", allMessages.ToString(), true)
                .AddField("Последний голосовой канал:", discordLastVoiceChannel?.Name ?? "Отсутствует", true)
                .AddField("Последний раз заходил в голосовой канал:", lastVoiceChannel?.LastJoin.ToString("HH:mm:ss\ndd/MM/yyyy") ?? "Отсутствует", true)
                .AddField("Последнее сообщение в канале:", nameLastTextChannel ?? "Отсутствует", true)
                .AddField("** **", "** **", true)
                .AddField("Дата последнего сообщения:", lastTextChannel?.LastMessageDate.ToString("HH:mm:ss\ndd/MM/yyyy") ?? "Отсутствует", true)
                .AddField("** **", "** **", true)
                .AddField("Роли:", string.Join(" ", member.Roles.Select(x => x.Mention).Where(x => !x.Contains("@everyone"))))
                .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl)
                .WithFooter("Информация о пациенте")
                .WithTimestamp(DateTimeOffset.Now)
                .WithImageUrl(imgurl);

            try
            {
                await wait.DeleteAsync();
            }
            catch (Exception)
            { }

            await ctx.RespondAndDeleteAsync(embed: embed.Build(), TimeSpan.FromMinutes(1), $"Информация по запросу {ctx.User.Mention}.");
            File.Delete(path);
        }

        [Command("reddit")]
        public async Task RedditCommand(CommandContext ctx, string sub = null)
        {
            await ctx.Message.DeleteAsync();

            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{sub ?? "memes"}/random.json?limit=1");

            if (!result.StartsWith("["))
            {
                await ctx.RespondErrorAndDeleteAsync("Команда завершила свою работу c ошибкой", "Страницы не существует.");
                return;
            }

            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .AddField(post["title"].ToString(), "** **", true)
                .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl)
                .WithFooter("Картинка по запросу")
                .WithTimestamp(DateTimeOffset.Now)
                .WithImageUrl(post["url"].ToString());

            await ctx.RespondAndDeleteAsync(embed.Build());
        }

        [Command("start")]
        public async Task Start(CommandContext ctx)
        {
            if (ctx.Message.Author.Id != 333193140091486208) return;

            await ctx.Message.DeleteAsync();

            var builder = new DiscordEmbedBuilder()
            .WithAuthor("Madhouse Doctor | Список команд", iconUrl: ctx.Client.CurrentUser.AvatarUrl)
            .WithDescription("Напишите одну из команд в данный чат:")
            .WithColor(DiscordColor.DarkRed)
            .AddField($"{EmojiCollection[Emoji.Set]} **!c set {{число}}**", "Задает размер голосового канала от 2 до 8.", true)
            .AddField($"{EmojiCollection[Emoji.Info]} **!info {{пусто/пользователь/ID}}**",
            "Выдает медкарту пациента. Для вывода информации о другом пользователе упомяните его, либо напишите его ID Discord.", true)
            .AddField($"{EmojiCollection[Emoji.VoteKick]} **!c vote {{пользователь/ID}}**", "Создает голосование за кик пользователя из голосового канала.", true)
            .AddField($"{EmojiCollection[Emoji.Del]} **!c del {{число}}**", "Очистка сообщений в выбранном канале. Только для администрации.", true)
            .AddField("** **", "** **", true)
            .AddField("** **", "** **", true)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter("Информация о командах");

            await ctx.RespondAsync(embed: builder.Build());
        }
    }
}
