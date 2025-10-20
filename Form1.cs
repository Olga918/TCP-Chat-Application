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

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private Socket _socket;
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

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                string ip = txtIP.Text;
                int port = int.Parse(txtPort.Text);

                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.ConnectAsync(endPoint);

                AddLog($"Подключено к {ip}:{port}");
                btnConnect.Enabled = false;
                btnDisConnect.Enabled = true;
                btnSend.Enabled = true;

                // Запускаем прием сообщений
                _ = Task.Run(async () => await ReceiveMessages());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
            }

        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024];

            while (_socket != null && _socket.Connected)
            {
                try
                {
                    int bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    AddLog($"Сервер: {message}");

                    // Проверка на "Bye"
                    if (message.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                    {
                        AddLog("Сервер попрощался");
                        _socket.Close();
                        break;
                    }

                    // Автоответ в режиме компьютера
                    if (chkComputer.Checked)
                    {
                        string response = GetAutoResponse();
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await _socket.SendAsync(new ArraySegment<byte>(responseData), SocketFlags.None);
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

        

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _socket?.Close();
                AddLog("Отключено от сервера");
                btnConnect.Enabled = true;
                btnDisConnect.Enabled = false;
                btnSend.Enabled = false;
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка отключения: {ex.Message}");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(message) && _socket != null)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                    AddLog($"Я: {message}");
                    txtMessage.Clear();

                    if (message.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                    {
                        _socket.Close();
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
            if (listChat.InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), message);
                return;
            }
            listChat.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            listChat.TopIndex = listChat.Items.Count - 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
