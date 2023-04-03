using Communications;
using HandlebarsDotNet.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{

   
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private Dictionary<Networking, string> clientDict;
        private Networking channel;

        public MainPage()
        {
            InitializeComponent();
            channel = new(NullLogger.Instance, onConnect, onDisconnect, onMessage, '\n');
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


            serverName.Text = "SERVER NAME: " +  System.Environment.MachineName;
        }


  
        public void onDisconnect(Networking channel)
        {

            string oldName = channel.ID + ": " + clientDict[channel];
            ClientList.Text = ClientList.Text.Replace(oldName, "");

            clientDict.Remove(channel);
        }

        public void onConnect(Networking channel) 
        {
            lock (clientDict)
            {
              clientDict.Add(channel, channel.ID);
            }
            //display in message widnow connection established
        }

        public void onMessage(Networking channel, string message)
        {

            if(message.StartsWith("Command Participants"))
            {
                string partPacket = "Command Participants,";
                foreach(KeyValuePair<Networking,string> kvp in clientDict)
                {
                    partPacket += kvp.Key.ID + ",";
                }

                channel.Send(partPacket);
                return;

            }

            if(message.StartsWith("Command Name"))
            {
                string oldNameWithoutIP = channel.ID;
                Dispatcher.Dispatch(() => MessageList.Text = MessageList.Text += $"{oldNameWithoutIP}: {message} \n");
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
                listOfNames = listOfNames.Replace("\r", "");
                listOfNames += name + "\n";


                ClientList.Text = listOfNames;


                channel.ID = nameWithoutIP;


                return;
                


            }

            Dispatcher.Dispatch(() => MessageList.Text = MessageList.Text += $"{channel.ID}: {message} \n");


            List<Networking> toRemove = new();
            List<Networking> toSendTo = new();

        
              
                //
                // Cannot have clients adding while we send messages, so make a copy of the
                // current list of clients.
                //
                lock (clientDict)
                {
                    foreach(Networking client in clientDict.Keys)
                    {
                        toSendTo.Add(client);
                    }
                }

                // Iterate over "saved" list of clients
                //
                // Question: Why can't we lock clients around this loop?
                //
                Console.WriteLine($"  Sending a message of size ({message.Length}) to {toSendTo.Count} clients");

                foreach (Networking client in toSendTo)
                {
                    try
                    {
                      client.Send($"{channel.ID}: {message}");
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



        private void Disconnect(object sender, EventArgs e)
        {
            channel.StopWaitingForClients();
            channel.Disconnect();
        }

    }
}