using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBot
{
    class PermissionProvider
    {
        private DiscordSocketClient Client;

        public static List<string> AdminRoleList = new List<string>
            {
                "Global Admin",
                "Server Admin",
                "Loiste Staff",
            };

        public PermissionProvider(DiscordSocketClient dc)
        {
            this.Client = dc;
        }

        public IReadOnlyCollection<SocketRole> GetPermissions(SocketGuildUser sg)
        {
            return sg.Roles;
        }

        public bool IsPermitted(SocketUser sg, List<string> PermittedRoles)
        {
            bool Permitted = false;
            List<string> UserPermissions = new List<string>();
            List<string> PermRoles = PermittedRoles.Concat(AdminRoleList).ToList();

            foreach (SocketRole sr in this.GetPermissions(Client.GetGuild(MorkoBot.ServerID).GetUser(sg.Id)))
            {
                UserPermissions.Add(sr.Name);
            }

            foreach (string Permission in PermittedRoles)
            {
                if (PermRoles.Contains(Permission))
                {
                    Permitted = true;
                }
            }

            return Permitted;
        }
    }
}
