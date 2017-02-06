using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Diagnostics;
using TwitchCSharp.Clients;
using TwitchCSharp.Models;

namespace TwitchBot
{
    public partial class Form1 : Form
    {
        #region Variables
        private static string userName = "mahyar121bot";
        private static string password = "oauth:5h2ucjr1x8gt6po3wku709pntw399x";

        private static string TwitchClientID = "2napvua11blsl5z2lsehffuc7prg0g";
        TwitchReadOnlyClient APIClient = new TwitchReadOnlyClient(TwitchClientID);
        TwitchROChat ChatClient = new TwitchROChat(TwitchClientID);

        IrcClient irc = new IrcClient("irc.chat.twitch.tv", 6667, userName, password);
        NetworkStream serverStream = default(NetworkStream);
        string readData = "";
        Thread chatThread;

        bool pointSpamFilter = false;
        bool rankSpamFilter = false;
        List<CommandSpamUser> rankSpamUser = new List<CommandSpamUser>();
        List<CommandSpamUser> commandSpamUser = new List<CommandSpamUser>();

        List<string> BannedWords = new List<string> { "viewbot", "faggot", "boob", "suck", "penis", "boobs" };
        IniFile PointsIni = new IniFile(@"C:\Users\mahyar\Desktop\TwitchBot\TwitchBot\Points.ini");
        IniFile CoinsIni = new IniFile(@"C:\Users\mahyar\Desktop\TwitchBot\TwitchBot\Coins.ini");
        #endregion


        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            irc.joinRoom("mahyar121");
            chatThread = new Thread(getMessage);
            chatThread.Start();
            ViewerBoxTimer.Start();
            CommandSpamTimer.Start();
            RankSpamTimer.Start();
            AutoRepeatTimer.Start();
            LoyaltyPointTimer.Start();
            ViewerBoxTimer_Tick(null, null);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            irc.leaveRoom();
            serverStream.Dispose();
            Environment.Exit(0);
        }
      
        private void getMessage()
        {
            serverStream = irc.tcpClient.GetStream();
            int buffsize = 0;
            byte[] inStream = new byte[10025];
            buffsize = irc.tcpClient.ReceiveBufferSize;
            while(true)
            {
                try
                {
                    readData = irc.readMessage();
                    msg();
                }
                catch(Exception e)
                {

                }
            }
        }

