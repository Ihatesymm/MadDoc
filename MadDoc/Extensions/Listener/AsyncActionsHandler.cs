using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using DSharpPlus;
using MadDoc.Infrastructure;

namespace MadDoc.Extensions
{
    internal static class AsyncActionsHandler
    {
        public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; }

        public static void InstallListeners(DiscordClient client, Bot bot)
        {
            ListenerMethods =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                from method in type.GetMethods()
                let attribute = method.GetCustomAttribute(typeof(AsyncActionsAttribute), true)
                where attribute != null
                select new ListenerMethod { Method = method, Attribute = attribute as AsyncActionsAttribute };
            
            foreach (var listener in ListenerMethods)
            {
                listener.Attribute.Register(bot, client, listener.Method);
            }
        }
    }

    internal class ListenerMethod
    {
        public MethodInfo Method { get; internal set; }
        public AsyncActionsAttribute Attribute { get; internal set; }
    }
}
