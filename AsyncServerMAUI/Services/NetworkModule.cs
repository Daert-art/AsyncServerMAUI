using System.Net;
using System.Net.Sockets;
using System.Text;

public class ServerNetworkModule
{
    private Action<string> logAction;
    private Action<string> messageReceivedAction;
    private TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream stream;

    public ServerNetworkModule(Action<string> logAction, Action<string> messageReceivedAction)
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

                    stream = tcpClient.GetStream();
                    byte[] receiveBuffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length)) > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                        logAction($"Received from client: {receivedMessage}");
                        messageReceivedAction(receivedMessage);

                        if (receivedMessage.Trim().Equals("Bye", StringComparison.OrdinalIgnoreCase))
                        {
                            logAction("Connection closed by client.");
                            break;
                        }

                        byte[] responseBuffer = Encoding.UTF8.GetBytes("Hello, client!");
                        await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
                        logAction($"Sent response to client.");
                    }

                    tcpClient.Close();
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
            await stream.WriteAsync(buffer, 0, buffer.Length);
            logAction($"Sent {buffer.Length} bytes to client.");
        }
        catch (Exception ex)
        {
            logAction($"Error in SendMessage: {ex.Message}");
        }
    }
}
