using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Discord;
using Discord.WebSocket;
using System.Text;
using System.IO;

using CoreDynamic;
using System.Reflection;

using log4net;
using log4net.Config;
using Discord.Rest;

namespace MorkoBot
{
    public class MorkoBot
    {
        private static string Token = "REDACTED";
        public static ulong ServerID = 291497857725366272;
        private DiscordSocketClient Client;
        private JsonWriter MapWriter = new JsonWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\mapmodule.json", true);
        private JsonWriter IncWriter = new JsonWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\incmodule.json", true);
        private PermissionProvider Provider;
        private static readonly ILog LogProvider = LogManager.GetLogger("MorkoBot");
        private RestUserMessage LastCounterMessage = null;

        private List<BotCommand> BotCommands = new List<BotCommand>();
        private Dictionary<SocketUser, DateTime> IncCooldownUsers = new Dictionary<SocketUser, DateTime>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
            => new MorkoBot().MorkoAsync().GetAwaiter().GetResult();

        public async Task MorkoAsync()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\logconfig.xml"));

            DiscordSocketConfig dsc = new DiscordSocketConfig
            {
                MessageCacheSize = 50,
            };

            this.Client = new DiscordSocketClient(dsc);
            this.Provider = new PermissionProvider(Client);
            
            await this.RegisterCommands();

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            Client.Log += Log;
            Client.MessageReceived += SentMessage;
            Client.MessageUpdated += Client_MessageUpdated;
            Client.Ready += Client_Ready;
            Client.UserJoined += Client_UserJoined;
            Client.UserLeft += Client_UserLeft;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MorkoBotPanel(Client));

