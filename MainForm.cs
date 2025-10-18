using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProxyCollector.Models;
using ProxyCollector.Services;

namespace ProxyCollector
{
    public partial class MainForm : Form
    {
        private readonly ProxyParser _proxyParser;
        private readonly ProxyChecker _proxyChecker;
        private readonly ProxyStorage _proxyStorage;
        private readonly System.Windows.Forms.Timer _refreshTimer;
        private readonly System.Windows.Forms.Timer _backgroundTimer;
        
        private List<ProxyServer> _allProxies;
        private bool _isMinimizedToTray;
        private NotifyIcon _trayIcon;

        public MainForm()
        {
            InitializeComponent();
            
            _proxyParser = new ProxyParser();
            _proxyChecker = new ProxyChecker();
            _proxyStorage = new ProxyStorage();
            _allProxies = new List<ProxyServer>();

            // Настройка таймеров
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 30000; // 30 секунд
            _refreshTimer.Tick += RefreshTimer_Tick;

            _backgroundTimer = new System.Windows.Forms.Timer();
            _backgroundTimer.Interval = 300000; // 5 минут
            _backgroundTimer.Tick += BackgroundTimer_Tick;

            SetupTrayIcon();
            InitializeDataGridView();
            LoadProxies();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridViewProxies = new DataGridView();
            this.btnRefresh = new Button();
            this.btnParseNew = new Button();
            this.btnCheckAll = new Button();
            this.btnExport = new Button();
            this.progressBar = new ProgressBar();
            this.lblStatus = new Label();
            this.lblTotalProxies = new Label();
            this.lblAvailableProxies = new Label();
            this.cmbFilterType = new ComboBox();
            this.cmbFilterCountry = new ComboBox();
            this.txtFilterIP = new TextBox();
            this.chkOnlyAvailable = new CheckBox();
            this.btnClearFilters = new Button();
            this.menuStrip = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.exportToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.settingsToolStripMenuItem = new ToolStripMenuItem();
            this.autoRefreshToolStripMenuItem = new ToolStripMenuItem();
            this.backgroundModeToolStripMenuItem = new ToolStripMenuItem();
            this.helpToolStripMenuItem = new ToolStripMenuItem();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();

            this.SuspendLayout();

            // MainForm
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 600);
            this.Controls.Add(this.dataGridViewProxies);
            this.Controls.Add(this.panelControls);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Proxy Collector";
            this.WindowState = FormWindowState.Normal;
            this.Resize += MainForm_Resize;

            // DataGridView
            this.dataGridViewProxies.AllowUserToAddRows = false;
            this.dataGridViewProxies.AllowUserToDeleteRows = false;
            this.dataGridViewProxies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewProxies.Dock = DockStyle.Fill;
            this.dataGridViewProxies.Location = new Point(0, 200);
            this.dataGridViewProxies.Name = "dataGridViewProxies";
            this.dataGridViewProxies.ReadOnly = true;
            this.dataGridViewProxies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewProxies.Size = new Size(1200, 400);
            this.dataGridViewProxies.TabIndex = 0;

            // Panel Controls
            this.panelControls = new Panel();
            this.panelControls.Dock = DockStyle.Top;
            this.panelControls.Height = 200;
            this.panelControls.Controls.AddRange(new Control[] {
                this.btnRefresh, this.btnParseNew, this.btnCheckAll, this.btnExport,
                this.progressBar, this.lblStatus, this.lblTotalProxies, this.lblAvailableProxies,
                this.cmbFilterType, this.cmbFilterCountry, this.txtFilterIP, this.chkOnlyAvailable,
                this.btnClearFilters
            });

