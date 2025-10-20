using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private TcpListener _server;
        private Socket _clientSocket;
        private Random _random = new Random();
        private string[] _autoResponses = {
            "Привет! Как дела?",
            "Интересно... Расскажи больше!",
            "Я программа, но стараюсь помочь!",
            "Что думаешь об этом?",
            "Сегодня хороший день!",
            "Продолжай, я слушаю...",
            "Это интересная тема!",
            "Хочешь поговорить о чем-то?",
            "Как прошел твой день?",
            "До свидания! Было приятно пообщаться!"
        };
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                _server = new TcpListener(IPAddress.Any, port);
                _server.Start();

                AddLog($"Сервер запущен на порту {port}");
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                // Ждем подключения клиента
                _ = Task.Run(async () => await AcceptClient());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private async Task AcceptClient()
        {
            try
            {
                _clientSocket = await _server.AcceptSocketAsync();

                AddLog("Клиент подключен!");
                btnSend.Enabled = true;

                // Запускаем прием сообщений
                _ = Task.Run(async () => await ReceiveMessages());
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка: {ex.Message}");
            }
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024];

            while (_clientSocket != null && _clientSocket.Connected)
            {
                try
                {
                    int bytesRead = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    AddLog($"Клиент: {message}");

                    // Проверка на "Bye"
                    if (message.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                    {
                        AddLog("Клиент попрощался");
                        _clientSocket.Close();
                        break;
                    }

                    // Автоответ в режиме компьютера
                    if (chkComputer.Checked)
                    {
                        string response = GetAutoResponse();
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await _clientSocket.SendAsync(new ArraySegment<byte>(responseData), SocketFlags.None);
                        AddLog($"Я (авто): {response}");
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"Ошибка: {ex.Message}");
                    break;
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                _server?.Stop();
                _clientSocket?.Close();

                AddLog("Сервер остановлен");
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                btnSend.Enabled = false;
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка: {ex.Message}");
            }

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(message) && _clientSocket != null)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _clientSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                    AddLog($"Я: {message}");
                    txtMessage.Clear();

                    if (message.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                    {
                        _clientSocket.Close();
                        btnSend.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"Ошибка отправки: {ex.Message}");
                }
            }
        }

        private string GetAutoResponse()
        {
            return _autoResponses[_random.Next(_autoResponses.Length)];
        }

        private void AddLog(string message)
        {
            if (listLog.InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), message);
                return;
            }
            listLog.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            listLog.TopIndex = listLog.Items.Count - 1;
        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
