using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace MorkoBot
{
    /// <summary>
    /// Class which registers commands.
    /// </summary>
    class BotCommand
    {
        /// <summary>
        /// Delegate which holds the command to execute.
        /// </summary>
        /// <param name="ms">The passed chat message.</param>
        /// <param name="dc">The DiscordSocketClient instance.</param>
        /// <returns></returns>
        public delegate Task ExecutionEvent(SocketMessage ms, DiscordSocketClient dc);

        /// <summary>
        /// Variable to store the delegate.
        /// </summary>
        private ExecutionEvent ActionableEvent;

        /// <summary>
        /// Variable to store the client instance.
        /// </summary>
        private DiscordSocketClient Client;

        /// <summary>
        /// The registered command.
        /// </summary>
        public string Command;

        private List<string> PermittedRoles = new List<string>();

        private log4net.ILog LogProvider;

        /// <summary>
        /// Initializes the class instance.
        /// </summary>
        /// <param name="ex">The ExecutionEvent delegate to pass.</param>
        /// <param name="Command">The command to register.</param>
        /// <param name="dc">The DiscordSocketClient instance to pass.</param>
        public BotCommand(ExecutionEvent ex, string Command, DiscordSocketClient dc, List<string> perms, log4net.ILog provider)
        {
            this.ActionableEvent = ex;
            this.Client = dc;
            this.Command = Command;
            this.LogProvider = provider;

            PermittedRoles = perms;
            PermittedRoles.Add("Global Admin");
            PermittedRoles.Add("Server Admin");
            PermittedRoles.Add("Loiste Staff");
        }

        /// <summary>
        /// Executes the message.
        /// </summary>
        /// <param name="ms">The SocketMessage passed from the client.</param>
        public async Task Execute(SocketMessage ms)
        {
            PermissionProvider perm = new PermissionProvider(Client);

            if (perm.IsPermitted(ms.Author, this.PermittedRoles))
            {
                LogProvider.Info("User " + ms.Author.Username + " just executed the command " + ms.Content + ".");
                await this.ActionableEvent(ms, this.Client);
            }
            else
            {
                LogProvider.Info("User " + ms.Author.Username + " couldn't execute the command " + ms.Content + " because they didn't have the required roles to do so.");
                await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync(Strings.NoPermission);
            }

            await ms.DeleteAsync();
        }
    }
}
