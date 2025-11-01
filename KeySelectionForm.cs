// KeySelectionForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp1
{
    /// <summary>
    /// نافذة منبثقة بسيطة تتيح للمستخدم اختيار مفتاحين (أساسي وثانوي) من قائمة المفاتيح المتاحة.
    /// </summary>
    public partial class KeySelectionForm : Form
    {
        // 🆕 الخصائص اللي هترجع المفاتيح المختارة
        public string PrimaryKey { get; private set; } = string.Empty;
        public string SecondaryKey { get; private set; } = string.Empty;

        private readonly List<string> _availableKeys;
        private ComboBox cmbKey1;
        private ComboBox cmbKey2;

        public KeySelectionForm(List<string> availableKeys)
        {
            _availableKeys = availableKeys;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            // إعدادات النافذة الأساسية
            this.Text = "Select API Keys for Job";
            this.Size = new Size(450, 270);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // --- Label 1 ---
            var lblKey1 = new Label
            {
                Text = "Primary Key (Required):",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.LightGray
            };
            this.Controls.Add(lblKey1);

            // --- ComboBox 1 (Primary Key) ---
            cmbKey1 = new ComboBox
            {
                Location = new Point(20, 45),
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            // إضافة المفاتيح المتاحة
            cmbKey1.Items.Add("-- Select Primary Key --");
            cmbKey1.Items.AddRange(_availableKeys.ToArray());
            cmbKey1.SelectedIndex = 0;
            this.Controls.Add(cmbKey1);

            // --- Label 2 ---
            var lblKey2 = new Label
            {
                Text = "Secondary Key (Optional):",
                Location = new Point(20, 95),
                AutoSize = true,
                ForeColor = Color.LightGray
            };
            this.Controls.Add(lblKey2);

            // --- ComboBox 2 (Secondary Key - Optional) ---
            cmbKey2 = new ComboBox
            {
                Location = new Point(20, 120),
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            // إضافة المفاتيح المتاحة (مع خيار "None")
            cmbKey2.Items.Add("-- None --");
            cmbKey2.Items.AddRange(_availableKeys.ToArray());
            cmbKey2.SelectedIndex = 0;
            this.Controls.Add(cmbKey2);

            // --- OK Button ---
            var btnOk = new Button
            {
                Text = "Confirm & Start Job",
                Location = new Point(20, 180),
                Width = 400,
                Height = 40,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(btnOk);

            // ربط زرار الـ OK بحدث التأكيد
            this.AcceptButton = btnOk;

            // --- منطق التحقق ---
            this.FormClosing += KeySelectionForm_FormClosing;
        }

        private void KeySelectionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                string key1 = cmbKey1.SelectedItem?.ToString();
                string key2 = cmbKey2.SelectedItem?.ToString();

                // 1. التحقق من اختيار المفتاح الأساسي
                if (key1 == null || key1.StartsWith("-- Select"))
                {
                    MessageBox.Show("Please select a Primary Key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                // 2. التحقق من أن المفتاحين مختلفان
                if (key2 != null && !key2.StartsWith("-- None") && key1 == key2)
                {
                    MessageBox.Show("Primary and Secondary keys must be different.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                // حفظ الاختيار
                this.PrimaryKey = key1;
                this.SecondaryKey = key2.StartsWith("-- None") ? string.Empty : key2;
            }
        }
    }
}