            // Buttons
            this.btnRefresh.Location = new Point(10, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(100, 30);
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.Click += BtnRefresh_Click;

            this.btnParseNew.Location = new Point(120, 10);
            this.btnParseNew.Name = "btnParseNew";
            this.btnParseNew.Size = new Size(120, 30);
            this.btnParseNew.Text = "Парсить новые";
            this.btnParseNew.Click += BtnParseNew_Click;

            this.btnCheckAll.Location = new Point(250, 10);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new Size(100, 30);
            this.btnCheckAll.Text = "Проверить все";
            this.btnCheckAll.Click += BtnCheckAll_Click;

            this.btnExport.Location = new Point(360, 10);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new Size(100, 30);
            this.btnExport.Text = "Экспорт";
            this.btnExport.Click += BtnExport_Click;

            // Progress Bar
            this.progressBar.Location = new Point(10, 50);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(500, 20);
            this.progressBar.Visible = false;

            // Labels
            this.lblStatus.Location = new Point(10, 80);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(500, 20);
            this.lblStatus.Text = "Готов к работе";

            this.lblTotalProxies.Location = new Point(10, 110);
            this.lblTotalProxies.Name = "lblTotalProxies";
            this.lblTotalProxies.Size = new Size(200, 20);
            this.lblTotalProxies.Text = "Всего прокси: 0";

            this.lblAvailableProxies.Location = new Point(220, 110);
            this.lblAvailableProxies.Name = "lblAvailableProxies";
            this.lblAvailableProxies.Size = new Size(200, 20);
            this.lblAvailableProxies.Text = "Доступных: 0";

            // Filters
            this.cmbFilterType.Location = new Point(10, 140);
            this.cmbFilterType.Name = "cmbFilterType";
            this.cmbFilterType.Size = new Size(100, 25);
            this.cmbFilterType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFilterType.Items.AddRange(new object[] { "Все типы", "HTTP", "HTTPS", "SOCKS4", "SOCKS5" });
            this.cmbFilterType.SelectedIndex = 0;
            this.cmbFilterType.SelectedIndexChanged += FilterChanged;

            this.cmbFilterCountry.Location = new Point(120, 140);
            this.cmbFilterCountry.Name = "cmbFilterCountry";
            this.cmbFilterCountry.Size = new Size(150, 25);
            this.cmbFilterCountry.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFilterCountry.SelectedIndexChanged += FilterChanged;

            this.txtFilterIP.Location = new Point(280, 140);
            this.txtFilterIP.Name = "txtFilterIP";
            this.txtFilterIP.Size = new Size(150, 25);
            this.txtFilterIP.PlaceholderText = "Фильтр по IP";
            this.txtFilterIP.TextChanged += FilterChanged;

            this.chkOnlyAvailable.Location = new Point(440, 140);
            this.chkOnlyAvailable.Name = "chkOnlyAvailable";
            this.chkOnlyAvailable.Size = new Size(150, 25);
            this.chkOnlyAvailable.Text = "Только доступные";
            this.chkOnlyAvailable.CheckedChanged += FilterChanged;

            this.btnClearFilters.Location = new Point(600, 140);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new Size(100, 25);
            this.btnClearFilters.Text = "Очистить";
            this.btnClearFilters.Click += BtnClearFilters_Click;

            // Menu Strip
            this.menuStrip.Items.AddRange(new ToolStripItem[] {
                this.fileToolStripMenuItem, this.settingsToolStripMenuItem, this.helpToolStripMenuItem
            });

            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.exportToolStripMenuItem, this.exitToolStripMenuItem
            });
            this.fileToolStripMenuItem.Text = "Файл";

            this.exportToolStripMenuItem.Text = "Экспорт";
            this.exportToolStripMenuItem.Click += BtnExport_Click;

            this.exitToolStripMenuItem.Text = "Выход";
            this.exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;

