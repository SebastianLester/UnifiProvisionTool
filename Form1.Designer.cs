using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace UniFiProvisionTool
{
    partial class Form1
    {
        private IContainer components = null;

        // GroupBoxes
        private GroupBox grpController;
        private GroupBox grpDiscovery;
        private GroupBox grpProvision;

        // Controller
        private ComboBox cmbConsole;
        private Button btnEditConfigs;
        private Button btnOpenUI;
        private Panel panelAdoption;
        private Button btnCloseUI;
        private Button btnRefreshUI;
        private WebView2 webView21;

        // Discovery
        private Button btnDiscover;
        private CheckBox chkSelectAll;
        private CheckedListBox chkDevices;

        // Provision
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnDefaultCreds;
        private Button btnRun;
        private Button btnCancel;
        private RichTextBox richTextBoxOutput;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();

            // --- Form setup ---
            this.ClientSize = new Size(500, 720);             // compact width
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "UniFi Provision Tool";
            this.Load += new EventHandler(this.Form1_Load);

            // --- Embedded UI panel ---
            this.panelAdoption = new Panel();
            this.panelAdoption.Dock = DockStyle.Right;
            this.panelAdoption.Width =  900;
            this.panelAdoption.Visible = false;

            this.btnCloseUI = new Button
            {
                Text = "Close UI",
                Dock = DockStyle.Top,
                Height = 30
            };
            this.btnCloseUI.Click += new EventHandler(this.btnCloseUI_Click);

            this.btnRefreshUI = new Button
            {
                Text = "Refresh UI",
                Dock = DockStyle.Top,
                Height = 30
            };
            this.btnRefreshUI.Click += new EventHandler(this.btnRefreshUI_Click);

            this.webView21 = new WebView2
            {
                Dock = DockStyle.Fill
            };

            this.panelAdoption.Controls.Add(this.webView21);
            this.panelAdoption.Controls.Add(this.btnRefreshUI);
            this.panelAdoption.Controls.Add(this.btnCloseUI);

            // --- Controller group ---
            this.grpController = new GroupBox
            {
                Text = "Controller",
                Location = new Point(12, 12),
                Size = new Size(480, 110)
            };

            this.cmbConsole = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(10, 25),
                Size = new Size(300, 24)
            };
            this.cmbConsole.SelectedIndexChanged += new EventHandler(this.cmbConsole_SelectedIndexChanged);

            this.btnEditConfigs = new Button
            {
                Text = "Edit Saved Consoles",
                Location = new Point(320, 18),
                Size = new Size(140, 28)
            };
            this.btnEditConfigs.Click += new EventHandler(this.btnEditConfigs_Click);

            this.btnOpenUI = new Button
            {
                Text = "Open Controller UI",
                Location = new Point(10, 58),
                Size = new Size(200, 30),
                Enabled = false
            };
            this.btnOpenUI.Click += new EventHandler(this.btnOpenUI_Click);

            this.grpController.Controls.Add(this.cmbConsole);
            this.grpController.Controls.Add(this.btnEditConfigs);
            this.grpController.Controls.Add(this.btnOpenUI);

            // --- Discovery group ---
            this.grpDiscovery = new GroupBox
            {
                Text = "Discovery",
                Location = new Point(12, 130),
                Size = new Size(480, 220)
            };

            this.btnDiscover = new Button
            {
                Text = "Discover Devices",
                Location = new Point(10, 25),
                Size = new Size(200, 30),
                Enabled = false
            };
            this.btnDiscover.Click += new EventHandler(this.btnDiscover_Click);

            this.chkSelectAll = new CheckBox
            {
                Text = "Select All",
                Location = new Point(220, 25),
                Size = new Size(80, 24)
            };
            this.chkSelectAll.CheckedChanged += new EventHandler(this.chkSelectAll_CheckedChanged);

            this.chkDevices = new CheckedListBox
            {
                CheckOnClick = true,
                Location = new Point(10, 60),
                Size = new Size(460, 150)
            };
            this.chkDevices.ItemCheck += new ItemCheckEventHandler(this.chkDevices_ItemCheck);

            this.grpDiscovery.Controls.Add(this.btnDiscover);
            this.grpDiscovery.Controls.Add(this.chkSelectAll);
            this.grpDiscovery.Controls.Add(this.chkDevices);

            // --- Provision group ---
            this.grpProvision = new GroupBox
            {
                Text = "Provision",
                Location = new Point(12, 360),
                Size = new Size(480, 340)
            };

            this.txtUsername = new TextBox
            {
                Location = new Point(10, 25),
                Size = new Size(200, 24)
            };

            this.txtPassword = new TextBox
            {
                Location = new Point(220, 25),
                Size = new Size(200, 24),
                UseSystemPasswordChar = true
            };

            this.btnDefaultCreds = new Button
            {
                Text = "Default Credentials",
                Location = new Point(10, 60),
                Size = new Size(150, 28)
            };
            this.btnDefaultCreds.Click += new EventHandler(this.btnDefaultCreds_Click);

            this.btnRun = new Button
            {
                Text = "Run Provisioning",
                Location = new Point(170, 60),
                Size = new Size(150, 30),
                Enabled = false
            };
            this.btnRun.Click += new EventHandler(this.btnRun_Click);

            this.btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(330, 60),
                Size = new Size(120, 30)
            };
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            this.richTextBoxOutput = new RichTextBox
            {
                Location = new Point(10, 100),
                Size = new Size(460, 220),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 9)
            };

            // credit label
            var lblCredit = new Label
            {
                Text = "Built by Sebastian Lester",
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(12, this.ClientSize.Height - 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(lblCredit);

            // GitHub link
            var lnkGitHub = new LinkLabel
            {
                Text = "GitHub: uniﬁ-provision-tool",
                AutoSize = true,
                Location = new Point(12 + lblCredit.Width + 20, this.ClientSize.Height - 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            lnkGitHub.Links.Add(8, lnkGitHub.Text.Length - 8, "https://github.com/SebastianLester/UnifiProvisionTool");
            lnkGitHub.LinkClicked += (s, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Link.LinkData.ToString(),
                    UseShellExecute = true
                });
            };
            this.Controls.Add(lnkGitHub);


            this.grpProvision.Controls.Add(this.txtUsername);
            this.grpProvision.Controls.Add(this.txtPassword);
            this.grpProvision.Controls.Add(this.btnDefaultCreds);
            this.grpProvision.Controls.Add(this.btnRun);
            this.grpProvision.Controls.Add(this.btnCancel);
            this.grpProvision.Controls.Add(this.richTextBoxOutput);

            // --- Add all to Form ---
            this.Controls.Add(this.panelAdoption);
            this.Controls.Add(this.grpController);
            this.Controls.Add(this.grpDiscovery);
            this.Controls.Add(this.grpProvision);
        }
    }
}
