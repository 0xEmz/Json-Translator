// LogWindow.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public delegate void RollNextRangeAction();

    public partial class LogWindow : Form
    {
        private RichTextBox rtbLog;
        private TableLayoutPanel buttonPanel;
        private Button btnStop;
        public Button btnRollNext;

        private Action _stopAction;
        private RollNextRangeAction _rollNextAction;

        public LogWindow(string title, Action stopAction, RollNextRangeAction rollNextAction, bool canRollNext)
        {
            InitializeComponent();

            _stopAction = stopAction;
            _rollNextAction = rollNextAction;

            this.Text = title;
            this.Size = new Size(700, 450);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.FormClosing += LogWindow_FormClosing;

            InitializeCustomComponents(canRollNext);
        }

        private void InitializeCustomComponents(bool canRollNext)
        {
            // --- Log Box ---
            rtbLog = new RichTextBox();
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.BackColor = Color.Black;
            rtbLog.ForeColor = Color.LimeGreen;
            rtbLog.Font = new Font("Consolas", 10F);
            rtbLog.ReadOnly = true;
            rtbLog.BorderStyle = BorderStyle.None;

            // --- Button Panel ---
            buttonPanel = new TableLayoutPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;
            buttonPanel.ColumnCount = 2;
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.BackColor = Color.FromArgb(45, 45, 48);

            // --- Stop Button ---
            btnStop = new Button();
            btnStop.Text = "🛑 Stop Translation";
            btnStop.Dock = DockStyle.Fill;
            btnStop.BackColor = Color.FromArgb(192, 0, 0);
            btnStop.ForeColor = Color.White;
            btnStop.FlatStyle = FlatStyle.Flat;
            // 🆕 يتم إرسال الإشارة فقط، الإغلاق يتم في Finally block في JsonTranslator
            btnStop.Click += (s, e) => { _stopAction?.Invoke(); };
            btnStop.Margin = new Padding(5, 5, 5, 5);

            // --- Roll Next Button ---
            btnRollNext = new Button();
            btnRollNext.Text = "🔁 Roll Next Instance";
            btnRollNext.Dock = DockStyle.Fill;
            btnRollNext.BackColor = Color.FromArgb(0, 122, 204);
            btnRollNext.ForeColor = Color.White;
            btnRollNext.FlatStyle = FlatStyle.Flat;
            btnRollNext.Enabled = canRollNext;
            btnRollNext.Click += (s, e) => {
                _rollNextAction?.Invoke();
                DisableRollNextButton();
            };
            btnRollNext.Margin = new Padding(5, 5, 5, 5);

            // إضافة الأزرار للـ Panel
            buttonPanel.Controls.Add(btnStop, 0, 0);
            buttonPanel.Controls.Add(btnRollNext, 1, 0);

            // إضافة العناصر للفورم
            this.Controls.Add(rtbLog);
            this.Controls.Add(buttonPanel);
            rtbLog.BringToFront();
        }

        public void DisableStopButton()
        {
            if (this.btnStop.InvokeRequired)
            {
                this.btnStop.Invoke(new Action(() => DisableStopButton()));
            }
            else
            {
                this.btnStop.Enabled = false;
                this.btnStop.Text = "Stopping...";
            }
        }

        public void DisableRollNextButton()
        {
            if (this.btnRollNext.InvokeRequired)
            {
                this.btnRollNext.Invoke(new Action(() => DisableRollNextButton()));
            }
            else
            {
                this.btnRollNext.Enabled = false;
            }
        }

        private void LogWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // منع الإغلاق التلقائي بزر X عشان العملية ما تتلخبطش
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                MessageBox.Show("Please stop the translation process using the 'Stop' button in this window before closing.", "Operation Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void AppendLog(string message, Color? color = null)
        {
            if (this.rtbLog.InvokeRequired)
            {
                this.rtbLog.Invoke(new Action(() => AppendLog(message, color)));
                return;
            }

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color ?? Color.LightGray;
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
            rtbLog.SelectionColor = rtbLog.ForeColor;
        }

        // 🆕 دالة جديدة للإغلاق الآمن (يمكن استدعاؤها من أي Thread)
        public void CloseWindow()
        {
            if (this.InvokeRequired)
            {
                // نطلب من الـ Thread الرئيسي إغلاق النافذة
                this.Invoke(new Action(() => CloseWindow()));
            }
            else
            {
                // نلغي المنع المؤقت للإغلاق
                this.FormClosing -= LogWindow_FormClosing;
                this.Close();
                this.Dispose();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 450);
            this.Name = "LogWindow";
            this.Text = "Log Output";
            this.ResumeLayout(false);
        }
    }
}