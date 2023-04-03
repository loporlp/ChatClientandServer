using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Communications
{
    public class Networking
    {
        public delegate void ReportMessageArrived(Networking channel, string message);
        public delegate void ReportDisconnect(Networking channel);
        public delegate void ReportConnectionEstablished(Networking channel);

        private readonly ReportMessageArrived onMessage;
        private readonly ReportDisconnect onDisconnect;
        private readonly ReportConnectionEstablished onConnect;

        private readonly ILogger logger;
        private TcpClient tcpClient { get; set; }
        private readonly char terminationCharacter;

        private List<Networking> clients;
        private CancellationTokenSource _WatiForCancellation;


        public string ID { get; set; }

        public Networking(ILogger logger,
           ReportConnectionEstablished onConnect,
           ReportDisconnect onDisconnect,
           ReportMessageArrived onMessage,
           char terminationCharacter)
        {
            ID = "";
            this.onMessage = onMessage;
            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;

            tcpClient = new TcpClient();
            this.logger = logger;
            this.terminationCharacter = terminationCharacter;

            clients = new List<Networking>();
            _WatiForCancellation = new();
        }

        /// <summary>
        ///     Connects to the host ip and port when called
        /// </summary>
        /// <param name="host"> ip to connect to </param>
        /// <param name="port"> port to connect to </param>
        public void Connect(string host, int port)
        {
            try
            {
                if (tcpClient.Connected)
                {
                    return;
                }

                tcpClient = new TcpClient(host, port);
                onConnect(this);
            }
            catch (Exception e)
            {
                logger.LogInformation($"Unsucessful attempt to connect to {host} on port {port} Error {e}");
                throw;
            }

        }

        /// <summary>
        ///     Waits for clients to connect method used by servers
        /// </summary>
        /// <param name="port"> port to connect on </param>
        /// <param name="infinite"></param>
        public async void WaitForClients(int port, bool infinite)
        {
            TcpListener network_listener = new TcpListener(IPAddress.Any, port);
            network_listener.Start();
            try
            {
                while (true)
                {

                    TcpClient connection = await network_listener.AcceptTcpClientAsync(_WatiForCancellation.Token);
                    logger.LogInformation($"\n ** New Connection ** Accepted From {connection.Client.RemoteEndPoint} to {connection.Client.LocalEndPoint}");
                    lock (this.clients)
                    {
                        Networking client = new Networking(logger, onConnect, onDisconnect, onMessage, terminationCharacter);
                        client.tcpClient = connection;
                        client.ID = client.tcpClient.Client.RemoteEndPoint.ToString();
                        lock (clients)
                        {
                            clients.Add(client);
                        }
                        onConnect(client);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public void StopWaitingForClients()
        {
            _WatiForCancellation.Cancel();
        }

        /// <summary>
        ///     When called waits for 
        /// </summary>
        /// <param name="infinite"></param>
        public async void AwaitMessagesAsync(bool infinite = true)
        {
            while (true)
            {
                try
                {
                    StringBuilder dataBacklog = new StringBuilder();
                    byte[] buffer = new byte[4096];
                    NetworkStream stream = tcpClient.GetStream();

                    if (stream == null)
                    {
                        return;
                    }

                    while (true)
                    {
                        int total = await stream.ReadAsync(buffer);

                        if (total == 0)
                        {
                            // the connection quit unexpectedly
                            throw new Exception("End of Stream Reached. Connection must be closed.");
                        }

                        string current_data = Encoding.UTF8.GetString(buffer, 0, total);

                        dataBacklog.Append(current_data);

                        Console.WriteLine($"  Received {total} new bytes for a total of {dataBacklog.Length}.");

                        this.CheckForMessage(dataBacklog);
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                    return;
                }
            }
        }

        /// <summary>
        ///   Given a string (actually a string builder object)
        ///   check to see if it contains one or more messages as defined by
        ///   our protocol (the period '/n').
        /// </summary>
        /// <param name="data"> all characters encountered so far</param>
        private void CheckForMessage(StringBuilder data)
        {
            string allData = data.ToString();
            int terminator_position = allData.IndexOf(terminationCharacter);
            bool foundOneMessage = false;

            while (terminator_position >= 0)
            {
                foundOneMessage = true;

                string message = allData.Substring(0, terminator_position);
                data.Remove(0, terminator_position + 1);

                onMessage(this, message);



                allData = data.ToString();
                terminator_position = allData.IndexOf(".");
            }

            if (!foundOneMessage)
            {
                logger.LogInformation($"  Message NOT found in data");
            }
            else
            {
                logger.LogInformation(
                    $"  --------------------------------------------------------------------------------\n" +
                    $"  After Message: {data.Length} bytes unprocessed.\n" +
                    $"  --------------------------------------------------------------------------------\n");
            }
        }

        /// <summary>
        ///     Sends text given to the connected tcpClient if connected
        /// </summary>
        /// <param name="text"></param>
        public async void Send(string text)
        {
            if (tcpClient.Connected)
            {
                if (text.Contains($"\n"))
                {
                    text.Replace($"\n", $"\r");
                }

                text += $"\n";

                byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(text);

                try
                {
                    await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                    logger.LogInformation($"    Message Sent from:   {tcpClient.Client.LocalEndPoint} to {tcpClient.Client.RemoteEndPoint}");
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"    Client Disconnected: {tcpClient.Client.RemoteEndPoint} - {ex.Message}");
                }

            }
        }

        public void Disconnect()
        {
            onDisconnect(this);
            tcpClient.Close();
        }
    }
}