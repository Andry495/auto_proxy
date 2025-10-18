using System;
using System.Drawing;
using System.Windows.Forms;
using ProxyCollector.Services;

namespace ProxyCollector
{
    public partial class SettingsForm : Form
    {
        private readonly SettingsManager _settingsManager;
        
        // Элементы управления
        private GroupBox groupBoxGeneral;
        private CheckBox checkBoxAutoRefresh;
        private NumericUpDown numericUpDownRefreshInterval;
        private Label labelRefreshInterval;
        
        private GroupBox groupBoxBackground;
        private CheckBox checkBoxBackgroundMode;
        private NumericUpDown numericUpDownBackgroundInterval;
        private Label labelBackgroundInterval;
        
        private GroupBox groupBoxProxy;
        private NumericUpDown numericUpDownProxyUpdateInterval;
        private Label labelProxyUpdateInterval;
        private NumericUpDown numericUpDownProxyCheckInterval;
        private Label labelProxyCheckInterval;
        private TextBox textBoxTestUrl;
        private Label labelTestUrl;
        private NumericUpDown numericUpDownTestTimeout;
        private Label labelTestTimeout;
        
        private Button buttonSave;
        private Button buttonCancel;
        private Button buttonReset;

        public SettingsForm(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Настройки формы
            this.Text = "Настройки Proxy Collector";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            
            // Группа "Общие настройки"
            groupBoxGeneral = new GroupBox
            {
                Text = "Общие настройки",
                Location = new Point(20, 20),
                Size = new Size(450, 100),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            
            checkBoxAutoRefresh = new CheckBox
            {
                Text = "Автообновление",
                Location = new Point(20, 30),
                Size = new Size(150, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            labelRefreshInterval = new Label
            {
                Text = "Интервал обновления (секунды):",
                Location = new Point(20, 60),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            numericUpDownRefreshInterval = new NumericUpDown
            {
                Location = new Point(250, 58),
                Size = new Size(80, 20),
                Minimum = 10,
                Maximum = 3600,
                Value = 30,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            groupBoxGeneral.Controls.AddRange(new Control[] 
            { 
                checkBoxAutoRefresh, 
                labelRefreshInterval, 
                numericUpDownRefreshInterval 
            });
            
            // Группа "Фоновый режим"
            groupBoxBackground = new GroupBox
            {
                Text = "Фоновый режим",
                Location = new Point(20, 130),
                Size = new Size(450, 100),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            
            checkBoxBackgroundMode = new CheckBox
            {
                Text = "Фоновый режим",
                Location = new Point(20, 30),
                Size = new Size(150, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            labelBackgroundInterval = new Label
            {
                Text = "Интервал фонового режима (секунды):",
                Location = new Point(20, 60),
                Size = new Size(250, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            numericUpDownBackgroundInterval = new NumericUpDown
            {
                Location = new Point(300, 58),
                Size = new Size(80, 20),
                Minimum = 60,
                Maximum = 3600,
                Value = 300,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            groupBoxBackground.Controls.AddRange(new Control[] 
            { 
                checkBoxBackgroundMode, 
                labelBackgroundInterval, 
                numericUpDownBackgroundInterval 
            });
            
            // Группа "Настройки прокси"
            groupBoxProxy = new GroupBox
            {
                Text = "Настройки прокси",
                Location = new Point(20, 240),
                Size = new Size(450, 250),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            
            labelProxyUpdateInterval = new Label
            {
                Text = "Интервал обновления прокси (минуты):",
                Location = new Point(20, 30),
                Size = new Size(250, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            numericUpDownProxyUpdateInterval = new NumericUpDown
            {
                Location = new Point(300, 28),
                Size = new Size(80, 20),
                Minimum = 1,
                Maximum = 1440,
                Value = 60,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            labelProxyCheckInterval = new Label
            {
                Text = "Интервал проверки прокси (секунды):",
                Location = new Point(20, 60),
                Size = new Size(250, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            numericUpDownProxyCheckInterval = new NumericUpDown
            {
                Location = new Point(300, 58),
                Size = new Size(80, 20),
                Minimum = 10,
                Maximum = 3600,
                Value = 60,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            labelTestUrl = new Label
            {
                Text = "URL для тестирования прокси:",
                Location = new Point(20, 90),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            textBoxTestUrl = new TextBox
            {
                Location = new Point(20, 110),
                Size = new Size(360, 20),
                Text = "https://2ip.ru",
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            labelTestTimeout = new Label
            {
                Text = "Таймаут тестирования (секунды):",
                Location = new Point(20, 140),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            numericUpDownTestTimeout = new NumericUpDown
            {
                Location = new Point(250, 138),
                Size = new Size(80, 20),
                Minimum = 1,
                Maximum = 60,
                Value = 10,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            groupBoxProxy.Controls.AddRange(new Control[] 
            { 
                labelProxyUpdateInterval, 
                numericUpDownProxyUpdateInterval,
                labelProxyCheckInterval,
                numericUpDownProxyCheckInterval,
                labelTestUrl,
                textBoxTestUrl,
                labelTestTimeout,
                numericUpDownTestTimeout
            });
            
            // Кнопки
            buttonSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(200, 510),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            buttonCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(290, 510),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            buttonReset = new Button
            {
                Text = "Сброс",
                Location = new Point(380, 510),
                Size = new Size(80, 30),
                Font = new Font("Microsoft Sans Serif", 9F)
            };
            
            buttonReset.Click += ButtonReset_Click;
            
            // Добавляем все элементы на форму
            this.Controls.AddRange(new Control[] 
            { 
                groupBoxGeneral, 
                groupBoxBackground, 
                groupBoxProxy, 
                buttonSave, 
                buttonCancel, 
                buttonReset 
            });
            
            this.ResumeLayout(false);
        }

        private void LoadSettings()
        {
            try
            {
                checkBoxAutoRefresh.Checked = _settingsManager.GetBoolSetting("AutoRefresh", true);
                numericUpDownRefreshInterval.Value = _settingsManager.GetIntSetting("RefreshInterval", 30);
                
                checkBoxBackgroundMode.Checked = _settingsManager.GetBoolSetting("BackgroundMode", false);
                numericUpDownBackgroundInterval.Value = _settingsManager.GetIntSetting("BackgroundInterval", 300);
                
                numericUpDownProxyUpdateInterval.Value = _settingsManager.GetIntSetting("ProxyUpdateInterval", 60);
                numericUpDownProxyCheckInterval.Value = _settingsManager.GetIntSetting("ProxyCheckInterval", 60);
                textBoxTestUrl.Text = _settingsManager.GetStringSetting("TestUrl", "https://2ip.ru");
                numericUpDownTestTimeout.Value = _settingsManager.GetIntSetting("TestTimeout", 10);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке настроек: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите сбросить все настройки к значениям по умолчанию?", 
                "Подтверждение сброса", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                _settingsManager.ResetToDefaults();
                LoadSettings();
                MessageBox.Show("Настройки сброшены к значениям по умолчанию.", "Сброс настроек", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void SaveSettings()
        {
            try
            {
                _settingsManager.SetBoolSetting("AutoRefresh", checkBoxAutoRefresh.Checked);
                _settingsManager.SetIntSetting("RefreshInterval", (int)numericUpDownRefreshInterval.Value);
                
                _settingsManager.SetBoolSetting("BackgroundMode", checkBoxBackgroundMode.Checked);
                _settingsManager.SetIntSetting("BackgroundInterval", (int)numericUpDownBackgroundInterval.Value);
                
                _settingsManager.SetIntSetting("ProxyUpdateInterval", (int)numericUpDownProxyUpdateInterval.Value);
                _settingsManager.SetIntSetting("ProxyCheckInterval", (int)numericUpDownProxyCheckInterval.Value);
                _settingsManager.SetStringSetting("TestUrl", textBoxTestUrl.Text.Trim());
                _settingsManager.SetIntSetting("TestTimeout", (int)numericUpDownTestTimeout.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
