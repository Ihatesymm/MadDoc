using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using MadDoc.Entities;
using MadDoc.Extensions;
using MadDoc.Settings;
using System;
using System.Threading.Tasks;

namespace MadDoc.Handlers
{
    public static class MessagesHandler
    {
        [AsyncActions(EventTypes.CommandErrored)]
        public static async Task ErrorSend(CommandsNextExtension sender, CommandErrorEventArgs result)
        {
            try
            {
                await result.Context.Message.DeleteAsync();
                await result.Context.RespondErrorAndDeleteAsync("Команда завершила свою работу c ошибкой", result.Exception.Message, content: result.Context.User.Mention);
            }
            catch (Exception) { }
        }

        [AsyncActions(EventTypes.MessageCreated)]
        public static async Task MessageRecerved(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;

            if (e.Channel.Id != AppSettings.CommandsChannel)
            {
                TextChannelsInfo tci = new TextChannelsInfo(e.Channel.Id, e.Author.Id);
                tci.LastMessageDate = DateTimeOffset.Now;
                tci.MessagesCount++;
            }

            if (e.Channel.Id == AppSettings.CommandsChannel &&
                !e.Message.Content.StartsWith("!"))
                await e.Message.DeleteAsync();
        }
    }
}
