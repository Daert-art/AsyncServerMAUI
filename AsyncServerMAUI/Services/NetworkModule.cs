using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkModule
{
    private Action<string> logAction;
    private Action<string> messageReceivedAction;
    private TcpListener tcpListener;
    private TcpClient tcpClient;

    public NetworkModule(Action<string> logAction, Action<string> messageReceivedAction)
    {
        this.logAction = logAction;
        this.messageReceivedAction = messageReceivedAction;
    }

    public void StartServer(string ipAddress, int port)
    {
        Task.Run(async () =>
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                tcpListener.Start();
                logAction("Server started...");

                while (true)
                {
                    tcpClient = await tcpListener.AcceptTcpClientAsync();
                    logAction($"Client connected: {tcpClient.Client.RemoteEndPoint}");

                    var stream = tcpClient.GetStream();
                    byte[] receiveBuffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                    if (bytesRead > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                        logAction($"Received from client: {receivedMessage}");
                        messageReceivedAction(receivedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                logAction($"Error in StartServer: {ex.Message}");
            }
        });
    }


    public async void SendMessage(string message)
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            logAction("No client connected to send the message.");
            return;
        }

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var stream = tcpClient.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length);
            logAction($"Sent {buffer.Length} bytes to client.");
        }
        catch (Exception ex)
        {
            logAction($"Error in SendMessage: {ex.Message}");
        }
    }
}
