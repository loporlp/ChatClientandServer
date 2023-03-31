using Communications;
using Microsoft.Extensions.Logging.Abstractions;
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

    void onMessage(Networking channel, string message)
    {
        addMessageAndScroll(message);
    }

    void onConnect(Networking channel)
    {
        this.channel.Send("Command Name " + this.name.Text.Trim());
    }

    void onDisconnect(Networking channel)
    {
        throw new NotImplementedException();
    }

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

        }
        catch (Exception)
        {
            addMessageAndScroll("Server Gone :(");
        }
    }

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

        clientList.Add("dd");
        ClientList.ScrollTo("dd", new ScrollToPosition(), true);
    }

    private void messageComplete(object sender, EventArgs e)
    {
        send.Text = string.Empty;

        try
        {
            channel.Send(send.Text);
        }
        catch (Exception)
        {
            addMessageAndScroll("Server Gone :(");
        }

    }

    private void addMessageAndScroll(string message)
    {
        messages.Add(message);
        MessageList.ScrollTo(message, new ScrollToPosition(), true);
    }
}

