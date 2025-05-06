using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UniFiProvisionTool
{
    public partial class ConfigEditorForm : Form
    {
        // Same consoles.txt path as Form1
        private readonly string consolesFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "consoles.txt");

        public ConfigEditorForm()
        {
            InitializeComponent();
        }

        private void ConfigEditorForm_Load(object sender, EventArgs e)
        {
            dgvConfigs.Rows.Clear();
            if (!File.Exists(consolesFile))
                return;

            foreach (var line in File.ReadAllLines(consolesFile))
            {
                var parts = line.Split('|');
                var name = parts.Length > 0 ? parts[0] : "";
                var ip = parts.Length > 1 ? parts[1] : "";
                var site = parts.Length > 2 ? parts[2] : "default";

                dgvConfigs.Rows.Add(name, ip, site);
            }
        }
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvConfigs.SelectedRows)
                if (!row.IsNewRow)
                    dgvConfigs.Rows.Remove(row);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var lines = dgvConfigs.Rows
                .OfType<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r =>
                {
                    var name = (r.Cells[0].Value ?? "").ToString().Trim();
                    var ip = (r.Cells[1].Value ?? "").ToString().Trim();
                    var site = (r.Cells[2].Value ?? "default").ToString().Trim();
                    return $"{name}|{ip}|{(string.IsNullOrEmpty(site) ? "default" : site)}";
                })
                // Only keep rows with both name and ip
                .Where(s =>
                {
                    var p = s.Split('|');
                    return p.Length >= 2
                        && !string.IsNullOrEmpty(p[0])
                        && !string.IsNullOrEmpty(p[1]);
                })
                .ToArray();

            File.WriteAllLines(consolesFile, lines);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
