// EditPromptForm.cs
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class EditPromptForm : Form
    {
        private readonly string _filePath;
        private RichTextBox rtbContent;

        public EditPromptForm(string filePath)
        {
            _filePath = filePath;
            InitializeComponent();
            InitializeCustomComponents();
            LoadFileContent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Editing: {Path.GetFileName(_filePath)}";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = true;
            this.FormClosing += EditPromptForm_FormClosing;
        }

        private void InitializeCustomComponents()
        {
            // RichTextBox for Content
            rtbContent = new RichTextBox();
            rtbContent.Dock = DockStyle.Fill;
            rtbContent.BackColor = Color.FromArgb(37, 37, 38);
            rtbContent.ForeColor = Color.White;
            rtbContent.Font = new Font("Consolas", 10F);
            rtbContent.BorderStyle = BorderStyle.None;

            // Save Button
            var btnSave = new Button();
            btnSave.Text = "💾 Save Changes and Close";
            btnSave.Dock = DockStyle.Bottom;
            btnSave.Height = 40;
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(rtbContent);
            this.Controls.Add(btnSave);
            rtbContent.BringToFront();
        }

        private void LoadFileContent()
        {
            try
            {
                // قراءة المحتوى الحالي للمقارنة
                this.Tag = File.ReadAllText(_filePath, System.Text.Encoding.UTF8);
                rtbContent.Text = this.Tag.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}", "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(_filePath, rtbContent.Text, System.Text.Encoding.UTF8);
                this.Tag = rtbContent.Text; // تحديث الـ Tag عشان FormClosing ما يسألش تاني
                MessageBox.Show("Prompt saved successfully to file!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing file: {ex.Message}", "File Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditPromptForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // التحقق إذا كان المحتوى الحالي مختلف عن المحتوى اللي تم تحميله أو حفظه آخر مرة
            if (rtbContent.Text.Trim() != this.Tag?.ToString().Trim())
            {
                var result = MessageBox.Show("Content has changed. Save changes before closing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    BtnSave_Click(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}