        private void msg() // This is where everything is dealt with in chat. Commands, automatic timeout, etc.
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(msg));
            }
            else
            {
                string[] separator = new string[] { "#mahyar121 :" };
                string[] singlesep = new string[] { ":","!" };

                if(readData.Contains("PRIVMSG"))
                {
                    // :mahyar121!mahyar121@mahyar121.tmi.twitch.tv PRIVMSG #mahyar121 :asdf
                    string username = readData.Split(singlesep, StringSplitOptions.None)[1]; // grabs 1st occurrence after split
                    string message = readData.Split(separator, StringSplitOptions.None)[1]; // grabs 1st occurrence after #mahyar121 :

                    if(IsBannedWordFilter(username, message))
                    {
                        return;
                    }

                    if(message[0] == '!')
                    {
                        commands(username, message);
                    }


                    chatBox.Text = chatBox.Text + username + ": " + message + Environment.NewLine;

                    if(chatBox.Lines.Count() > 100)
                    {
                        var foos = new List<string>(chatBox.Lines);
                        foos.RemoveAt(0);
                        chatBox.Lines = foos.ToArray();
                    }
                }
                if (readData.Contains("PING"))
                {
                    irc.PingResponse();
                }
            }
        }

        private void commands(string username, string message)
        {
            string command = message.Split(new[] { ' ', '!' }, StringSplitOptions.None)[1]; //!command

            switch(command.ToLower())
            {
                case "twitter":
                    irc.sendChatMessage("Don't Forget to follow Mahyar's twitter. Twitter: https://twitter.com/Mahyar121 ");
                    break;
                case "joke":
                    joke();
                    break;
                case "add": //!add mahyar121 100
                    AddPointsCommand(username, message);
                    break;
                case "remove": //!remove mahyar121 100
                    RemovePointsCommand(username, message);
                    break;
                case "points":
                    CheckPoints(username);
                    break;
                case "rank":
                    CheckRank(username);
                    break;
                case "commands":
                    irc.sendChatMessage("I also have commands that you can use such as !joke, !rank, !points, !coins, !gamble");
                    break;
                case "coins":
                    CheckCoins(username);
                    break;
                case "gamble":
                    GambleCoinsCommand(username, message);
                    break;
                default:
                    irc.sendChatMessage("Sorry, that is not a command. Type !commands to see the list of commands you can use");
                    break;
            }
        }

        #region msc functions
        private void joke()
        {
            Random random = new Random();
            int choice = random.Next(1, 22);
            switch(choice)
            {
                case 1:
                    irc.sendChatMessage("I bought one of those tapes to teach you Spanish in your sleep. During the night, the tape skipped. Now I can only stutter in Spanish.");
                    break;
                case 2:
                    irc.sendChatMessage("I dreamt I was forced to eat a giant marshmallow. When I woke up, my pillow was gone.");
                    break;
                case 3:
                    irc.sendChatMessage("A neutron walks into a bar and orders a drink. When the neutron gets his drink, he asks, \"Bartender, how much do I owe you?\"" +
                        "The bartender replies, \"For you, neutron, no charge.\" ");
                    break;
                case 4:
                    irc.sendChatMessage("My friend thinks he is smart. He told me an onion is the only food that makes you cry, so I threw a coconut at his face.");
                    break;
                case 5:
                    irc.sendChatMessage("My sister bet me a hundred dollars I could not build a car out of spaghetti. You should've seen the look on her face as I drove pasta! ");
                    break;
                case 6:
                    irc.sendChatMessage("Where do animals go when their tails fall off? The retail store.");
                    break;
                case 7:
                    irc.sendChatMessage("Why can't you hear a pterodactyl going to the bathroom? Because the 'P' is silent.");
                    break;
                case 8:
                    irc.sendChatMessage("How does a train eat? It goes chew chew.");
                    break;
                case 9:
                    irc.sendChatMessage("What does a nosey pepper do? Gets jalapeno business!");
                    break;
                case 10:
                    irc.sendChatMessage("What do you call a fake noodle? An Impasta");
                    break;
                case 11:
                    irc.sendChatMessage("What do you call an alligator in a vest? An Investigator");
                    break;
                case 12:
                    irc.sendChatMessage("What's the difference between a guitar, and a fish? You can't tuna fish");
                    break;
                case 13:
                    irc.sendChatMessage("What do lawyers wear to court? Lawsuits!");
                    break;
                case 14:
                    irc.sendChatMessage("Why did the picture go to jail? Because it was framed");
                    break;
                case 15:
                    irc.sendChatMessage("What do you get when you cross fish and an elephant? Swimming trunks");
                    break;
                case 16:
                    irc.sendChatMessage("Why are frogs so happy? They eat whatever bugs them");
                    break;
                case 17:
                    irc.sendChatMessage("What do you call someone who is afraid of Santa? CLAUSterphobic ");
                    break;
                case 18:
                    irc.sendChatMessage("Why are pirates called pirates? Because they arrrr");
                    break;
                case 19:
                    irc.sendChatMessage("What did the triangle say to the circle? You are pointless!");
                    break;
                case 20:
                    irc.sendChatMessage("Why was the student's report card wet? It was below C level!");
                    break;
                case 21:
                    irc.sendChatMessage("What did the man lose his job at the orange juice factory? He couldn't concentrate");
                    break;
            }
        }

        private bool IsBannedWordFilter(string username, string message)
        {
            foreach(string word in BannedWords)
            {
                if(message.Contains(word))
                {
                    string command = "/timeout " + username + " 10";
                    irc.sendChatMessage(command);
                    irc.sendChatMessage(username + " has been timed out because they said a bad word.");
                    return true;
                }
            }
            return false;
        }

        private void AddPoints(string username, double points)
        {
            double finalnumber = 0;
            try
            {
                string[] separator = new string[] { @"\r\n" };
                username = username.Trim().ToLower();
                string pointsofuser = PointsIni.IniReadValue("#mahyar121." +username, "Points");
                double numberofpoints = double.Parse(pointsofuser);
                finalnumber = Convert.ToDouble(numberofpoints + points);
                if (finalnumber > 0)
                {
                    PointsIni.IniWriteValue("#mahyar121." + username, "Points", finalnumber.ToString());
                }
                if (finalnumber <= 0)
                {
                    PointsIni.IniWriteValue("#mahyar121." + username, "Points", "0");
                }
            }
            catch(Exception e)
            {
                if (points > 0)
                {
                    PointsIni.IniWriteValue("#mahyar121." + username, "Points", points.ToString());
                }
            }
        }

        private void AddCoins(string username, double points)
        {
            double finalnumber = 0;
            try
            {
                string[] separator = new string[] { @"\r\n" };
                username = username.Trim().ToLower();
                string coinsofuser = CoinsIni.IniReadValue("#mahyar121." + username, "Coins");
                double numberofcoins = double.Parse(coinsofuser);
                finalnumber = Convert.ToDouble(numberofcoins + points);
                if (finalnumber > 0)
                {
                    CoinsIni.IniWriteValue("#mahyar121." + username, "Coins", finalnumber.ToString());
                }
                if (finalnumber <= 0)
                {
                    CoinsIni.IniWriteValue("#mahyar121." + username, "Coins", "0");
                }
            }
            catch (Exception e)
            {
                if (points > 0)
                {
                    CoinsIni.IniWriteValue("#mahyar121." + username, "Coins", points.ToString());
                }
            }
        }

        private void AddPointsCommand(string user, string mes)
        {
            string username = user;
            string message = mes;
            if (username == "mahyar121")
            {
                string recipient = message.Split(new string[] { " " }, StringSplitOptions.None)[1];
                if (recipient[0] == '@') //!add @mahyar121 100
                {
                    recipient = recipient.Split(new[] { '@' }, StringSplitOptions.None)[1];
                }
                string pointsToTransferString = message.Split(new string[] { " " }, StringSplitOptions.None)[2];
                double pointstotransfer = 0;
                bool validNumber = double.TryParse(pointsToTransferString.Split(new[] { ' ' }, StringSplitOptions.None)[0], out pointstotransfer);
                if (validNumber && pointstotransfer > 0)
                {
                    AddPoints(recipient, pointstotransfer);
                    irc.sendChatMessage(recipient + " has gained " + pointstotransfer + " points!");
                }
            }
        }

        private void RemovePointsCommand(string user, string mes)
        {
            string username = user;
            string message = mes;
            if (username == "mahyar121")
            {
                string recipient = message.Split(new string[] { " " }, StringSplitOptions.None)[1];
                if (recipient[0] == '@') //!remove @mahyar121 100
                {
                    recipient = recipient.Split(new[] { '@' }, StringSplitOptions.None)[1];
                }
                string pointsToTransferString = message.Split(new string[] { " " }, StringSplitOptions.None)[2];
                double pointstotransfer = 0;
                bool validNumber = double.TryParse(pointsToTransferString.Split(new[] { ' ' }, StringSplitOptions.None)[0], out pointstotransfer);
                if (validNumber && pointstotransfer > 0)
                {
                    AddPoints(recipient, -pointstotransfer);
                    irc.sendChatMessage(recipient + " has lost " + pointstotransfer + " points!");
                }
            }
        }

        private void GambleCoinsCommand(string user, string mes)
        {
            Random random = new Random();
            int choice = random.Next(1, 101);
            string username = user;
            string message = mes;
            if (username == "mahyar121")
            {
                string coinsToTransferString = message.Split(new string[] { " " }, StringSplitOptions.None)[1];
                double pointstotransfer = 0;
                bool validNumber = double.TryParse(coinsToTransferString.Split(new[] { ' ' }, StringSplitOptions.None)[0], out pointstotransfer);
                if (validNumber && pointstotransfer > 0)
                {
                    if (choice > 40)
                    {
                        pointstotransfer = pointstotransfer * 2;
                        AddCoins(username, pointstotransfer);
                        irc.sendChatMessage(username + " has gained " + pointstotransfer + " coins!");
                    }
                    else
                    {
                        AddCoins(username, -pointstotransfer);
                        irc.sendChatMessage(username + " has lost " + pointstotransfer + " coins!");
                    }
                }
            }
        }

        private void CheckPoints(string user)
        {
            if(!pointSpamFilter)
            {
                foreach(CommandSpamUser singleuser in commandSpamUser)
                {
                    if(user == singleuser.username)
                    {
                        return;
                    }
                }
                pointSpamFilter = true;

                CommandSpamUser Cuser = new CommandSpamUser();
                Cuser.username = user;
                Cuser.timeOfMessage = DateTime.Now;
                commandSpamUser.Add(Cuser);
               
                string yourpoints = PointsIni.IniReadValue("#mahyar121." + user, "Points");
                if (yourpoints == "")
                {
                    irc.sendChatMessage("You don't have any points :(");
                }
                irc.sendChatMessage(user + " has " + yourpoints + " points!");
            }
            
        }

        private void CheckCoins(string username)
        {
            string yourcoins = CoinsIni.IniReadValue("#mahyar121." + username, "Coins");
            if (yourcoins == "")
            {
                irc.sendChatMessage("You don't have any coins :(");
                return;
            }
            irc.sendChatMessage(username + " has " + yourcoins + " coins!");
        }

        private void CheckRank(string username)
        {
            if (!rankSpamFilter)
            {
                foreach (CommandSpamUser singleuser in rankSpamUser)
                {
                    if (username == singleuser.username)
                    {
                        return;
                    }
                }
                rankSpamFilter = true;

                CommandSpamUser Cuser = new CommandSpamUser();
                Cuser.username = username;
                Cuser.timeOfMessage = DateTime.Now;
                rankSpamUser.Add(Cuser);

                string user = username;
                string yourpoints = PointsIni.IniReadValue("#mahyar121." + user, "Points");
                int num = Int32.Parse(yourpoints);

                // BRONZE
                if (num >= 0 && num < 300) { irc.sendChatMessage(user + " is currently Bronze V with " + yourpoints + " points, and needs " + (300 - num) + " points to rank up."); }
                if (num >= 300 && num < 600) { irc.sendChatMessage(user + " is currently Bronze IV with " + yourpoints + " points, and needs " + (600 - num) + " points to rank up."); }
                if (num >= 600 && num < 900) { irc.sendChatMessage(user + " is currently Bronze III with " + yourpoints + " points, and needs " + (900 - num) + " points to rank up."); }
                if (num >= 900 && num < 1200) { irc.sendChatMessage(user + " is currently Bronze II with " + yourpoints + " points, and needs " + (1200 - num) + " points to rank up."); }
                if (num >= 1200 && num < 1500) { irc.sendChatMessage(user + " is currently Bronze I with " + yourpoints + " points, and needs " + (1500 - num) + " points to rank up."); }
                // SILVER
                if (num >= 1500 && num < 1800) { irc.sendChatMessage(user + " is currently Silver V with " + yourpoints + " points, and needs " + (1800 - num) + " points to rank up."); }
                if (num >= 1800 && num < 2100) { irc.sendChatMessage(user + " is currently Silver IV with " + yourpoints + " points, and needs " + (2100 - num) + " points to rank up."); }
                if (num >= 2100 && num < 2400) { irc.sendChatMessage(user + " is currently Silver III with " + yourpoints + " points, and needs " + (2400 - num) + " points to rank up."); }
                if (num >= 2400 && num < 2700) { irc.sendChatMessage(user + " is currently Silver II with " + yourpoints + " points, and needs " + (2700 - num) + " points to rank up."); }
                if (num >= 2700 && num < 3000) { irc.sendChatMessage(user + " is currently Silver I with " + yourpoints + " points, and needs " + (3000 - num) + " points to rank up."); }
                // GOLD
                if (num >= 3000 && num < 3300) { irc.sendChatMessage(user + " is currently Gold V with " + yourpoints + " points, and needs " + (3300 - num) + " points to rank up."); }
                if (num >= 3300 && num < 3600) { irc.sendChatMessage(user + " is currently Gold IV with " + yourpoints + " points, and needs " + (3600 - num) + " points to rank up."); }
                if (num >= 3600 && num < 3900) { irc.sendChatMessage(user + " is currently Gold III with " + yourpoints + " points, and needs " + (3900 - num) + " points to rank up."); }
                if (num >= 3900 && num < 4200) { irc.sendChatMessage(user + " is currently Gold II with " + yourpoints + " points, and needs " + (4200 - num) + " points to rank up."); }
                if (num >= 4200 && num < 4500) { irc.sendChatMessage(user + " is currently Gold I with " + yourpoints + " points, and needs " + (4500 - num) + " points to rank up."); }
                // PLATINUM
                if (num >= 4500 && num < 4800) { irc.sendChatMessage(user + " is currently Platinum V with " + yourpoints + " points, and needs " + (4800 - num) + " points to rank up."); }
                if (num >= 4800 && num < 5100) { irc.sendChatMessage(user + " is currently Platinum IV with " + yourpoints + " points, and needs " + (5100 - num) + " points to rank up."); }
                if (num >= 5100 && num < 5400) { irc.sendChatMessage(user + " is currently Platinum III with " + yourpoints + " points, and needs " + (5400 - num) + " points to rank up."); }
                if (num >= 5400 && num < 5700) { irc.sendChatMessage(user + " is currently Platinum II with " + yourpoints + " points, and needs " + (5700 - num) + " points to rank up."); }
                if (num >= 5700 && num < 6000) { irc.sendChatMessage(user + " is currently Platinum I with " + yourpoints + " points, and needs " + (6000 - num) + " points to rank up."); }
                // DIAMOND
                if (num >= 6000 && num < 6300) { irc.sendChatMessage(user + " is currently Diamond V with " + yourpoints + " points, and needs " + (6300 - num) + " points to rank up."); }
                if (num >= 6300 && num < 6600) { irc.sendChatMessage(user + " is currently Diamond IV with " + yourpoints + " points, and needs " + (6600 - num) + " points to rank up."); }
                if (num >= 6600 && num < 6900) { irc.sendChatMessage(user + " is currently Diamond III with " + yourpoints + " points, and needs " + (6900 - num) + " points to rank up."); }
                if (num >= 6900 && num < 7200) { irc.sendChatMessage(user + " is currently Diamond II with " + yourpoints + " points, and needs " + (7200 - num) + " points to rank up."); }
                if (num >= 7200 && num < 7500) { irc.sendChatMessage(user + " is currently Diamond I with " + yourpoints + " points, and needs " + (7500 - num) + " points to rank up."); }
                // MASTER
                if (num >= 7500 && num < 7800) { irc.sendChatMessage(user + " is currently Master V with " + yourpoints + " points, and needs " + (7800 - num) + " points to rank up."); }
                if (num >= 7800 && num < 8100) { irc.sendChatMessage(user + " is currently Master IV with " + yourpoints + " points, and needs " + (8100 - num) + " points to rank up."); }
                if (num >= 8100 && num < 8400) { irc.sendChatMessage(user + " is currently Master III with " + yourpoints + " points, and needs " + (8400 - num) + " points to rank up."); }
                if (num >= 8400 && num < 8700) { irc.sendChatMessage(user + " is currently Master II with " + yourpoints + " points, and needs " + (8700 - num) + " points to rank up."); }
                if (num >= 8700 && num < 9000) { irc.sendChatMessage(user + " is currently Master I with " + yourpoints + " points, and needs " + (9000 - num) + " points to rank up."); }
                // GRANDMASTER
                if (num >= 9000 && num < 9300) { irc.sendChatMessage(user + " is currently GrandMaster V with " + yourpoints + " points, and needs " + (9300 - num) + " points to rank up."); }
                if (num >= 9300 && num < 9600) { irc.sendChatMessage(user + " is currently GrandMaster IV with " + yourpoints + " points, and needs " + (9600 - num) + " points to rank up."); }
                if (num >= 9600 && num < 9900) { irc.sendChatMessage(user + " is currently GrandMaster III with " + yourpoints + " points, and needs " + (9900 - num) + " points to rank up."); }
                if (num >= 9900 && num < 10200) { irc.sendChatMessage(user + " is currently GrandMaster II with " + yourpoints + " points, and needs " + (10200 - num) + " points to rank up."); }
                if (num >= 10200 && num < 10500) { irc.sendChatMessage(user + " is currently GrandMaster I with " + yourpoints + " points, and needs " + (10500 - num) + " points to rank up."); }
                // LEGEND
                if (num >= 10500) { irc.sendChatMessage(user + " is currently a Legend with " + yourpoints + " points."); }
            }
        }

        private void ViewerBoxTimer_Tick(object sender, EventArgs e)
        {
            ViewerListUpdate();
        }

        private void ViewerListUpdate()
        {
            ViewerBox.Items.Clear();
            Chatters AllChatters = ChatClient.GetChatters("mahyar121");
            int numberofchatters = ChatClient.GetChatterCount("mahyar121");
            

            foreach (string admin in AllChatters.Admins)
            {
                ViewerBox.Items.Add(admin + Environment.NewLine);
            }
            foreach (string staff in AllChatters.Staff)
            {
                ViewerBox.Items.Add(staff + Environment.NewLine);
            }
            foreach (string globalmod in AllChatters.GlobalMods)
            {
                ViewerBox.Items.Add(globalmod + Environment.NewLine);
            }
            foreach (string moderator in AllChatters.Moderators)
            {
                ViewerBox.Items.Add(moderator + Environment.NewLine);
            }
            foreach (string viewer in AllChatters.Viewers)
            {
                ViewerBox.Items.Add(viewer + Environment.NewLine);
            }
        }

        private void LoyaltyPointTimer_Tick(object sender, EventArgs e)
        {
            foreach (string username in ViewerBox.Items)
            {
                AddPoints(username, 1);
                AddCoins(username, 1);
            }
        }

        private void chatBox_TextChanged(object sender, EventArgs e)
        {
            chatBox.SelectionStart = chatBox.Text.Length;
            chatBox.ScrollToCaret();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            irc.sendChatMessage(BotChatBox.Text);
            BotChatBox.Clear();
        }

        private void BotChatBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == Convert.ToChar(Keys.Return))
            {
                irc.sendChatMessage(BotChatBox.Text);
                BotChatBox.Clear();
            }
            
        }

        private void CommandSpamTimer_Tick(object sender, EventArgs e)
        {
            pointSpamFilter = false;
            List<CommandSpamUser> temp = commandSpamUser;
            foreach(CommandSpamUser user in temp)
            {
                TimeSpan duration = DateTime.Now - user.timeOfMessage;
                if(duration > TimeSpan.FromSeconds(10))
                {
                    commandSpamUser.Remove(user);
                    return;
                }
            }
            
        }

        private void RankSpamTimer_Tick(object sender, EventArgs e)
        {
            rankSpamFilter = false;
            List<CommandSpamUser> temp2 = rankSpamUser;
            foreach (CommandSpamUser user in temp2)
            {
                TimeSpan duration = DateTime.Now - user.timeOfMessage;
                if (duration > TimeSpan.FromSeconds(10))
                {
                    rankSpamUser.Remove(user);
                    return;
                }
            }
        }

        private void AutoRepeatTimer_Tick(object sender, EventArgs e)
        {
            irc.sendChatMessage("Don't Forget to follow Mahyar's twitter. Twitter: https://twitter.com/Mahyar121 ");
            irc.sendChatMessage("I also have commands that you can use such as !joke, !rank, !points, !coins, !gamble");
        }
        #endregion


    }

    #region classes
    class IrcClient
    {
        private string userName;
        private string channel;

        public TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string userName, string password)
        {
            tcpClient = new TcpClient(ip, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
            outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            outputStream.WriteLine("CAP REQ :twitch.tv/commands");
            outputStream.Flush();
        }

        public void joinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void leaveRoom()
        {
            outputStream.Close();
            inputStream.Close();
        }

        public void sendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void sendChatMessage(string message)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void PingResponse()
        {
            sendIrcMessage("PONG tmi.twitch.tv\r\n");
        }

        public string readMessage()
        {
            string message = "";
            message = inputStream.ReadLine();
            return message;
        }
    }

    class IniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public IniFile(string IniPath)
        {
            path = IniPath;
        }

        public void IniWriteValue(string Section, string Key, string value)
        {
            WritePrivateProfileString(Section, Key, value, this.path);
        }

        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }
    }

    class CommandSpamUser
    {
        public string username;
        public DateTime timeOfMessage;
    }
    #endregion

}
