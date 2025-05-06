namespace UniFiProvisionTool
{
    partial class ConfigEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvConfigs;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private Button btnDelete;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvConfigs = new System.Windows.Forms.DataGridView();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDelete = new Button();
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.Size = new Size(100, 30);
            this.btnDelete.Location = new Point(110, 325);
            this.btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(this.btnDelete);

            // 
            // dgvConfigs
            // 
            this.dgvConfigs.AllowUserToAddRows = true;
            this.dgvConfigs.AllowUserToDeleteRows = true;
            this.dgvConfigs.Anchor = ((System.Windows.Forms.AnchorStyles)(
                (System.Windows.Forms.AnchorStyles.Top
               | System.Windows.Forms.AnchorStyles.Bottom)
               | System.Windows.Forms.AnchorStyles.Left)
               | System.Windows.Forms.AnchorStyles.Right);
            this.dgvConfigs.ColumnHeadersHeightSizeMode =
                System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConfigs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new System.Windows.Forms.DataGridViewTextBoxColumn {
                    Name       = "colName",
                    HeaderText = "Name",
                    Width      = 150
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn {
                    Name       = "colIp",
                    HeaderText = "IP",
                    Width      = 150
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn {
                    Name       = "colSite",
                    HeaderText = "Site",
                    Width      = 150
                }
            });
            this.dgvConfigs.Location = new System.Drawing.Point(12, 12);
            this.dgvConfigs.Size = new System.Drawing.Size(500, 300);
            this.dgvConfigs.TabIndex = 0;

            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Bottom
               | System.Windows.Forms.AnchorStyles.Left));
            this.btnSave.Location = new System.Drawing.Point(12, 325);
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Bottom
               | System.Windows.Forms.AnchorStyles.Right));
            this.btnCancel.Location = new System.Drawing.Point(432, 325);
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 
            // ConfigEditorForm
            // 
            this.ClientSize = new System.Drawing.Size(524, 370);
            this.Controls.Add(this.dgvConfigs);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigEditorForm";
            this.Text = "Edit Saved Consoles";
            this.Load += new System.EventHandler(this.ConfigEditorForm_Load);

            ((System.ComponentModel.ISupportInitialize)(
                (System.ComponentModel.ISupportInitialize)this.dgvConfigs)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
