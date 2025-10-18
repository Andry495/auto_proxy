using System;
using System.Collections.Generic;
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
        private readonly SettingsManager _settingsManager;
        private List<ProxyServer> _allProxies;
        private List<string> _actionLogs;
        private const int MaxLogEntries = 200;
        private NotifyIcon _trayIcon;
        private bool _isMinimizedToTray = false;
        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.Timer _backgroundTimer;
        private System.Windows.Forms.Timer _proxyUpdateTimer;
        private System.Windows.Forms.Timer _proxyCheckTimer;

        public MainForm()
        {
            InitializeComponent();
            
            _settingsManager = new SettingsManager();
            _proxyParser = new ProxyParser();
            _proxyParser.LogMessage += (message) => LogAction(message);
            _proxyChecker = new ProxyChecker(_settingsManager);
            _proxyStorage = new ProxyStorage();
            _allProxies = new List<ProxyServer>();
            _actionLogs = new List<string>();

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = _settingsManager.GetIntSetting("RefreshInterval", 30) * 1000;
            _refreshTimer.Tick += RefreshTimer_Tick;

            _backgroundTimer = new System.Windows.Forms.Timer();
            _backgroundTimer.Interval = _settingsManager.GetIntSetting("BackgroundInterval", 300) * 1000;
            _backgroundTimer.Tick += BackgroundTimer_Tick;

            // Таймер для обновления списка прокси с сайтов
            _proxyUpdateTimer = new System.Windows.Forms.Timer();
            _proxyUpdateTimer.Interval = _settingsManager.GetIntSetting("ProxyUpdateInterval", 60) * 60 * 1000; // минуты в миллисекунды
            _proxyUpdateTimer.Tick += ProxyUpdateTimer_Tick;

            // Таймер для проверки доступности прокси
            _proxyCheckTimer = new System.Windows.Forms.Timer();
            _proxyCheckTimer.Interval = _settingsManager.GetIntSetting("ProxyCheckInterval", 60) * 1000; // секунды в миллисекунды
            _proxyCheckTimer.Tick += ProxyCheckTimer_Tick;

            SetupTrayIcon();
            InitializeDataGridView();
            LogAction("Программа запущена");
            
            LoadSettings();
            LoadProxies();
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            _ = RefreshProxies();
        }

        private void BackgroundTimer_Tick(object? sender, EventArgs e)
        {
            _ = ParseNewProxies();
        }

        private async void ProxyUpdateTimer_Tick(object? sender, EventArgs e)
        {
            LogAction("Автоматическое обновление списка прокси с сайтов...");
            await RefreshProxies();
        }

        private async void ProxyCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (_allProxies.Any())
            {
                LogAction($"Проверка доступности {_allProxies.Count} прокси...");
                await CheckAllProxies();
            }
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = SystemIcons.Application;
            _trayIcon.Text = "Proxy Collector";
            _trayIcon.Visible = false;
            _trayIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                _trayIcon.Visible = false;
                _isMinimizedToTray = false;
                LogAction("Программа восстановлена из трея");
            };
        }

        private void InitializeDataGridView()
        {
            dataGridViewProxies.AutoGenerateColumns = false;
            dataGridViewProxies.Columns.Clear();

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IpAddress",
                HeaderText = "IP адрес",
                Name = "IpAddress",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Port",
                HeaderText = "Порт",
                Name = "Port",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Type",
                HeaderText = "Тип",
                Name = "Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Country",
                HeaderText = "Страна",
                Name = "Country",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ResponseTime",
                HeaderText = "Скорость (мс)",
                Name = "ResponseTime",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IsAvailable",
                HeaderText = "Доступен",
                Name = "IsAvailable",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LastChecked",
                HeaderText = "Последняя проверка",
                Name = "LastChecked",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridViewProxies.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Source",
                HeaderText = "Источник",
                Name = "Source",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void LoadSettings()
        {
            try
            {
                var autoRefresh = _settingsManager.GetBoolSetting("AutoRefresh", false);
                if (autoRefreshToolStripMenuItem != null)
                {
                    autoRefreshToolStripMenuItem.Checked = autoRefresh;
                    _refreshTimer.Enabled = autoRefresh;
                }

                var backgroundMode = _settingsManager.GetBoolSetting("BackgroundMode", false);
                if (backgroundModeToolStripMenuItem != null)
                {
                    backgroundModeToolStripMenuItem.Checked = backgroundMode;
                    _backgroundTimer.Enabled = backgroundMode;
                }

                // Запускаем таймеры для обновления и проверки прокси
                _proxyUpdateTimer.Enabled = true;
                _proxyCheckTimer.Enabled = true;

                LogAction("Настройки загружены из файла");
                LogAction($"Интервал обновления прокси: {_settingsManager.GetIntSetting("ProxyUpdateInterval", 60)} минут");
                LogAction($"Интервал проверки прокси: {_settingsManager.GetIntSetting("ProxyCheckInterval", 60)} секунд");
                LogAction($"URL для тестирования: {_settingsManager.GetStringSetting("TestUrl", "https://2ip.ru")}");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                _settingsManager.SetSetting("AutoRefresh", autoRefreshToolStripMenuItem.Checked.ToString());
                _settingsManager.SetSetting("RefreshInterval", (_refreshTimer.Interval / 1000).ToString());
                _settingsManager.SetSetting("BackgroundMode", backgroundModeToolStripMenuItem.Checked.ToString());
                _settingsManager.SetSetting("BackgroundInterval", (_backgroundTimer.Interval / 1000).ToString());
                _settingsManager.SetSetting("MinimizeToTray", "True");

                LogAction("Настройки сохранены в файл");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        private async void LoadProxies()
        {
            try
            {
                LogAction("Загрузка прокси из файла...");
                _allProxies = await _proxyStorage.LoadProxiesAsync();
                UpdateDisplay();
                LogAction($"Загружено {_allProxies.Count} прокси из файла");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при загрузке прокси: {ex.Message}");
            }
        }

        private void UpdateDisplay()
        {
            var sortedProxies = _allProxies
                .OrderByDescending(p => p.IsAvailable)
                .ThenBy(p => p.ResponseTime)
                .ToList();

            dataGridViewProxies.DataSource = null;
            dataGridViewProxies.DataSource = sortedProxies;
        }

        private void DataGridViewProxies_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                var column = dataGridViewProxies.Columns[e.ColumnIndex];
                var currentData = (List<ProxyServer>)dataGridViewProxies.DataSource;
                
                if (currentData == null) return;

                List<ProxyServer> sortedData;

                switch (column.Name)
                {
                    case "IsAvailable":
                        sortedData = currentData.OrderByDescending(p => p.IsAvailable).ThenBy(p => p.ResponseTime).ToList();
                        break;
                    case "ResponseTime":
                        sortedData = currentData.OrderBy(p => p.ResponseTime).ThenByDescending(p => p.IsAvailable).ToList();
                        break;
                    case "IpAddress":
                        sortedData = currentData.OrderBy(p => p.IpAddress).ToList();
                        break;
                    case "Port":
                        sortedData = currentData.OrderBy(p => p.Port).ToList();
                        break;
                    case "Type":
                        sortedData = currentData.OrderBy(p => p.Type).ToList();
                        break;
                    case "Country":
                        sortedData = currentData.OrderBy(p => p.Country).ToList();
                        break;
                    case "LastChecked":
                        sortedData = currentData.OrderByDescending(p => p.LastChecked).ToList();
                        break;
                    case "Source":
                        sortedData = currentData.OrderBy(p => p.Source).ToList();
                        break;
                    default:
                        // По умолчанию сортируем по доступности и скорости
                        sortedData = currentData.OrderByDescending(p => p.IsAvailable).ThenBy(p => p.ResponseTime).ToList();
                        break;
                }

                dataGridViewProxies.DataSource = null;
                dataGridViewProxies.DataSource = sortedData;
                
                LogAction($"Сортировка по колонке: {column.HeaderText}");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при сортировке: {ex.Message}");
            }
        }

        private void DataGridViewProxies_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dataGridViewProxies.Rows[e.RowIndex].DataBoundItem is ProxyServer proxy)
                {
                    // Недоступные серверы - красный фон
                    if (!proxy.IsAvailable)
                    {
                        e.CellStyle.BackColor = Color.LightCoral;
                        e.CellStyle.ForeColor = Color.DarkRed;
                        e.CellStyle.SelectionBackColor = Color.Red;
                        e.CellStyle.SelectionForeColor = Color.White;
                    }
                    else
                    {
                        // Доступные серверы - градиент от зеленого к желтому в зависимости от скорости
                        var responseTime = proxy.ResponseTime;
                        
                        if (responseTime <= 500) // Очень быстрые (0-500мс) - ярко-зеленый
                        {
                            e.CellStyle.BackColor = Color.LightGreen;
                            e.CellStyle.ForeColor = Color.DarkGreen;
                            e.CellStyle.SelectionBackColor = Color.Green;
                            e.CellStyle.SelectionForeColor = Color.White;
                        }
                        else if (responseTime <= 1000) // Быстрые (500-1000мс) - зеленый
                        {
                            e.CellStyle.BackColor = Color.LightGreen;
                            e.CellStyle.ForeColor = Color.DarkGreen;
                            e.CellStyle.SelectionBackColor = Color.Green;
                            e.CellStyle.SelectionForeColor = Color.White;
                        }
                        else if (responseTime <= 2000) // Средние (1000-2000мс) - желто-зеленый
                        {
                            e.CellStyle.BackColor = Color.LightYellow;
                            e.CellStyle.ForeColor = Color.DarkGreen;
                            e.CellStyle.SelectionBackColor = Color.Yellow;
                            e.CellStyle.SelectionForeColor = Color.Black;
                        }
                        else if (responseTime <= 3000) // Медленные (2000-3000мс) - желтый
                        {
                            e.CellStyle.BackColor = Color.Yellow;
                            e.CellStyle.ForeColor = Color.DarkOrange;
                            e.CellStyle.SelectionBackColor = Color.Orange;
                            e.CellStyle.SelectionForeColor = Color.White;
                        }
                        else // Очень медленные (3000мс+) - оранжевый
                        {
                            e.CellStyle.BackColor = Color.LightSalmon;
                            e.CellStyle.ForeColor = Color.DarkRed;
                            e.CellStyle.SelectionBackColor = Color.Orange;
                            e.CellStyle.SelectionForeColor = Color.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при форматировании ячейки: {ex.Message}");
            }
        }

        private void LogAction(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}";
            
            // Добавляем новую запись в начало списка (сверху)
            _actionLogs.Insert(0, logEntry);
            
            if (_actionLogs.Count > MaxLogEntries)
            {
                // Удаляем старые записи с конца списка
                _actionLogs.RemoveRange(MaxLogEntries, _actionLogs.Count - MaxLogEntries);
            }
            
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateLogDisplay()));
            }
            else
            {
                UpdateLogDisplay();
            }
        }

        private void UpdateLogDisplay()
        {
            if (txtActionLog != null)
            {
                txtActionLog.Text = string.Join(Environment.NewLine, _actionLogs);
                // Прокручиваем к началу (сверху), где находятся новые записи
                txtActionLog.SelectionStart = 0;
                txtActionLog.ScrollToCaret();
            }
        }

        private async Task RefreshProxies()
        {
            try
            {
                var startTime = DateTime.Now;
                LogAction("Начало обновления прокси...");
                
                // Парсим новые прокси с сайтов
                LogAction("Парсинг новых прокси с сайтов...");
                var newProxies = await _proxyParser.ParseAllSourcesAsync();
                
                if (newProxies.Any())
                {
                    var beforeCount = _allProxies.Count;
                    _allProxies.AddRange(newProxies);
                    _allProxies = _allProxies.DistinctBy(p => new { p.IpAddress, p.Port }).ToList();
                    var afterCount = _allProxies.Count;
                    var addedCount = afterCount - beforeCount;
                    
                    LogAction($"Добавлено {addedCount} новых прокси (было: {beforeCount}, стало: {afterCount})");
                }
                else
                {
                    LogAction("Новые прокси не найдены");
                }

                LogAction("Сохранение прокси в файл...");
                await _proxyStorage.SaveProxiesAsync(_allProxies);
                UpdateDisplay();
                
                var duration = (int)(DateTime.Now - startTime).TotalMilliseconds;
                LogAction($"Обновление завершено за {duration}мс. Всего прокси: {_allProxies.Count}");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при обновлении прокси: {ex.Message}");
            }
        }

        private async Task ParseNewProxies()
        {
            try
            {
                LogAction("Начало парсинга новых прокси...");
                
                var newProxies = await _proxyParser.ParseAllSourcesAsync();
                
                foreach (var proxy in newProxies)
                {
                    if (!_allProxies.Any(p => p.IpAddress == proxy.IpAddress && p.Port == proxy.Port))
                    {
                        _allProxies.Add(proxy);
                    }
                }

                await _proxyStorage.SaveProxiesAsync(_allProxies);
                UpdateDisplay();
                LogAction($"Парсинг завершен. Всего прокси: {_allProxies.Count}");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при парсинге прокси: {ex.Message}");
            }
        }

        private async Task CheckAllProxies()
        {
            try
            {
                var progress = new Progress<ProxyCheckProgress>(progress =>
                {
                    // Выделяем текущий проверяемый прокси
                    if (!string.IsNullOrEmpty(progress.CurrentProxy))
                    {
                        HighlightCurrentProxy(progress.CurrentProxy);
                    }
                    
                    // Логируем подробную информацию о проверке
                    if (!string.IsNullOrEmpty(progress.Status))
                    {
                        var statusMessage = $"{progress.CurrentProxy}: {progress.Status}";
                        if (progress.ResponseTime > 0)
                        {
                            statusMessage += $" (время: {progress.ResponseTime}мс)";
                        }
                        if (!string.IsNullOrEmpty(progress.ErrorMessage))
                        {
                            statusMessage += $" - {progress.ErrorMessage}";
                        }
                        LogAction(statusMessage);
                    }
                    
                    if (progress.IsComplete)
                    {
                        LogAction($"Проверка завершена. Проверено: {progress.Completed}/{progress.Total}");
                        ClearProxyHighlight();
                    }
                });

                var checkedProxies = await _proxyChecker.CheckProxiesAsync(_allProxies, progress);
                _allProxies = checkedProxies;
                
                await _proxyStorage.SaveProxiesAsync(_allProxies);
                UpdateDisplay();
                
                var availableCount = _allProxies.Count(p => p.IsAvailable);
                LogAction($"Проверка прокси завершена. Доступно: {availableCount}/{_allProxies.Count}");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при проверке прокси: {ex.Message}");
                ClearProxyHighlight();
            }
        }

        private void HighlightCurrentProxy(string proxyAddress)
        {
            try
            {
                if (dataGridViewProxies.InvokeRequired)
                {
                    dataGridViewProxies.Invoke(new Action<string>(HighlightCurrentProxy), proxyAddress);
                    return;
                }

                // Очищаем предыдущее выделение
                ClearProxyHighlight();

                // Находим строку с текущим прокси
                for (int i = 0; i < dataGridViewProxies.Rows.Count; i++)
                {
                    var row = dataGridViewProxies.Rows[i];
                    if (row.DataBoundItem is ProxyServer proxy && proxy.FullAddress == proxyAddress)
                    {
                        // Выделяем строку
                        row.Selected = true;
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        row.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
                        row.DefaultCellStyle.SelectionForeColor = Color.White;
                        
                        // Прокручиваем к выделенной строке
                        dataGridViewProxies.FirstDisplayedScrollingRowIndex = i;
                        dataGridViewProxies.Refresh();
                        
                        LogAction($"Проверяется прокси: {proxyAddress}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при выделении прокси: {ex.Message}");
            }
        }

        private void ClearProxyHighlight()
        {
            try
            {
                if (dataGridViewProxies.InvokeRequired)
                {
                    dataGridViewProxies.Invoke(new Action(ClearProxyHighlight));
                    return;
                }

                // Очищаем выделение всех строк
                dataGridViewProxies.ClearSelection();
                dataGridViewProxies.Refresh();
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при очистке выделения: {ex.Message}");
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                _trayIcon.Visible = true;
                _isMinimizedToTray = true;
                LogAction("Программа свернута в трей");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LogAction("Форма загружена");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|JSON files (*.json)|*.json",
                    DefaultExt = "txt"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _proxyStorage.SaveProxiesAsync(_allProxies).Wait();
                    LogAction($"Прокси экспортированы в {dialog.FileName}");
                    MessageBox.Show("Прокси успешно экспортированы", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при экспорте: {ex.Message}");
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Application.Exit();
        }

        private void AutoRefreshToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _refreshTimer.Enabled = autoRefreshToolStripMenuItem.Checked;
            SaveSettings();
        }

        private void BackgroundModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _backgroundTimer.Enabled = backgroundModeToolStripMenuItem.Checked;
            SaveSettings();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var settingsForm = new SettingsForm(_settingsManager))
                {
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        settingsForm.SaveSettings();
                        LoadSettings();
                        LogAction("Настройки обновлены");
                    }
                }
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при открытии настроек: {ex.Message}");
                MessageBox.Show($"Ошибка при открытии настроек: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Proxy Collector v1.0\n\n" +
                "Программа для коллекционирования и управления прокси серверами.\n\n" +
                "© 2025",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            _actionLogs.Clear();
            UpdateLogDisplay();
            LogAction("Лог очищен");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isMinimizedToTray)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            SaveSettings();
            _proxyParser?.Dispose();
            _proxyChecker?.Dispose();
            _trayIcon?.Dispose();
            base.OnFormClosing(e);
        }
    }
}

