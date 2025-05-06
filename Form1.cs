using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using Microsoft.Web.WebView2.Core;

namespace UniFiProvisionTool
{
    public partial class Form1 : Form
    {
        private readonly string baseDir;
        private readonly string consolesFile;
        private readonly string logFilePath;
        private List<ConsoleEntry> consoles = new();
        private List<bool> _isAdoptedFlags = new();
        private int _baseFormWidth;
        private const string updateJsonUrl =
            "https://raw.githubusercontent.com/YourUser/YourRepo/main/latest_version.json";
        private const string currentVersion = "1.0.0";

        public Form1()
        {
            InitializeComponent();
            baseDir = AppDomain.CurrentDomain.BaseDirectory;
            consolesFile = Path.Combine(baseDir, "consoles.txt");
            logFilePath = Path.Combine(baseDir, $"ProvisionLog_{DateTime.Now:yyyyMMdd}.txt");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Remember compact width
            _baseFormWidth = this.Width;

            // Initialize WebView2 with persistent storage
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var userDataFolder = Path.Combine(localAppData, "UniFiProvisionTool", "WebView2");
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView21.EnsureCoreWebView2Async(env);

            LoadConsoles();
            await CheckForUpdateAsync();
        }

        private void LoadConsoles()
        {
            consoles.Clear();
            cmbConsole.DataSource = null;
            if (File.Exists(consolesFile))
            {
                foreach (var line in File.ReadAllLines(consolesFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                        consoles.Add(new ConsoleEntry
                        {
                            Name = parts[0],
                            Ip = parts[1],
                            Site = parts.Length > 2 ? parts[2] : "default"
                        });
                }
            }
            cmbConsole.DisplayMember = "Name";
            cmbConsole.ValueMember = "Ip";
            cmbConsole.DataSource = consoles;
        }

        private void cmbConsole_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOpenUI.Enabled = cmbConsole.SelectedIndex >= 0;
        }

        private void btnEditConfigs_Click(object sender, EventArgs e)
        {
            using var dlg = new ConfigEditorForm();
            dlg.ShowDialog();
            LoadConsoles();
        }

        private void btnOpenUI_Click(object sender, EventArgs e)
        {
            if (!(cmbConsole.SelectedItem is ConsoleEntry ce)) return;

            // Expand to show panel
            this.Width = _baseFormWidth + panelAdoption.Width;
            panelAdoption.Visible = true;

            webView21.CoreWebView2.Navigate(
                $"https://{ce.Ip}:8443/manage/account/login?redirect=%2Fmanage"
            );

            btnDiscover.Enabled = true;
            btnOpenUI.Enabled = false;
        }

        private void btnRefreshUI_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Reload();
        }

        private void btnCloseUI_Click(object sender, EventArgs e)
        {
            // Shrink back to compact
            panelAdoption.Visible = false;
            this.Width = _baseFormWidth;
        }

