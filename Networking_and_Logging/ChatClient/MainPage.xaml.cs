using Communications;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatClient;

public partial class MainPage : ContentPage
{

   private ObservableCollection<string> clientList;

    

    public MainPage()
    {
        InitializeComponent();
        //	Networking channel = new Networking(NullLogger.Instance, onConnect(), onDisconnect(), onMessage(), '.');
        clientList = new ObservableCollection<string>{ "Maosn", "1 Hour", "1:30 Hour", "2 Hours" };
        ClientList.ItemsSource = clientList;
    }

    ReportMessageArrived onMessage()
    {
        throw new NotImplementedException();
    }

    ReportConnectionEstablished onConnect()
    {
        throw new NotImplementedException();
    }

    ReportDisconnect onDisconnect()
    {
        throw new NotImplementedException();
    }

    private void addressEntered(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void connectToServer(object sender, EventArgs e)
    {
        clientList.Add("SDFS");
       // ClientList.ItemsSource = clientList;
    }

    private void nameEntered(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}

