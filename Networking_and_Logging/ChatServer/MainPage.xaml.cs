using Communications;
using HandlebarsDotNet.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    /// <summary>
    /// Author: Mason Sansom
    /// Partner: Druv Rachakonda
    /// Date: 3-Mar-2023
    /// Course:    CS 3500, University of Utah, School of Computing
    /// Copyright: CS 3500 and Mason Sansom - This work may not 
    ///            be copied for use in Academic Coursework.
    ///
    /// We, Mason Sansom and Druve Rachakonda, certify that we wrote this code from scratch and
    /// All references used in the completion of the assignments are cited 
    /// in the README file.
    ///
    /// File Contents
    /// 
    /// This class contains the GUI for the server object
    /// the server handles connections coming in from client objects and relays
    /// any messages sent to all clients connected to server. Default port is 11000
    /// </summary>

    public partial class MainPage : ContentPage
    {
        private Dictionary<Networking, string> clientDict;
        private Networking channel;
        private readonly ILogger<MainPage> _logger;

        public MainPage(ILogger<MainPage> logger)
        {
            _logger = logger;
            InitializeComponent();
            channel = new(_logger, onConnect, onDisconnect, onMessage, '\n');
            channel.WaitForClients(11000, true);
            clientDict = new Dictionary<Networking, string>();


            //From the following website to get IP: https://stackoverflow.com/questions/6803073/get-local-ip-address

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    serverIP.Text = "SERVER IP: " + ip.ToString();

                }
            }


            serverName.Text = "SERVER NAME: " + System.Environment.MachineName;
        }


        /// <summary>
        ///     When a client disconnects remove client from list and send a message
        ///     if server disconnects disconnect all clients and stop waiting for messages
        /// </summary>
        /// <param name="channel"></param>
        public void onDisconnect(Networking channel)
        {

            if (channel == this.channel)
            {
                lock (clientDict)
                {
                    foreach (Networking client in clientDict.Keys)
                    {
                        client.Disconnect();
                    }

                }
                return;
            }

            if (!clientDict.Keys.Contains(channel))
            {
                return;
            }
            string oldName = channel.ID + ": " + clientDict[channel];
            ClientList.Text = ClientList.Text.Replace(oldName, "");

            clientDict.Remove(channel);
        }

        /// <summary>
        ///     When a client connects to the server add it to the client list
        ///     then await messages
        /// </summary>
        /// <param name="channel"></param>
        public void onConnect(Networking channel)
        {
            lock (clientDict)
            {
                clientDict.Add(channel, channel.ID);
            }

            channel.AwaitMessagesAsync();
            //display in message widnow connection established
        }

        /// <summary>
        ///     When the server recieves a message check if its a command
        ///     and do the appropirate command or send message to all clients
        /// </summary>
        /// <param name="channel"> client who sent message </param>
        /// <param name="message"> message sent </param>
        public void onMessage(Networking channel, string message)
        {

            if (message.StartsWith("Command Participants"))
            {
                string partPacket = "Command Participants,";
                foreach (KeyValuePair<Networking, string> kvp in clientDict)
                {
                    partPacket += kvp.Key.ID + ",";
                }

                channel.Send(partPacket);
                return;

            }

            if (message.StartsWith("Command Name"))
            {
                string oldNameWithoutIP = channel.ID;
                Dispatcher.Dispatch(() => MessageList.Text = MessageList.Text += $"{oldNameWithoutIP} - {message} \n");
                string oldName = channel.ID + ": " + clientDict[channel];

                int i = message.IndexOf('e') + 2;
                string nameWithoutIP = message.Substring(i);
                string name = message.Substring(i) + ": " + clientDict[channel] + "\n";

                if (ClientList.Text is null)
                {
                    ClientList.Text = "";
                }

                string listOfNames = ClientList.Text;
                listOfNames = listOfNames.Replace(oldName, "");
                listOfNames = listOfNames.Replace("\r", "\n");
                listOfNames += name;


                ClientList.Text = listOfNames;


                channel.ID = nameWithoutIP;


                return;



            }

            Dispatcher.Dispatch(() => MessageList.Text = MessageList.Text += $"{channel.ID} - {message} \n");


            List<Networking> toRemove = new();
            List<Networking> toSendTo = new();



            //
            // Cannot have clients adding while we send messages, so make a copy of the
            // current list of clients.
            //
            lock (clientDict)
            {
                foreach (Networking client in clientDict.Keys)
                {
                    toSendTo.Add(client);
                }
            }

            _logger.LogInformation($"  Sending a message of size ({message.Length}) to {toSendTo.Count} clients");

            foreach (Networking client in toSendTo)
            {
                try
                {
                    client.Send($"{channel.ID} - {message}");
                }
                catch (Exception)
                {
                    toRemove.Add(client);
                }
            }

            lock (clientDict)
            {
                // update list of "current" clients by removing closed clients
                foreach (Networking client in toRemove)
                {
                    clientDict.Remove(client);
                }
            }

            toSendTo.Clear();
            toRemove.Clear();
        }


        /// <summary>
        ///     Disconnect the TcpClient
        /// </summary>
        /// <param name="sender"> unused </param>
        /// <param name="e"> unused </param>
        private void Disconnect(object sender, EventArgs e)
        {
            discon.IsVisible = false;
            recon.IsVisible = true;
            channel.StopWaitingForClients();
            channel.Disconnect();

        }

        /// <summary>
        ///     Restart the server after it has be shutdown
        /// </summary>
        /// <param name="sender"> unused </param>
        /// <param name="e"> unused </param>
        private void Restart(object sender, EventArgs e)
        {
          //  channel = new(_logger, onConnect, onDisconnect, onMessage, '\n');
            channel.WaitForClients(11000, true);
            discon.IsVisible = true;
            recon.IsVisible = false;
        }

    }
}