            this.settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.autoRefreshToolStripMenuItem, this.backgroundModeToolStripMenuItem
            });
            this.settingsToolStripMenuItem.Text = "Настройки";

            this.autoRefreshToolStripMenuItem.Text = "Автообновление";
            this.autoRefreshToolStripMenuItem.CheckOnClick = true;
            this.autoRefreshToolStripMenuItem.CheckedChanged += AutoRefreshToolStripMenuItem_CheckedChanged;

            this.backgroundModeToolStripMenuItem.Text = "Фоновый режим";
            this.backgroundModeToolStripMenuItem.CheckOnClick = true;
            this.backgroundModeToolStripMenuItem.CheckedChanged += BackgroundModeToolStripMenuItem_CheckedChanged;

            this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.aboutToolStripMenuItem
            });
            this.helpToolStripMenuItem.Text = "Помощь";

            this.aboutToolStripMenuItem.Text = "О программе";
            this.aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeDataGridView()
        {
            dataGridViewProxies.Columns.Clear();
            
            dataGridViewProxies.Columns.Add("IP", "IP адрес");
            dataGridViewProxies.Columns.Add("Port", "Порт");
            dataGridViewProxies.Columns.Add("Type", "Тип");
            dataGridViewProxies.Columns.Add("Country", "Страна");
            dataGridViewProxies.Columns.Add("Speed", "Скорость (мс)");
            dataGridViewProxies.Columns.Add("Available", "Доступен");
            dataGridViewProxies.Columns.Add("ResponseTime", "Время отклика (мс)");
            dataGridViewProxies.Columns.Add("LastChecked", "Последняя проверка");
            dataGridViewProxies.Columns.Add("Source", "Источник");

            // Настройка колонок
            dataGridViewProxies.Columns["IP"].Width = 120;
            dataGridViewProxies.Columns["Port"].Width = 60;
            dataGridViewProxies.Columns["Type"].Width = 80;
            dataGridViewProxies.Columns["Country"].Width = 100;
            dataGridViewProxies.Columns["Speed"].Width = 100;
            dataGridViewProxies.Columns["Available"].Width = 80;
            dataGridViewProxies.Columns["ResponseTime"].Width = 120;
            dataGridViewProxies.Columns["LastChecked"].Width = 150;
            dataGridViewProxies.Columns["Source"].Width = 200;
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon(components)
            {
                Icon = SystemIcons.Application,
                Text = "Proxy Collector",
                Visible = false
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Показать", null, (s, e) => ShowFromTray());
            contextMenu.Items.Add("Выход", null, (s, e) => Application.Exit());
            _trayIcon.ContextMenuStrip = contextMenu;
            _trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private async void LoadProxies()
        {
            try
            {
                lblStatus.Text = "Загрузка прокси...";
                _allProxies = await _proxyStorage.LoadProxiesAsync();
                UpdateDisplay();
                UpdateStatistics();
                lblStatus.Text = $"Загружено {_allProxies.Count} прокси";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке прокси: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка загрузки";
            }
        }

        private void UpdateDisplay()
        {
            var filteredProxies = GetFilteredProxies();
            
            dataGridViewProxies.Rows.Clear();
            
            foreach (var proxy in filteredProxies.OrderByDescending(p => p.IsAvailable).ThenBy(p => p.ResponseTime))
            {
                var row = new object[]
                {
                    proxy.IpAddress,
                    proxy.Port,
                    proxy.Type.ToString(),
                    proxy.Country,
                    proxy.Speed,
                    proxy.IsAvailable ? "Да" : "Нет",
                    proxy.ResponseTime > 0 ? proxy.ResponseTime.ToString() : "N/A",
                    proxy.LastChecked.ToString("dd.MM.yyyy HH:mm"),
                    proxy.Source
                };
                dataGridViewProxies.Rows.Add(row);
            }
        }

        private List<ProxyServer> GetFilteredProxies()
        {
            var filtered = _allProxies.AsEnumerable();

            if (cmbFilterType.SelectedIndex > 0)
            {
                var selectedType = (ProxyType)(cmbFilterType.SelectedIndex - 1);
                filtered = filtered.Where(p => p.Type == selectedType);
            }

            if (cmbFilterCountry.SelectedIndex > 0)
            {
                var selectedCountry = cmbFilterCountry.SelectedItem.ToString();
                filtered = filtered.Where(p => p.Country.Contains(selectedCountry));
            }

            if (!string.IsNullOrWhiteSpace(txtFilterIP.Text))
            {
                filtered = filtered.Where(p => p.IpAddress.Contains(txtFilterIP.Text));
            }

            if (chkOnlyAvailable.Checked)
            {
                filtered = filtered.Where(p => p.IsAvailable);
            }

            return filtered.ToList();
        }

        private void UpdateStatistics()
        {
            lblTotalProxies.Text = $"Всего прокси: {_allProxies.Count}";
            lblAvailableProxies.Text = $"Доступных: {_allProxies.Count(p => p.IsAvailable)}";
            
            // Обновляем список стран для фильтра
            var countries = _allProxies.Select(p => p.Country).Distinct().OrderBy(c => c).ToList();
            countries.Insert(0, "Все страны");
            
            cmbFilterCountry.Items.Clear();
            cmbFilterCountry.Items.AddRange(countries.ToArray());
            if (cmbFilterCountry.Items.Count > 0)
                cmbFilterCountry.SelectedIndex = 0;
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshProxies();
        }

        private async void BtnParseNew_Click(object sender, EventArgs e)
        {
            await ParseNewProxies();
        }

        private async void BtnCheckAll_Click(object sender, EventArgs e)
        {
            await CheckAllProxies();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ExportProxies();
        }

        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            cmbFilterType.SelectedIndex = 0;
            cmbFilterCountry.SelectedIndex = 0;
            txtFilterIP.Clear();
            chkOnlyAvailable.Checked = false;
            UpdateDisplay();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private async Task RefreshProxies()
        {
            try
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Обновление прокси...";
                btnRefresh.Enabled = false;

                await Task.Run(async () =>
                {
                    var newProxies = await _proxyParser.ParseAllSourcesAsync();
                    var checkedProxies = await _proxyChecker.CheckProxiesAsync(newProxies);
                    
                    // Объединяем с существующими прокси
                    var existingProxies = _allProxies.ToDictionary(p => $"{p.IpAddress}:{p.Port}", p => p);
                    
                    foreach (var proxy in checkedProxies)
                    {
                        var key = $"{proxy.IpAddress}:{proxy.Port}";
                        if (existingProxies.ContainsKey(key))
                        {
                            // Обновляем существующий
                            var existing = existingProxies[key];
                            existing.IsAvailable = proxy.IsAvailable;
                            existing.ResponseTime = proxy.ResponseTime;
                            existing.LastChecked = proxy.LastChecked;
                        }
                        else
                        {
                            // Добавляем новый
                            _allProxies.Add(proxy);
                        }
                    }
                });

                await _proxyStorage.SaveProxiesAsync(_allProxies);
                UpdateDisplay();
                UpdateStatistics();
                lblStatus.Text = "Прокси обновлены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении прокси: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка обновления";
            }
            finally
            {
                progressBar.Visible = false;
                btnRefresh.Enabled = true;
            }
        }

        private async Task ParseNewProxies()
        {
            try
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Парсинг новых прокси...";
                btnParseNew.Enabled = false;

                var newProxies = await _proxyParser.ParseAllSourcesAsync();
                var checkedProxies = await _proxyChecker.CheckProxiesAsync(newProxies);
                
                // Добавляем только новые прокси
                var existingKeys = _allProxies.Select(p => $"{p.IpAddress}:{p.Port}").ToHashSet();
                var uniqueNewProxies = checkedProxies.Where(p => !existingKeys.Contains($"{p.IpAddress}:{p.Port}")).ToList();
                
                _allProxies.AddRange(uniqueNewProxies);
                await _proxyStorage.SaveProxiesAsync(_allProxies);
                
                UpdateDisplay();
                UpdateStatistics();
                lblStatus.Text = $"Добавлено {uniqueNewProxies.Count} новых прокси";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при парсинге прокси: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка парсинга";
            }
            finally
            {
                progressBar.Visible = false;
                btnParseNew.Enabled = true;
            }
        }

        private async Task CheckAllProxies()
        {
            try
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = _allProxies.Count;
                progressBar.Value = 0;
                lblStatus.Text = "Проверка прокси...";
                btnCheckAll.Enabled = false;

                var progress = new Progress<ProxyCheckProgress>(p =>
                {
                    progressBar.Value = p.Completed;
                    lblStatus.Text = $"Проверка: {p.CurrentProxy} ({p.Completed}/{p.Total})";
                });

                _allProxies = await _proxyChecker.CheckProxiesAsync(_allProxies, progress);
                await _proxyStorage.SaveProxiesAsync(_allProxies);
                
                UpdateDisplay();
                UpdateStatistics();
                lblStatus.Text = "Проверка завершена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке прокси: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка проверки";
            }
            finally
            {
                progressBar.Visible = false;
                btnCheckAll.Enabled = true;
            }
        }

        private void ExportProxies()
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv|JSON файлы (*.json)|*.json",
                DefaultExt = "txt"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var format = Path.GetExtension(saveDialog.FileName).ToLower() switch
                    {
                        ".csv" => ExportFormat.CSV,
                        ".json" => ExportFormat.JSON,
                        _ => ExportFormat.Text
                    };

                    var filteredProxies = GetFilteredProxies();
                    _proxyStorage.ExportProxiesAsync(filteredProxies, saveDialog.FileName, format).Wait();
                    
                    MessageBox.Show($"Прокси экспортированы в файл: {saveDialog.FileName}", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (!_isMinimizedToTray)
            {
                _ = Task.Run(async () => await RefreshProxies());
            }
        }

        private void BackgroundTimer_Tick(object sender, EventArgs e)
        {
            _ = Task.Run(async () => await RefreshProxies());
        }

        private void AutoRefreshToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _refreshTimer.Enabled = autoRefreshToolStripMenuItem.Checked;
        }

        private void BackgroundModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _backgroundTimer.Enabled = backgroundModeToolStripMenuItem.Checked;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && backgroundModeToolStripMenuItem.Checked)
            {
                Hide();
                _trayIcon.Visible = true;
                _isMinimizedToTray = true;
            }
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            _trayIcon.Visible = false;
            _isMinimizedToTray = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Proxy Collector v1.0\n\nПрограмма для коллекционирования и управления прокси серверами.\n\nАвтор: AI Assistant", "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isMinimizedToTray)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            _proxyParser?.Dispose();
            _proxyChecker?.Dispose();
            _trayIcon?.Dispose();
            base.OnFormClosing(e);
        }

        // Controls
        private DataGridView dataGridViewProxies;
        private Panel panelControls;
        private Button btnRefresh;
        private Button btnParseNew;
        private Button btnCheckAll;
        private Button btnExport;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblTotalProxies;
        private Label lblAvailableProxies;
        private ComboBox cmbFilterType;
        private ComboBox cmbFilterCountry;
        private TextBox txtFilterIP;
        private CheckBox chkOnlyAvailable;
        private Button btnClearFilters;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem autoRefreshToolStripMenuItem;
        private ToolStripMenuItem backgroundModeToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private IContainer components;
    }
}
