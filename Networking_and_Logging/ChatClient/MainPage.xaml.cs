﻿using Communications;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Channels;

namespace ChatClient;

public partial class MainPage : ContentPage
{

    private ObservableCollection<string> clientList;
    private ObservableCollection<string> messages;
    private Networking channel;

    public MainPage()
    {
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
        addMessageAndScroll($"Disconnected from {channel.ID}");
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
            channel = new Networking(NullLogger.Instance, onConnect, onDisconnect, onMessage, '\n');
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

    private void addMessageAndScroll(string message)
    {
        messages.Add(message);
        MessageList.ScrollTo(message, new ScrollToPosition(), false);
    }
}

