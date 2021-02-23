using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MorkoBot.BotCommand;

namespace MorkoBot
{
    class BotCommandFactory
    {
        private DiscordSocketClient Client;
        private log4net.ILog logger;
        public BotCommandFactory(DiscordSocketClient dsc, log4net.ILog log)
        {
            this.Client = dsc;
            this.logger = log;
        }

        public BotCommand NewCommand(ExecutionEvent ex, string command, List<string> permissions = null)
        {
            if (permissions != null)
            {
                return new BotCommand(ex, command, Client, permissions, logger);
            }
            else
            {
                List<string> PermList = new List<string>();
                PermList.Add("@everyone");
                return new BotCommand(ex, command, Client, PermList, logger);
            }
            
        }
    }
}
