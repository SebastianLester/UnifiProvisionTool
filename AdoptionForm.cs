using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace UniFiProvisionTool
{
    public partial class AdoptionForm : Form
    {
        private readonly string controllerUrl;

        public AdoptionForm(string controllerIp)
        {
            InitializeComponent();
            controllerUrl = controllerIp;
        }

        private async void AdoptionForm_Load(object sender, EventArgs e)
        {
            // Initialize WebView2 environment
            var env = await CoreWebView2Environment.CreateAsync(null, null, null);
            await webView21.EnsureCoreWebView2Async(env);

            // Navigate to the UniFi controller login page
            webView21.CoreWebView2.Navigate($"https://{controllerUrl}:8443/manage/account/login");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Reload();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}