        private async void btnDiscover_Click(object sender, EventArgs e)
        {
            // 1) Clear previous discovery results
            chkDevices.Items.Clear();
            _isAdoptedFlags.Clear();
            chkSelectAll.Checked = false;
            richTextBoxOutput.Clear();

            // 2) Disable the button to prevent re-entrancy
            btnDiscover.Enabled = false;
            Log("Starting discovery...");

            // 3) Un-adopted devices via UDP probe
            var unadopted = new List<UnifiDiscoveryResponse>();
            using (var client = new UdpClient(AddressFamily.InterNetwork) { EnableBroadcast = true })
            {
                try
                {
                    client.Client.Bind(new IPEndPoint(IPAddress.Any, 10001));
                    Log("Bound UDP port 10001");
                }
                catch (Exception ex)
                {
                    Log("Failed to bind UDP port 10001: " + ex.Message);
                    btnDiscover.Enabled = true;
                    return;
                }

                // send the probe packet
                await client.SendAsync(new byte[] { 1, 0, 0, 0 }, 4,
                                       new IPEndPoint(IPAddress.Broadcast, 10001));

                var deadline = DateTime.Now.AddMilliseconds(3000);
                client.Client.ReceiveTimeout = 500;

                while (DateTime.Now < deadline)
                {
                    try
                    {
                        var result = await client.ReceiveAsync();
                        var buf = result.Buffer;
                        if (buf.Length > 4)
                        {
                            var json = Encoding.UTF8.GetString(buf, 4, buf.Length - 4);
                            var d = JsonSerializer.Deserialize<UnifiDiscoveryResponse>(json);
                            if (d != null && !unadopted.Any(x => x.mac == d.mac))
                                unadopted.Add(d);
                        }
                    }
                    catch (SocketException) { /* timeout—keep looping */ }
                }
            }

            // 4) Adopted devices via Controller API session
            var adopted = new List<UnifiDiscoveryResponse>();
            if (panelAdoption.Visible && cmbConsole.SelectedItem is ConsoleEntry ce)
            {
                adopted = await GetAdoptedDevicesViaSession(ce);
                Log($"Fetched {adopted.Count} adopted device(s).");
            }

            // 5) Populate list with adopted (disabled) then un-adopted (checkable)
            foreach (var d in adopted)
            {
                chkDevices.Items.Add($"{d.name} ({d.ip}) — {d.model} [ADOPTED]");
                _isAdoptedFlags.Add(true);
            }
            foreach (var d in unadopted)
            {
                if (!adopted.Any(a => a.mac == d.mac))
                {
                    chkDevices.Items.Add($"{d.name} ({d.ip}) — {d.model}");
                    _isAdoptedFlags.Add(false);
                }
            }

            Log($"Discovery complete: {adopted.Count} adopted + {unadopted.Count} unadopted found.");

            // 6) Re-enable the Run button if there’s anything to do
            btnRun.Enabled = chkDevices.Items.Count > 0;
            btnDiscover.Enabled = true;
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < chkDevices.Items.Count; i++)
                chkDevices.SetItemChecked(i, chkSelectAll.Checked);
        }

        private void chkDevices_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < _isAdoptedFlags.Count && _isAdoptedFlags[e.Index])
                e.NewValue = e.CurrentValue;
        }

        private void btnDefaultCreds_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "ubnt";
            txtPassword.Text = "ubnt";
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            btnRun.Enabled = false;
            var user = txtUsername.Text.Trim();
            var pass = txtPassword.Text.Trim();
            var ctrl = (cmbConsole.SelectedItem as ConsoleEntry)?.Ip;

            await Task.Run(() => {
                foreach (var item in chkDevices.CheckedItems)
                {
                    var text = item.ToString();
                    var ip = text.Split('(')[1].Split(')')[0];
                    ProvisionDevice(ip, user, pass, ctrl);
                }
            });

            btnRun.Enabled = true;
        }
        private async void btnCancel_Click(object sender, EventArgs e)
        {
            await ResetToFreshAsync();
        }

        private void ProvisionDevice(string deviceIP, string user, string pass, string controllerIP)
        {
            var informCmd = $"set-inform https://{controllerIP}:8080/inform";
            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    Log($"[{deviceIP}] Attempt {i}: Connecting...");
                    using var ssh = new SshClient(deviceIP, user, pass);
                    ssh.Connect();
                    var cmd = ssh.CreateCommand(informCmd);
                    var res = cmd.Execute();
                    ssh.Disconnect();
                    Log($"[{deviceIP}] Success: {res.Trim()}");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"[{deviceIP}] Error: {ex.Message}");
                    if (i == 3) Log($"[{deviceIP}] Max retries reached.");
                }
            }
        }

        private async Task<List<UnifiDiscoveryResponse>> GetAdoptedDevicesViaSession(ConsoleEntry ce)
        {
            // (JsonDocument-based extraction as before)
            var domain = new Uri($"https://{ce.Ip}:8443").Host;
            var winCookies = await webView21.CoreWebView2.CookieManager
                                        .GetCookiesAsync($"https://{domain}");
            var cookieContainer = new CookieContainer();
            foreach (var wc in winCookies)
            {
                var c = new Cookie(
                    wc.Name, wc.Value, wc.Path,
                    wc.Domain.StartsWith(".") ? wc.Domain.Substring(1) : wc.Domain
                );
                cookieContainer.Add(c);
            }

            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri($"https://{ce.Ip}:8443")
            };

            var site = string.IsNullOrEmpty(ce.Site) ? "default" : ce.Site;
            var resp = await http.GetAsync($"/api/s/{site}/stat/device");
            if (!resp.IsSuccessStatusCode) return new();

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("data", out var dataEl))
                return new();

            var list = JsonSerializer.Deserialize<List<UnifiDiscoveryResponse>>(
                dataEl.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return list ?? new();
        }

        private void Log(string msg)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {msg}{Environment.NewLine}";
            File.AppendAllText(logFilePath, line);
            richTextBoxOutput.AppendText(line);
        }

        private async Task CheckForUpdateAsync() { /* unchanged */ }

        private async Task ResetToFreshAsync()
        {
            // 1) Collapse the UI panel
            panelAdoption.Visible = false;
            this.Width = _baseFormWidth;

            // 2) Clear all lists and flags
            chkDevices.Items.Clear();
            _isAdoptedFlags.Clear();
            chkSelectAll.Checked = false;
            richTextBoxOutput.Clear();

            // 3) Disable downstream buttons
            btnDiscover.Enabled = false;
            btnRun.Enabled = false;
            // Open UI only if a console is still selected
            btnOpenUI.Enabled = cmbConsole.SelectedIndex >= 0;

            // 4) Wipe out any WebView2 session (cookies, localStorage, etc.)
            //if (webView21.CoreWebView2 != null)
            //{
            //    await webView21.CoreWebView2.CookieManager.DeleteAllCookiesAsync();
            //    webView21.CoreWebView2.Navigate("about:blank");
            //}
        }

        private class ConsoleEntry
        {
            public string Name { get; set; }
            public string Ip { get; set; }
            public string Site { get; set; }
        }
    }
}
