namespace ProxyCollector
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoRefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewProxies = new System.Windows.Forms.DataGridView();
            this.panelLog = new System.Windows.Forms.Panel();
            this.txtActionLog = new System.Windows.Forms.TextBox();
            this.lblActionLog = new System.Windows.Forms.Label();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProxies)).BeginInit();
            this.panelLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1400, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.fileToolStripMenuItem.Text = "Файл";
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportToolStripMenuItem.Text = "Экспорт";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Выход";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoRefreshToolStripMenuItem,
            this.backgroundModeToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.settingsToolStripMenuItem.Text = "Настройки";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // autoRefreshToolStripMenuItem
            // 
            this.autoRefreshToolStripMenuItem.CheckOnClick = true;
            this.autoRefreshToolStripMenuItem.Name = "autoRefreshToolStripMenuItem";
            this.autoRefreshToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.autoRefreshToolStripMenuItem.Text = "Автообновление";
            this.autoRefreshToolStripMenuItem.CheckedChanged += new System.EventHandler(this.AutoRefreshToolStripMenuItem_CheckedChanged);
            // 
            // backgroundModeToolStripMenuItem
            // 
            this.backgroundModeToolStripMenuItem.CheckOnClick = true;
            this.backgroundModeToolStripMenuItem.Name = "backgroundModeToolStripMenuItem";
            this.backgroundModeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.backgroundModeToolStripMenuItem.Text = "Фоновый режим";
            this.backgroundModeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.BackgroundModeToolStripMenuItem_CheckedChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.helpToolStripMenuItem.Text = "Помощь";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "О программе";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridViewProxies);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelLog);
            this.splitContainer1.Size = new System.Drawing.Size(1400, 676);
            this.splitContainer1.SplitterDistance = 1000;
            this.splitContainer1.TabIndex = 1;
            // 
            // dataGridViewProxies
            // 
            this.dataGridViewProxies.AllowUserToAddRows = false;
            this.dataGridViewProxies.AllowUserToDeleteRows = false;
            this.dataGridViewProxies.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewProxies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewProxies.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewProxies.Name = "dataGridViewProxies";
            this.dataGridViewProxies.ReadOnly = true;
            this.dataGridViewProxies.RowHeadersVisible = false;
            this.dataGridViewProxies.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewProxies.Size = new System.Drawing.Size(1000, 676);
            this.dataGridViewProxies.TabIndex = 0;
            this.dataGridViewProxies.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridViewProxies_ColumnHeaderMouseClick);
            this.dataGridViewProxies.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridViewProxies_CellFormatting);
            // 
            // panelLog
            // 
            this.panelLog.Controls.Add(this.btnClearLog);
            this.panelLog.Controls.Add(this.txtActionLog);
            this.panelLog.Controls.Add(this.lblActionLog);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Location = new System.Drawing.Point(0, 0);
            this.panelLog.Name = "panelLog";
            this.panelLog.Size = new System.Drawing.Size(396, 676);
            this.panelLog.TabIndex = 0;
            // 
            // txtActionLog
            // 
            this.txtActionLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtActionLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtActionLog.Location = new System.Drawing.Point(0, 25);
            this.txtActionLog.Multiline = true;
            this.txtActionLog.Name = "txtActionLog";
            this.txtActionLog.ReadOnly = true;
            this.txtActionLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtActionLog.Size = new System.Drawing.Size(396, 621);
            this.txtActionLog.TabIndex = 1;
            // 
            // lblActionLog
            // 
            this.lblActionLog.BackColor = System.Drawing.Color.LightBlue;
            this.lblActionLog.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblActionLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblActionLog.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblActionLog.Location = new System.Drawing.Point(0, 0);
            this.lblActionLog.Name = "lblActionLog";
            this.lblActionLog.Padding = new System.Windows.Forms.Padding(5);
            this.lblActionLog.Size = new System.Drawing.Size(396, 25);
            this.lblActionLog.TabIndex = 0;
            this.lblActionLog.Text = "Журнал действий:";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnClearLog.Location = new System.Drawing.Point(0, 646);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(396, 30);
            this.btnClearLog.TabIndex = 2;
            this.btnClearLog.Text = "Очистить";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.BtnClearLog_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 700);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Proxy Collector";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProxies)).EndInit();
            this.panelLog.ResumeLayout(false);
            this.panelLog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoRefreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridViewProxies;
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.TextBox txtActionLog;
        private System.Windows.Forms.Label lblActionLog;
        private System.Windows.Forms.Button btnClearLog;
    }
}

