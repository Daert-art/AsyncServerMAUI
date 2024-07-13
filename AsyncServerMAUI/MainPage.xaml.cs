using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AsyncServerMAUI
{
    public partial class MainPage : ContentPage
    {
        private NetworkModule networkModule;
        private List<string> phrases = new List<string> { "Hello!", "How are you?", "Goodbye!", "Nice to meet you!", "Have a great day!" };
        private Random random = new Random();

        public MainPage()
        {
            InitializeComponent();
            networkModule = new NetworkModule(AddLog, MessageReceived);
        }

        private void OnStartServerClicked(object sender, EventArgs e)
        {
            string ipAddress = IpEntry.Text;
            int port = int.Parse(PortEntry.Text);
            networkModule.StartServer(ipAddress, port);
        }

        private void OnSendClicked(object sender, EventArgs e)
        {
            string message = MessageEntry.Text;
            networkModule.SendMessage(message);
        }

        private void MessageReceived(string message)
        {
            AddLog($"Received from client: {message}");

            string response = phrases[random.Next(phrases.Count)];
            networkModule.SendMessage(response);
        }

        private void AddLog(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LogEditor.Text += $"{message}{Environment.NewLine}";
            });
        }
    }
}
