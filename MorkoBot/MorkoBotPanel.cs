using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Discord.Webhook;

namespace MorkoBot
{
    public partial class MorkoBotPanel : Form
    {
        private DiscordSocketClient Client;

        public MorkoBotPanel(DiscordSocketClient sc)
        {
            InitializeComponent();

            this.Client = sc;
        }

        private void MorkoBotPanel_Load(object sender, EventArgs e)
        {

        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            Client.GetGuild(291497857725366272).DefaultChannel.SendMessageAsync(ChatBox.Text);
        }
    }
}
