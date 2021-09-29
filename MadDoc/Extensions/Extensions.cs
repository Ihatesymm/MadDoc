using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace MadDoc.Extensions
{
    public static class Extensions
    {
        public static async Task<DiscordMessage> SendErrorAsync(this CommandContext ctx, string title, string description, string content = null)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithAuthor(title, iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/1024/sign-error-icon.png")
                .WithDescription(description)
                .WithTimestamp(DateTime.Now);

            var message = await ctx.RespondAsync(content ,embed: embed.Build());

            return message;
        }

        public static async Task<DiscordMessage> SendErrorAsync(this DiscordChannel ctx, string title, string description, string content = null)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithAuthor(title, iconUrl: "https://icons.iconarchive.com/icons/paomedia/small-n-flat/1024/sign-error-icon.png")
                .WithDescription(description)
                .WithTimestamp(DateTime.Now);

            var message = await ctx.SendMessageAsync(content, embed: embed.Build());

            return message;
        }

        public static async Task<DiscordMessage> SendSuccessAsync(this CommandContext ctx, string title, string description, string content = null)
        {
            
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithAuthor(title, iconUrl: "https://e7.pngegg.com/pngimages/878/377/png-clipart-check-mark-computer-icons-others-miscellaneous-angle.png")
                .WithDescription(description)
                .WithTimestamp(DateTime.Now);

            var message = await ctx.RespondAsync(content, embed: embed.Build());

            return message;
        }
        public static async Task<DiscordMessage> SendSuccessAsync(this DiscordChannel ctx, string title, string description, string content = null)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithAuthor(title, iconUrl: "https://e7.pngegg.com/pngimages/878/377/png-clipart-check-mark-computer-icons-others-miscellaneous-angle.png")
                .WithDescription(description)
                .WithTimestamp(DateTime.Now);

            var message = await ctx.SendMessageAsync(content, embed: embed.Build());

            return message;
        }

        public static async Task<DiscordMessage> SendWaitMessage(this CommandContext ctx, string title, string description, string content = null)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithAuthor(title, iconUrl: "https://monomal.s3.amazonaws.com/uploads/item/itemimage/5286/9bd62a_40c8fc9e5a794e9fad1655b05c3d7e0c_mv2.gif")
                .WithDescription(description)
                .WithTimestamp(DateTime.Now);
            
            var message = await ctx.RespondAsync(content, embed: embed.Build());

            return message;
        }

        public static async Task<DiscordMessage> SendWaitMessage(this DiscordChannel ctx, string title, string description, string content = null)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithAuthor(title, iconUrl: "https://monomal.s3.amazonaws.com/uploads/item/itemimage/5286/9bd62a_40c8fc9e5a794e9fad1655b05c3d7e0c_mv2.gif")
                .WithDescription(description)
                .WithTimestamp(DateTime.Now);

            var message = await ctx.SendMessageAsync(content, embed: embed.Build());

            return message;
        }

        public static async Task RespondErrorAndDeleteAsync(this CommandContext ctx, string title, string description, TimeSpan? time = null, string content = null)
        {
            time ??= TimeSpan.FromSeconds(10);

            var msg = await ctx.SendErrorAsync(title, $"{description}\nСообщение будет удалено через {time.Value.TotalSeconds} секунд.", content);

            Elapsed(msg, time);
        }
        public static async Task RespondErrorAndDeleteAsync(this DiscordChannel ctx, string title, string description, TimeSpan? time = null, string content = null)
        {
            time ??= TimeSpan.FromSeconds(10);

            var msg = await ctx.SendErrorAsync(title, $"{description}\nСообщение будет удалено через {time.Value.TotalSeconds} секунд.", content);

            Elapsed(msg, time);
        }
        public static async Task RespondSuccessAndDeleteAsync(this CommandContext ctx, string title, string description, TimeSpan? time = null, string content = null)
        {
            time ??= TimeSpan.FromSeconds(10);

            var msg = await ctx.SendSuccessAsync(title, $"{description}\nСообщение будет удалено через {time.Value.TotalSeconds} секунд.", content);

            Elapsed(msg, time);
        }
        public static async Task RespondSuccessAndDeleteAsync(this DiscordChannel ctx, string title, string description, TimeSpan? time = null, string content = null)
        {
            time ??= TimeSpan.FromSeconds(10);

            var msg = await ctx.SendSuccessAsync(title, $"{description}\nСообщение будет удалено через {time.Value.TotalSeconds} секунд.", content);

            Elapsed(msg, time);
        }

        

        public static async Task<DiscordMessage> RespondAndDeleteAsync(this CommandContext ctx, DiscordEmbed embed, TimeSpan? time = null, string content = null)
        {
            time ??= TimeSpan.FromSeconds(10);

            var msg = await ctx.RespondAsync(content, embed: embed);

            Elapsed(msg, time);

            return msg;
        }

        public static async Task RespondAndDeleteAsync(this DiscordChannel ctx, DiscordEmbed embed, TimeSpan? time = null)
        {
            time ??= TimeSpan.FromSeconds(10);

            var msg = await ctx.SendMessageAsync(embed: embed);

            Elapsed(msg, time);
        }

        private static async void Elapsed(DiscordMessage message, TimeSpan? time)
        {
            await Task.Delay(time.Value);

            try
            {
                await message.DeleteAsync();
            }
            catch (Exception) { }
        }
    }
}
