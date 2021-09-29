using MadDoc.Settings;
using MadDoc.Infrastructure;

namespace MadDoc
{
    class Program
    {
        public static void Main()
        {
            AppSettings.Initializate();
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
