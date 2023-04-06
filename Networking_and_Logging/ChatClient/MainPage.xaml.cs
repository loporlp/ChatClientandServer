using Communications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Channels;

namespace ChatClient;
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
/// This class contains the GUI for the Chat client.
/// The Gui controls what is displayed when a client connects to a server and sends 
/// messages, also displays any messages it recieves from the server
/// </summary>
public partial class MainPage : ContentPage
{

    private ObservableCollection<string> clientList;
    private ObservableCollection<string> messages;
    private Networking channel;
    private readonly ILogger<MainPage> _logger;

    public MainPage(ILogger<MainPage> logger)
    {
        _logger = logger;
        InitializeComponent();
        clientList = new ObservableCollection<string>();
        messages = new ObservableCollection<string>();
        MessageList.ItemsSource = messages;
        ClientList.ItemsSource = clientList;
    }

    /// <summary>
    ///     Method to be called when Client receives a message
    /// </summary>
    /// <param name="channel"> the channel the message is recieved on</param>
    /// <param name="message"> the message recieved</param>
    void onMessage(Networking channel, string message)
    {
        //Check if message is a command and update clientList if it is
        if(message.StartsWith("Command Participants"))
        {
            string[] participants = message.Split(',');
            clientList.Clear();
            for(int i = 1; i < participants.Length; i++)
            {
                clientList.Add(participants[i]);
            }

            ClientList.ScrollTo(participants[participants.Length - 1], new ScrollToPosition(), true);
            return;
        }

        addMessageAndScroll(message);
    }

    /// <summary>
    ///     Method to be called when client connects to server
    /// </summary>
    /// <param name="channel"> the channel connected to </param>
    void onConnect(Networking channel)
    {
        this.channel.Send("Command Name " + this.name.Text.Trim());
    }

    /// <summary>
    ///     Method to be called when client is disconnected from server
    /// </summary>
    /// <param name="channel"></param>
    void onDisconnect(Networking channel)
    {
        addMessageAndScroll("Disconnected from Server");
        connectButton.IsVisible = true;
        connectLabel.IsVisible = false;
    }

    /// <summary>
    ///     Attempts to connect to server when button is clicked
    /// </summary>
    /// <param name="sender"> unused </param>
    /// <param name="e"> unused </param>
    private void connectToServer(object sender, EventArgs e)
    {
        if(name.Text is null)
        {
            nameLabel.TextColor = new Color(255, 0, 0);
            return;
        }
        nameLabel.TextColor = new Color(0, 0, 0);

        addMessageAndScroll("Attempting to Connect to Server");
        
        try
        {
            channel = new Networking(_logger, onConnect, onDisconnect, onMessage, '\n');
            channel.Connect(address.Text, 11000);
            addMessageAndScroll("Connected To Server!");
            channel.AwaitMessagesAsync(infinite: true);
            connectButton.IsVisible = false;
            connectLabel.IsVisible = true;

        }
        catch (Exception)
        {
            addMessageAndScroll("Server Gone :(");
        }
    }

    /// <summary>
    ///     Populates participant list when button is clicked
    /// </summary>
    /// <param name="sender"> unused </param>
    /// <param name="e"> unused </param>
    private void retrieveParticipants(object sender, EventArgs e)
    {
        try
        {
            channel.Send("Command Participants");
        }
        catch (Exception)
        {
            addMessageAndScroll("Server Gone :(");
        }
    }

    /// <summary>
    ///     Sends a message to the server through the network object
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void messageComplete(object sender, EventArgs e)
    {
        try
        {
            channel.Send(send.Text);
        }
        catch (Exception)
        {
            addMessageAndScroll("Server Gone :(");
        }

        send.Text = string.Empty;

    }

    /// <summary>
    ///     Helper method that adds a message
    ///     to the message list then auto scrolls to it
    /// </summary>
    /// <param name="message"> message to add </param>
    private void addMessageAndScroll(string message)
    {
        messages.Add(message);
        MessageList.ScrollTo(message, new ScrollToPosition(), false);
    }
}

