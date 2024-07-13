using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class NetworkModule
{
    private Action<string> logAction;
    private Action<string> messageReceivedAction;
    private Socket clientSocket; 

    public NetworkModule(Action<string> logAction, Action<string> messageReceivedAction)
    {
        this.logAction = logAction;
        this.messageReceivedAction = messageReceivedAction;
    }

    public void StartServer(string ipAddress, int port)
    {
        Task.Run(() =>
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                serverSocket.Bind(endPoint);
                serverSocket.Listen(10);
                logAction("Server started...");
                serverSocket.BeginAccept(AcceptCallback, serverSocket);
            }
            catch (Exception ex)
            {
                logAction($"Error in StartServer: {ex.Message}");
            }
        });
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            Socket serverSocket = (Socket)ar.AsyncState;
            clientSocket = serverSocket.EndAccept(ar); 
            logAction($"Client connected: {clientSocket.RemoteEndPoint}");

            byte[] receiveBuffer = new byte[1024];
            clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveCallback, receiveBuffer);

            serverSocket.BeginAccept(AcceptCallback, serverSocket);
        }
        catch (Exception ex)
        {
            logAction($"Error in AcceptCallback: {ex.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] receiveBuffer = (byte[])ar.AsyncState;
            int bytesRead = clientSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                logAction($"Received from client: {receivedMessage}");
                messageReceivedAction(receivedMessage);

                // Продолжаем принимать данные от клиента
                clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveCallback, receiveBuffer);
            }
            else
            {
                logAction("Client disconnected.");
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        catch (Exception ex)
        {
            logAction($"Error in ReceiveCallback: {ex.Message}");
        }
    }

    public void SendMessage(string message)
    {
        try
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, clientSocket);
        }
        catch (Exception ex)
        {
            logAction($"Error in SendMessage: {ex.Message}");
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            int bytesSent = clientSocket.EndSend(ar);
            logAction($"Sent {bytesSent} bytes to client.");
        }
        catch (Exception ex)
        {
            logAction($"Error in SendCallback: {ex.Message}");
        }
    }
}
