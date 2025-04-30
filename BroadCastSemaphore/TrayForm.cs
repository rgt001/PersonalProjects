using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Media;

namespace BroadCastSemaphore
{
    public partial class TrayForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;

        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private Icon currentIcon;
        private TcpListener tcpListener;
        private Thread tcpListenerThread;
        private const int TcpPort = 5000;
        private const int StatusRequestTimeout = 100;

        private HashSet<string> remoteIPs = new HashSet<string>();
        private Color currentColor = Color.Green;

        public TrayForm()
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Load += TrayForm_Load;
            this.FormClosing += TrayForm_FormClosing;
        }

        private void TrayForm_Load(object sender, EventArgs e)
        {
            LoadConfig();

            // Menu: só Verde e Vermelho
            contextMenu = new ContextMenuStrip();
            var menuVerde = new ToolStripMenuItem("Verde", null, (s, ev) => ChangeColor(Color.Green));
            var menuVermelho = new ToolStripMenuItem("Vermelho", null, (s, ev) => ChangeColor(Color.Red));
            var sair = new ToolStripMenuItem("Sair", null, (s, ev) => Close());
            contextMenu.Items.AddRange(new ToolStripItem[] { menuVerde, menuVermelho, sair });

            notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = contextMenu,
                Visible = true
            };
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            tcpListener = new TcpListener(IPAddress.Any, TcpPort);
            tcpListener.Start();
            tcpListenerThread = new Thread(ListenForTcpMessages)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            tcpListenerThread.Start();

            UpdateNotifyIcon(Color.Green);
            RequestInitialStatus();
        }

        private void TrayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpListener?.Stop();
            if (tcpListenerThread != null && tcpListenerThread.IsAlive)
                tcpListenerThread.Abort();
            notifyIcon.Visible = false;
            currentIcon?.Dispose();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ChangeColor(currentColor == Color.Red ? Color.Green : Color.Red);
        }

        private void ChangeColor(Color newColor)
        {
            UpdateNotifyIcon(newColor);

            if (newColor == Color.Red)
                MinimizeApps();

            ThreadPool.QueueUserWorkItem(_ => SendColorChange(newColor));
        }

        private void UpdateNotifyIcon(Color color)
        {
            if (currentColor != color)
                SystemSounds.Asterisk.Play();

            Icon newIcon = CreateIcon(color);
            currentIcon?.Dispose();
            currentIcon = newIcon;
            currentColor = color;

            if (InvokeRequired)
                Invoke(new Action(() => notifyIcon.Icon = currentIcon));
            else
                notifyIcon.Icon = currentIcon;
        }

        private Icon CreateIcon(Color color)
        {
            int size = 32;
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(brush, 0, 0, size - 1, size - 1);
            }
            IntPtr hIcon = bmp.GetHicon();
            return Icon.FromHandle(hIcon);
        }

        private void SendColorChange(Color color)
        {
            string message = color == Color.Green ? "GREEN" : "RED";

            foreach (string ip in remoteIPs)
            {
                TcpClient client = null;
                try
                {
                    client = new TcpClient();
                    client.NoDelay = true;

                    IAsyncResult result = client.BeginConnect(ip, TcpPort, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(100))
                        continue;
                    client.EndConnect(result);

                    using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                    {
                        writer.WriteLine(message);
                    }
                }
                catch
                {
                    // ignora erro de envio
                }
                finally
                {
                    client?.Close();
                }
            }
        }

        private void RequestInitialStatus()
        {
            foreach (string ip in remoteIPs)
            {
                TcpClient client = null;
                try
                {
                    client = new TcpClient();
                    client.NoDelay = true;

                    IAsyncResult result = client.BeginConnect(ip, TcpPort, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(StatusRequestTimeout))
                        continue;

                    client.EndConnect(result);
                    using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                    using (var reader = new StreamReader(client.GetStream()))
                    {
                        writer.WriteLine("REQUEST_STATUS");
                        string response = reader.ReadLine();
                        if (response == "GREEN") UpdateNotifyIcon(Color.Green);
                        else if (response == "RED") UpdateNotifyIcon(Color.Red);
                        break;
                    }
                }
                catch
                {
                    // tenta próximo IP
                }
                finally
                {
                    client?.Close();
                }
            }
        }

        private void ListenForTcpMessages()
        {
            try
            {
                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleTcpClient, client);
                }
            }
            catch (SocketException)
            {
                // listener parado
            }
        }

        private void HandleTcpClient(object clientObj)
        {
            TcpClient client = clientObj as TcpClient;
            if (client == null) return;

            try
            {
                IPEndPoint remoteEP = client.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEP != null && !remoteIPs.Contains(remoteEP.Address.ToString()))
                {
                    client.Close();
                    return;
                }

                using (var reader = new StreamReader(client.GetStream()))
                using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                {
                    string msg = reader.ReadLine();
                    if (msg == "REQUEST_STATUS")
                    {
                        writer.WriteLine(currentColor == Color.Green ? "GREEN" : "RED");
                    }
                    else if (msg == "GREEN")
                    {
                        UpdateNotifyIcon(Color.Green);
                    }
                    else if (msg == "RED")
                    {
                        UpdateNotifyIcon(Color.Red);
                        MinimizeApps();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no cliente TCP: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private void LoadConfig()
        {
            try
            {
                string configFile = Path.Combine(Application.StartupPath, "config.txt");
                if (!File.Exists(configFile)) return;

                foreach (string line in File.ReadAllLines(configFile))
                {
                    string ip = line.Trim();
                    IPAddress parsed;
                    if (!string.IsNullOrEmpty(ip) &&
                        IPAddress.TryParse(ip, out parsed) &&
                        !IsLocalAddress(parsed))
                    {
                        remoteIPs.Add(ip);
                    }
                }
            }
            catch
            {
                // ignora erro de config
            }
        }

        private bool IsLocalAddress(IPAddress address)
        {
            foreach (IPAddress local in Dns.GetHostAddresses(Dns.GetHostName()))
                if (address.Equals(local))
                    return true;
            return false;
        }

        // dentro da sua classe TrayForm:

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        private const uint GW_OWNER = 4;

        private void MinimizeApps()
        {
            string path = Path.Combine(Application.StartupPath, "apps.txt");
            if (!File.Exists(path)) return;

            foreach (string line in File.ReadAllLines(path))
            {
                string procName = Path.GetFileNameWithoutExtension(line.Trim());
                if (string.IsNullOrEmpty(procName)) continue;

                Process[] procs = Process.GetProcessesByName(procName);
                foreach (Process proc in procs)
                {
                    EnumWindows((hWnd, lParam) =>
                    {
                        uint pid;
                        GetWindowThreadProcessId(hWnd, out pid);

                        // só janelas do processo, visíveis e sem owner
                        if (pid == proc.Id
                            && IsWindowVisible(hWnd)
                            && GetWindow(hWnd, GW_OWNER) == IntPtr.Zero)
                        {
                            ShowWindow(hWnd, SW_MINIMIZE);
                        }
                        return true; // continua enumerando
                    }, IntPtr.Zero);
                }
            }
        }
    }
}