            await Task.Delay(-1);
        }

        private Task Client_UserLeft(SocketGuildUser arg)
        {
            if (arg.Guild.Id == ServerID)
            {
                LogProvider.Info("User " + arg.Username + " just left the server.");
            }

            return Task.CompletedTask;
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            if (arg.Guild.Id == ServerID)
            {
                LogProvider.Info("User " + arg.Username + " just joined the server.");
                await (await arg.CreateDMChannelAsync()).SendMessageAsync("Welcome to the INFRA Discord Server! We hope you enjoy your stay.\nBefore posting, please read the pinned rules in #announcements carefully.\nIf you're a mapper, playtester, or developer for INFRA, please ping a Server Admin or Loiste Staff member to be added to the group!");
            }
        }

        private async Task Client_Ready()
        {
            await Client.SetStatusAsync(UserStatus.DoNotDisturb);
        }

        private Task RegisterCommands()
        {
            BotCommandFactory bcf = this.GetCommandFactory();

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                await ms.Channel.SendMessageAsync("Tule B2: ään");
            }, "!awaken"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                IDMChannel dm = await ms.Author.CreateDMChannelAsync();
                await dm.SendMessageAsync("MörköBot 1.2 Command List\n" +
                    Strings.CodeBlock +
                    "!help: Shows this command list.\n" +
                    "!listmaps: Gets a list of all INFRA maps.\n" +
                    "!mapinfo <mapname>: Shows map information for the specified map.\n" +
                    "!teddy: I wonder what this does?\n" +
                    "!increment: Increments the Event Counter.\n" +
                    "!vroleadd <role> <user>: Adds a Vanity Role to the specified user.\n" +
                    "!vroledel <role> <user>: Removes a Vanity Role from the specified user.\n" + Strings.CodeBlock +
                    "Not all commands are listed here. Some commands are restricted, or are only available to those who know them...");
            }, "!help"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Strings.CodeBlock);
                foreach (KeyValuePair<string, dynamic> mapname in MapWriter.Read()["maplist"])
                {
                    if ((Provider.IsPermitted(ms.Author, new List<string>() { "Playtester" }) && mapname.Value["playtest"]) || !mapname.Value["playtest"])
                    {
                        sb.Append(mapname.Key + ": " + mapname.Value["bspname"] + "\n");
                    } 
                }
                sb.Append(Strings.CodeBlock);

                await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync(sb.ToString());
            }, "!listmaps"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                string[] DepCommands = ms.Content.Split(' ');

                if (DepCommands.Length >= 2)
                {
                    Dictionary<string, dynamic> maplist = MapWriter.Read()["maplist"];
                    if (MapWriter.Read()["maplist"].ContainsKey(DepCommands[1]))
                    {
                        Dictionary<string, dynamic> mapdata = maplist[DepCommands[1]];

                        if ((mapdata["playtest"] == true && Provider.IsPermitted(ms.Author, new List<string>() { "Playtester" })) || mapdata["playtest"] == false)
                        {
                            StringBuilder mdsb = new StringBuilder();

                            foreach (KeyValuePair<string, dynamic> meta in mapdata["infra_metadata"])
                            {
                                string valtitle;

                                switch(meta.Key)
                                {
                                    case "camera_targets":
                                        valtitle = "Camera Targets";
                                        break;
                                    case "corruption_targets":
                                        valtitle = "Corruption Targets";
                                        break;
                                    case "repair_targets":
                                        valtitle = "Repair Targets";
                                        break;
                                    case "mistake_targets":
                                        valtitle = "Mistake Targets";
                                        break;
                                    case "geocaches":
                                        valtitle = "Geocaches";
                                        break;
                                    case "water_flow_meter_targets":
                                        valtitle = "Flow Meters";
                                        break;
                                    default:
                                        valtitle = meta.Key;
                                        break;
                                }

                                if (meta.Value.ToString() == "-1")
                                {
                                    mdsb.Append(valtitle + ": [REDACTED]\n");
                                }
                                else
                                {
                                    mdsb.Append(valtitle + ": " + meta.Value.ToString() + "\n");
                                }
                            }

                            string mapinfo = "Map information for " + DepCommands[1] + "\n" + Strings.CodeBlock +
                            "BSP Name: " + mapdata["bspname"] + "\n" +
                            "In Playtest: " + mapdata["playtest"].ToString() + "\n" +
                            "Chapter: " + mapdata["chapter"].ToString() + "\n" +
                            mdsb.ToString() + Strings.CodeBlock;

                            if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\map_thumbs\" + DepCommands[1] + ".png"))
                            {
                                await (await ms.Author.CreateDMChannelAsync()).SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\map_thumbs\" + DepCommands[1] + ".png", mapinfo);
                            } else
                            {
                                await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync(mapinfo + "\nNo thumbnail found for this map.");
                            }
                            
                        }
                        else
                        {
                            await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync(Strings.NoPermission);
                        }
                    }
                    else
                    {
                        await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("The map specified does not exist.");
                    }
                }
                else
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("Incorrect command usage. Correct usage is !mapinfo <mapname>.");
                }
            }, "!mapinfo"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                await ms.Channel.SendMessageAsync("<:tbear:329886534360760321>");
            }, "!teddy"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                string[] DepCommands = ms.Content.Split(' ');
                if (DepCommands.Length >= 2 && DepCommands[1] == "4027")
                {
                    await ms.Channel.SendMessageAsync("Tunnel door access permitted. Welcome, Administrator. The truth is in the tunnels.");
                }
                else
                {
                    await ms.Channel.SendMessageAsync("Error: Tunnel door control access denied. Invalid PIN.");
                }
                
            }, "!openb2"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                if (Provider.IsPermitted(ms.Author, new List<string>() { "Playtester" }) && ms.Channel.Name == "playtesting")
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("Hello, Playtester! The playtest code is " + Strings.PlaytestCode + ". Please do not share this with anyone!");
                }
                else
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("This command only works if you are a playtester executing this command in #playtesting.");
                }
            }, "!ptcode"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                Dictionary<string, dynamic> writer = IncWriter.Read();
                int count;
                int maxcount;

                if (writer.ContainsKey("count"))
                {
                    count = Convert.ToInt32(writer["count"]);
                    maxcount = Convert.ToInt32(writer["maxcount"]);
                }
                else
                {
                    Dictionary<string, dynamic> rd = new Dictionary<string, dynamic>();
                    rd.Add("count", 0);
                    rd.Add("maxcount", 50);
                    IncWriter.WriteAll(rd);
                    count = 0;
                    maxcount = 50;
                }

                TimeSpan cooldown = new TimeSpan(1, 0, 0);
                if (IncCooldownUsers.ContainsKey(ms.Author) && IncCooldownUsers[ms.Author].Add(cooldown) > DateTime.Now)
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("You've recently incremented the counter. You need to wait another " + (IncCooldownUsers[ms.Author].Add(cooldown) - DateTime.Now).ToString("%m") + " minutes before incrementing the counter again.");
                }
                else
                {
                    if (count >= maxcount)
                    {  
                        IncCooldownUsers.Add(ms.Author, DateTime.Now);
                        Dictionary<string, dynamic> rd = new Dictionary<string, dynamic>();
                        rd.Add("count", 0);
                        rd.Add("maxcount", maxcount * 2);
                        IncWriter.WriteAll(rd);

                        Random rand = new Random();
                        switch(3)
                        {
                            case 1:
                                await ms.Channel.SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\counter\arctarus1.png", "The truth is in the tunnels, Robin.");
                                break;
                            case 2:
                                await ms.Channel.SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\counter\arctarus1.png", "This city is nothing but corruption, greed, and power. It must be purged.");
                                break;
                            case 3:
                                await ms.Channel.SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\counter\arctarus1.png", "We sealed up that tunnel years ago. Bricked up the entrance to B2..");
                                await Task.Delay(5000);
                                await ms.Channel.SendMessageAsync("Please wait. Command override C63 initiated. Data will be printed as it is recieved.");
                                await Task.Delay(5000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 20 seconds to singularity.");
                                await Task.Delay(10000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 10 seconds to singularity.");
                                await Task.Delay(5000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 5 seconds to singularity.");
                                await Task.Delay(1000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 4 seconds to singularity.");
                                await Task.Delay(1000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 3 seconds to singularity.");
                                await Task.Delay(1000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 2 seconds to singularity.");
                                await Task.Delay(1000);
                                await ms.Channel.SendMessageAsync("Error: Reactor core overheating. Approximately 1 seconds to singularity.");
                                await Task.Delay(1000);
                                await ms.Channel.SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\counter\chernobyl.gif", "Singularity.");
                                await Task.Delay(2000);
                                await ms.Channel.SendMessageAsync("Command override released. Resuming automatic control.");
                                await Task.Delay(2000);
                                break;
                            case 4:
                                await ms.Channel.SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\counter\arctarus1.png", "What happened to the survivors? Why is everyone dead?!");
                                break;
                            case 5:
                                await ms.Channel.SendFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @".\..\..\data\counter\arctarus1.png", "I need to know the truth. Someone, please help me!");
                                break;
                        }

                        await ms.Channel.SendMessageAsync("The counter has been reset to 0. The next counter goal is " + Convert.ToString(maxcount * 2) + ".");
                    }
                    else
                    {
                        if (LastCounterMessage != null)
                        {
                            await LastCounterMessage.DeleteAsync();
                        }

                        IncCooldownUsers[ms.Author] = DateTime.Now;
                        Dictionary<string, dynamic> rd = new Dictionary<string, dynamic>();
                        rd.Add("count", count + 1);
                        rd.Add("maxcount", maxcount);
                        IncWriter.WriteAll(rd);
                        LastCounterMessage = await ms.Channel.SendMessageAsync("The counter was incremented from " + Convert.ToString(count) + " to " + Convert.ToString(count + 1) + " by " + ms.Author.Username + ". " + Convert.ToString(maxcount - (count + 1)) + " increments remaining until the next special event.");
                    } 
                }
            }, "!increment"));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                string[] cmd = ms.Content.Split(' ');
                if (cmd.Length >= 3)
                {
                    Dictionary<string, Dictionary<ulong, bool>> roles = new Dictionary<string, Dictionary<ulong, bool>>
                    {
                        { "Mapper", new Dictionary<ulong, bool> { { 308488132775510017, false } } },
                        { "Playtester", new Dictionary<ulong, bool> { { 308345313318535169, true } } }
                    };

                    if (roles.ContainsKey(cmd[1])) {
                        SocketUser su = null;

                        foreach (SocketGuildUser user in Client.GetGuild(ServerID).Users)
                        {
                            if (user.Username == cmd[2])
                            {
                                su = user;
                            }
                        }

                        if (su != null)
                        {
                            if (roles[cmd[1]].First().Value == false || (roles[cmd[1]].First().Value == true && Provider.IsPermitted(ms.Author, new List<string> {"Server Admin"})))
                            {
                                await Client.GetGuild(ServerID).GetUser(su.Id).AddRoleAsync(Client.GetGuild(ServerID).GetRole(roles[cmd[1]].First().Key));
                                await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("Successfully added the role " + cmd[1] + " to the user " + cmd[2] + ".");
                            }
                            else
                            {
                                await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("The role specified can only be added by server administrators with the Loiste Staff, Server Admin, or Global Admin roles.");
                            }
                        }
                        else
                        {
                            await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("The user specified does not exist. Please ensure you are using their Discord username and not their nickname.");
                        } 
                    }
                    else
                    {
                        await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("The role does not exist or is not in the list of permitted roles for this command.");
                    }
                }
                else
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("Incorrect command usage. Correct usage is !vroleadd <role> <user>.");
                }
            }, "!vroleadd", new List<string>() { "Loiste Staff", "Server Admin", "Global Admin", "Server Moderator" }));

            this.BotCommands.Add(bcf.NewCommand(async (ms, Client) =>
            {
                string[] cmd = ms.Content.Split(' ');
                if (cmd.Length >= 3)
                {
                    Dictionary<string, Dictionary<ulong, bool>> roles = new Dictionary<string, Dictionary<ulong, bool>>
                    {
                        { "Mapper", new Dictionary<ulong, bool> { { 308488132775510017, false } } },
                        { "Playtester", new Dictionary<ulong, bool> { { 308345313318535169, true } } }
                    };

                    if (roles.ContainsKey(cmd[1]))
                    {
                        SocketUser su = null;

                        foreach (SocketGuildUser user in Client.GetGuild(ServerID).Users)
                        {
                            if (user.Username == cmd[2])
                            {
                                su = user;
                            }
                        }

                        if (roles[cmd[1]].First().Value == false || (roles[cmd[1]].First().Value == true && Provider.IsPermitted(ms.Author, new List<string> { "Server Admin" })))
                        {
                            await Client.GetGuild(ServerID).GetUser(su.Id).RemoveRoleAsync(Client.GetGuild(ServerID).GetRole(roles[cmd[1]].First().Key));
                            await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("Successfully removed the role " + cmd[1] + " from the user " + cmd[2] + ".");
                        }
                        else
                        {
                            await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("The role specified can only be removed by server administrators with the Loiste Staff, Server Admin, or Global Admin roles.");
                        }
                    }
                    else
                    {
                        await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("The role does not exist or is not in the list of permitted roles for this command.");
                    }
                }
                else
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync("Incorrect command usage. Correct usage is !vroledel <role> <user>.");
                }

            }, "!vroledel", new List<string>() { "Loiste Staff", "Server Admin", "Global Admin" }));

            return Task.CompletedTask;
        }

        private BotCommandFactory GetCommandFactory()
        {
            return new BotCommandFactory(Client, LogProvider);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task SentMessage(SocketMessage ms)
        {
            if (ms.Content.ToCharArray().First() == '!')
            {
                bool ValidCommand = false;

                foreach (BotCommand cmd in this.BotCommands)
                {
                    if (cmd.Command == ms.Content.Split(' ')[0])
                    {
                        ValidCommand = true;
                        await cmd.Execute(ms);
                    }
                }

                if (!ValidCommand)
                {
                    await (await ms.Author.CreateDMChannelAsync()).SendMessageAsync(Strings.CommandNotFound);
                    LogProvider.Info("User " + ms.Author.Username + " couldn't execute the command " + ms.Content + " because it was not found.");
                    await ms.DeleteAsync();
                }
            } 
        }

        private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            IMessage recv = await arg1.GetOrDownloadAsync();

             if (recv.Content != arg2.Content)
            {
                await this.SentMessage(arg2);
            }
        }
    }
}
