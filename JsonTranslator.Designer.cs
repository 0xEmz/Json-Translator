// JsonTranslator.Designer.cs - (تعديل) إضافة زرار حفظ الإعدادات

using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    partial class JsonTranslator
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            groupBoxPaths = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnBrowseLog = new Button();
            txtLogFolder = new TextBox();
            labelLogFolder = new Label();
            btnBrowseOutput = new Button();
            txtOutputFolder = new TextBox();
            labelOutputFolder = new Label();
            btnBrowseInput = new Button();
            txtInputFolder = new TextBox();
            labelInputFolder = new Label();
            groupBoxKeys = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            labelKeysCount = new Label();
            btnBrowseKeys = new Button();
            txtKeysFilePath = new TextBox();
            labelKeysFile = new Label();
            groupBoxPrompt = new GroupBox();
            btnViewEditPrompt = new Button();
            btnBrowsePrompt = new Button();
            txtPromptFilePath = new TextBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            btnStartTranslation = new Button();
            labelFileRange = new Label();
            cmbFileRange = new ComboBox();
            chkUseLocalOnly = new CheckBox();
            chkPreserveTags = new CheckBox();

            // 🆕 [إضافة] تعريف الكونترولز الجديدة
            groupBoxModels = new GroupBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            labelLocalModel = new Label();
            txtLocalModelName = new TextBox();
            labelCloudModel = new Label();
            txtCloudModelName = new TextBox();

            // 🆕 [إضافة] زرار الحفظ
            tableLayoutPanel5 = new TableLayoutPanel();
            btnSaveSettings = new Button();

            groupBoxPaths.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBoxKeys.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupBoxPrompt.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            groupBoxModels.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel5.SuspendLayout(); // 🆕
            SuspendLayout();
            // 
            // groupBoxPaths
            // 
            groupBoxPaths.BackColor = Color.FromArgb(45, 45, 48);
            groupBoxPaths.Controls.Add(tableLayoutPanel1);
            groupBoxPaths.Dock = DockStyle.Top;
            groupBoxPaths.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            groupBoxPaths.ForeColor = Color.LightGray;
            groupBoxPaths.Location = new Point(10, 10);
            groupBoxPaths.Name = "groupBoxPaths";
            groupBoxPaths.Padding = new Padding(5);
            groupBoxPaths.Size = new Size(930, 175);
            groupBoxPaths.TabIndex = 0;
            groupBoxPaths.TabStop = false;
            groupBoxPaths.Text = "Path Settings";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tableLayoutPanel1.Controls.Add(btnBrowseLog, 2, 2);
            tableLayoutPanel1.Controls.Add(txtLogFolder, 1, 2);
            tableLayoutPanel1.Controls.Add(labelLogFolder, 0, 2);
            tableLayoutPanel1.Controls.Add(btnBrowseOutput, 2, 1);
            tableLayoutPanel1.Controls.Add(txtOutputFolder, 1, 1);
            tableLayoutPanel1.Controls.Add(labelOutputFolder, 0, 1);
            tableLayoutPanel1.Controls.Add(btnBrowseInput, 2, 0);
            tableLayoutPanel1.Controls.Add(txtInputFolder, 1, 0);
            tableLayoutPanel1.Controls.Add(labelInputFolder, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(5, 30);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.Size = new Size(920, 140);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnBrowseLog
            // 
            btnBrowseLog.BackColor = Color.FromArgb(60, 60, 60);
            btnBrowseLog.Dock = DockStyle.Fill;
            btnBrowseLog.FlatStyle = FlatStyle.Flat;
            btnBrowseLog.Font = new Font("Segoe UI", 11F);
            btnBrowseLog.ForeColor = Color.White;
            btnBrowseLog.Location = new Point(803, 95);
            btnBrowseLog.Name = "btnBrowseLog";
            btnBrowseLog.Size = new Size(114, 42);
            btnBrowseLog.TabIndex = 8;
            btnBrowseLog.Text = "🔍";
            btnBrowseLog.UseVisualStyleBackColor = false;
            // 
            // txtLogFolder
            // 
            txtLogFolder.BackColor = Color.FromArgb(37, 37, 38);
            txtLogFolder.BorderStyle = BorderStyle.FixedSingle;
            txtLogFolder.Dock = DockStyle.Fill;
            txtLogFolder.Font = new Font("Segoe UI", 12F);
            txtLogFolder.ForeColor = Color.White;
            txtLogFolder.Location = new Point(183, 95);
            txtLogFolder.Name = "txtLogFolder";
            txtLogFolder.Size = new Size(614, 29);
            txtLogFolder.TabIndex = 7;
            // 
            // labelLogFolder
            // 
            labelLogFolder.AutoSize = true;
            labelLogFolder.Dock = DockStyle.Fill;
            labelLogFolder.Font = new Font("Segoe UI", 11F);
            labelLogFolder.Location = new Point(3, 92);
            labelLogFolder.Name = "labelLogFolder";
            labelLogFolder.RightToLeft = RightToLeft.No;
            labelLogFolder.Size = new Size(174, 48);
            labelLogFolder.TabIndex = 6;
            labelLogFolder.Text = "Log Folder Path:";
            labelLogFolder.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.BackColor = Color.FromArgb(60, 60, 60);
            btnBrowseOutput.Dock = DockStyle.Fill;
            btnBrowseOutput.FlatStyle = FlatStyle.Flat;
            btnBrowseOutput.Font = new Font("Segoe UI", 11F);
            btnBrowseOutput.ForeColor = Color.White;
            btnBrowseOutput.Location = new Point(803, 49);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(114, 40);
            btnBrowseOutput.TabIndex = 5;
            btnBrowseOutput.Text = "🔍";
            btnBrowseOutput.UseVisualStyleBackColor = false;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.BackColor = Color.FromArgb(37, 37, 38);
            txtOutputFolder.BorderStyle = BorderStyle.FixedSingle;
            txtOutputFolder.Dock = DockStyle.Fill;
            txtOutputFolder.Font = new Font("Segoe UI", 12F);
            txtOutputFolder.ForeColor = Color.White;
            txtOutputFolder.Location = new Point(183, 49);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new Size(614, 29);
            txtOutputFolder.TabIndex = 4;
            // 
            // labelOutputFolder
            // 
            labelOutputFolder.AutoSize = true;
            labelOutputFolder.Dock = DockStyle.Fill;
            labelOutputFolder.Font = new Font("Segoe UI", 11F);
            labelOutputFolder.Location = new Point(3, 46);
            labelOutputFolder.Name = "labelOutputFolder";
            labelOutputFolder.RightToLeft = RightToLeft.No;
            labelOutputFolder.Size = new Size(174, 46);
            labelOutputFolder.TabIndex = 3;
            labelOutputFolder.Text = "Output Path:";
            labelOutputFolder.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnBrowseInput
            // 
            btnBrowseInput.BackColor = Color.FromArgb(60, 60, 60);
            btnBrowseInput.Dock = DockStyle.Fill;
            btnBrowseInput.FlatStyle = FlatStyle.Flat;
            btnBrowseInput.Font = new Font("Segoe UI", 11F);
            btnBrowseInput.ForeColor = Color.White;
            btnBrowseInput.Location = new Point(803, 3);
            btnBrowseInput.Name = "btnBrowseInput";
            btnBrowseInput.Size = new Size(114, 40);
            btnBrowseInput.TabIndex = 2;
            btnBrowseInput.Text = "📂";
            btnBrowseInput.UseVisualStyleBackColor = false;
            // 
            // txtInputFolder
            // 
            txtInputFolder.BackColor = Color.FromArgb(37, 37, 38);
            txtInputFolder.BorderStyle = BorderStyle.FixedSingle;
            txtInputFolder.Dock = DockStyle.Fill;
            txtInputFolder.Font = new Font("Segoe UI", 12F);
            txtInputFolder.ForeColor = Color.White;
            txtInputFolder.Location = new Point(183, 3);
            txtInputFolder.Name = "txtInputFolder";
            txtInputFolder.Size = new Size(614, 29);
            txtInputFolder.TabIndex = 1;
            // 
            // labelInputFolder
            // 
            labelInputFolder.AutoSize = true;
            labelInputFolder.Dock = DockStyle.Fill;
            labelInputFolder.Font = new Font("Segoe UI", 11F);
            labelInputFolder.Location = new Point(3, 0);
            labelInputFolder.Name = "labelInputFolder";
            labelInputFolder.RightToLeft = RightToLeft.No;
            labelInputFolder.Size = new Size(174, 46);
            labelInputFolder.TabIndex = 0;
            labelInputFolder.Text = "Input Path (JSON):";
            labelInputFolder.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBoxKeys
            // 
            groupBoxKeys.BackColor = Color.FromArgb(45, 45, 48);
            groupBoxKeys.Controls.Add(tableLayoutPanel2);
            groupBoxKeys.Dock = DockStyle.Top;
            groupBoxKeys.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            groupBoxKeys.ForeColor = Color.LightGray;
            groupBoxKeys.Location = new Point(10, 285);
            groupBoxKeys.Name = "groupBoxKeys";
            groupBoxKeys.Size = new Size(930, 120);
            groupBoxKeys.TabIndex = 2;
            groupBoxKeys.TabStop = false;
            groupBoxKeys.Text = "API Keys Pool (Mistral)";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tableLayoutPanel2.Controls.Add(labelKeysCount, 0, 1);
            tableLayoutPanel2.Controls.Add(btnBrowseKeys, 2, 0);
            tableLayoutPanel2.Controls.Add(txtKeysFilePath, 1, 0);
            tableLayoutPanel2.Controls.Add(labelKeysFile, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 28);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(924, 89);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // labelKeysCount
            // 
            labelKeysCount.AutoSize = true;
            tableLayoutPanel2.SetColumnSpan(labelKeysCount, 3);
            labelKeysCount.Dock = DockStyle.Fill;
            labelKeysCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelKeysCount.ForeColor = Color.Yellow;
            labelKeysCount.Location = new Point(3, 44);
            labelKeysCount.Name = "labelKeysCount";
            labelKeysCount.RightToLeft = RightToLeft.No;
            labelKeysCount.Size = new Size(918, 45);
            labelKeysCount.TabIndex = 3;
            labelKeysCount.Text = "Loaded Keys: 0";
            labelKeysCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnBrowseKeys
            // 
            btnBrowseKeys.BackColor = Color.FromArgb(60, 60, 60);
            btnBrowseKeys.Dock = DockStyle.Fill;
            btnBrowseKeys.FlatStyle = FlatStyle.Flat;
            btnBrowseKeys.Font = new Font("Segoe UI", 11F);
            btnBrowseKeys.ForeColor = Color.White;
            btnBrowseKeys.Location = new Point(807, 3);
            btnBrowseKeys.Name = "btnBrowseKeys";
            btnBrowseKeys.Size = new Size(114, 38);
            btnBrowseKeys.TabIndex = 2;
            btnBrowseKeys.Text = "📂";
            btnBrowseKeys.UseVisualStyleBackColor = false;
            // 
            // txtKeysFilePath
            // 
            txtKeysFilePath.BackColor = Color.FromArgb(37, 37, 38);
            txtKeysFilePath.BorderStyle = BorderStyle.FixedSingle;
            txtKeysFilePath.Dock = DockStyle.Fill;
            txtKeysFilePath.Font = new Font("Segoe UI", 12F);
            txtKeysFilePath.ForeColor = Color.White;
            txtKeysFilePath.Location = new Point(183, 3);
            txtKeysFilePath.Name = "txtKeysFilePath";
            txtKeysFilePath.Size = new Size(618, 29);
            txtKeysFilePath.TabIndex = 0;
            // 
            // labelKeysFile
            // 
            labelKeysFile.AutoSize = true;
            labelKeysFile.Dock = DockStyle.Fill;
            labelKeysFile.Font = new Font("Segoe UI", 11F);
            labelKeysFile.Location = new Point(3, 0);
            labelKeysFile.Name = "labelKeysFile";
            labelKeysFile.RightToLeft = RightToLeft.No;
            labelKeysFile.Size = new Size(174, 44);
            labelKeysFile.TabIndex = 1;
            labelKeysFile.Text = "Keys File (.txt):";
            labelKeysFile.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBoxPrompt
            // 
            groupBoxPrompt.BackColor = Color.FromArgb(45, 45, 48);
            groupBoxPrompt.Controls.Add(btnViewEditPrompt);
            groupBoxPrompt.Controls.Add(btnBrowsePrompt);
            groupBoxPrompt.Controls.Add(txtPromptFilePath);
            groupBoxPrompt.Dock = DockStyle.Top;
            groupBoxPrompt.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            groupBoxPrompt.ForeColor = Color.LightGray;
            groupBoxPrompt.Location = new Point(10, 405);
            groupBoxPrompt.Name = "groupBoxPrompt";
            groupBoxPrompt.Padding = new Padding(5);
            groupBoxPrompt.Size = new Size(930, 130);
            groupBoxPrompt.TabIndex = 7;
            groupBoxPrompt.TabStop = false;
            groupBoxPrompt.Text = "Translation Prompt File (.txt)";
            // 
            // btnViewEditPrompt
            // 
            btnViewEditPrompt.BackColor = Color.FromArgb(60, 60, 60);
            btnViewEditPrompt.Dock = DockStyle.Top;
            btnViewEditPrompt.FlatStyle = FlatStyle.Flat;
            btnViewEditPrompt.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnViewEditPrompt.ForeColor = Color.White;
            btnViewEditPrompt.Location = new Point(5, 59);
            btnViewEditPrompt.Name = "btnViewEditPrompt";
            btnViewEditPrompt.Size = new Size(800, 40);
            btnViewEditPrompt.TabIndex = 2;
            btnViewEditPrompt.Text = "📝 View / Edit Prompt Content";
            btnViewEditPrompt.UseVisualStyleBackColor = false;
            // 
            // btnBrowsePrompt
            // 
            btnBrowsePrompt.BackColor = Color.FromArgb(60, 60, 60);
            btnBrowsePrompt.Dock = DockStyle.Right;
            btnBrowsePrompt.FlatStyle = FlatStyle.Flat;
            btnBrowsePrompt.Font = new Font("Segoe UI", 11F);
            btnBrowsePrompt.ForeColor = Color.White;
            btnBrowsePrompt.Location = new Point(805, 59);
            btnBrowsePrompt.Name = "btnBrowsePrompt";
            btnBrowsePrompt.Size = new Size(120, 66);
            btnBrowsePrompt.TabIndex = 1;
            btnBrowsePrompt.Text = "📂 Browse";
            btnBrowsePrompt.UseVisualStyleBackColor = false;
            // 
            // txtPromptFilePath
            // 
            txtPromptFilePath.BackColor = Color.FromArgb(37, 37, 38);
            txtPromptFilePath.BorderStyle = BorderStyle.FixedSingle;
            txtPromptFilePath.Dock = DockStyle.Top;
            txtPromptFilePath.Font = new Font("Segoe UI", 12F);
            txtPromptFilePath.ForeColor = Color.White;
            txtPromptFilePath.Location = new Point(5, 30);
            txtPromptFilePath.Name = "txtPromptFilePath";
            txtPromptFilePath.Size = new Size(920, 29);
            txtPromptFilePath.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(btnStartTranslation, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Top;
            tableLayoutPanel3.Location = new Point(10, 695); // 🆕 تعديل مكان زرار البدء
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(930, 70);
            tableLayoutPanel3.TabIndex = 10; // 🆕 تعديل الترتيب
            // 
            // btnStartTranslation
            // 
            btnStartTranslation.BackColor = Color.FromArgb(0, 122, 204);
            btnStartTranslation.Dock = DockStyle.Fill;
            btnStartTranslation.FlatAppearance.BorderSize = 0;
            btnStartTranslation.FlatStyle = FlatStyle.Flat;
            btnStartTranslation.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnStartTranslation.ForeColor = Color.White;
            btnStartTranslation.Location = new Point(3, 3);
            btnStartTranslation.Name = "btnStartTranslation";
            btnStartTranslation.Size = new Size(924, 64);
            btnStartTranslation.TabIndex = 3;
            btnStartTranslation.Text = "🚀 Start Translation Process";
            btnStartTranslation.UseVisualStyleBackColor = false;
            // 
            // labelFileRange
            // 
            labelFileRange.AutoSize = true;
            labelFileRange.Dock = DockStyle.Top;
            labelFileRange.Font = new Font("Segoe UI", 12F);
            labelFileRange.ForeColor = Color.LightGray;
            labelFileRange.Location = new Point(10, 185);
            labelFileRange.Name = "labelFileRange";
            labelFileRange.RightToLeft = RightToLeft.No;
            labelFileRange.Size = new Size(244, 21);
            labelFileRange.TabIndex = 5;
            labelFileRange.Text = "File Range to Process (per 400):";
            labelFileRange.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbFileRange
            // 
            cmbFileRange.BackColor = Color.FromArgb(37, 37, 38);
            cmbFileRange.Dock = DockStyle.Top;
            cmbFileRange.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFileRange.FlatStyle = FlatStyle.Flat;
            cmbFileRange.Font = new Font("Segoe UI", 12F);
            cmbFileRange.ForeColor = Color.White;
            cmbFileRange.FormattingEnabled = true;
            cmbFileRange.Location = new Point(10, 206);
            cmbFileRange.Name = "cmbFileRange";
            cmbFileRange.Size = new Size(930, 29);
            cmbFileRange.TabIndex = 6;
            // 
            // chkUseLocalOnly
            // 
            chkUseLocalOnly.AutoSize = true;
            chkUseLocalOnly.Dock = DockStyle.Top;
            chkUseLocalOnly.Font = new Font("Segoe UI", 12F);
            chkUseLocalOnly.ForeColor = Color.FromArgb(255, 192, 128);
            chkUseLocalOnly.Location = new Point(10, 235);
            chkUseLocalOnly.Name = "chkUseLocalOnly";
            chkUseLocalOnly.Size = new Size(930, 25);
            chkUseLocalOnly.TabIndex = 9;
            chkUseLocalOnly.Text = "Use Local Model Only (Ollama)";
            chkUseLocalOnly.UseVisualStyleBackColor = true;
            // 
            // chkPreserveTags
            // 
            chkPreserveTags.AutoSize = true;
            chkPreserveTags.Dock = DockStyle.Top;
            chkPreserveTags.Font = new Font("Segoe UI", 12F);
            chkPreserveTags.ForeColor = Color.FromArgb(100, 200, 255);
            chkPreserveTags.Location = new Point(10, 260);
            chkPreserveTags.Name = "chkPreserveTags";
            chkPreserveTags.Size = new Size(930, 25);
            chkPreserveTags.TabIndex = 10;
            chkPreserveTags.Text = "Preserve TAGS (e.g., [T_1], [BR_1]) - (Slice Mode)";
            chkPreserveTags.UseVisualStyleBackColor = true;
            // 
            // groupBoxModels
            // 
            groupBoxModels.BackColor = Color.FromArgb(45, 45, 48);
            groupBoxModels.Controls.Add(tableLayoutPanel4);
            groupBoxModels.Dock = DockStyle.Top;
            groupBoxModels.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            groupBoxModels.ForeColor = Color.LightGray;
            groupBoxModels.Location = new Point(10, 535);
            groupBoxModels.Name = "groupBoxModels";
            groupBoxModels.Padding = new Padding(5);
            groupBoxModels.Size = new Size(930, 100);
            groupBoxModels.TabIndex = 8;
            groupBoxModels.TabStop = false;
            groupBoxModels.Text = "Ollama Model Names";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(labelLocalModel, 0, 0);
            tableLayoutPanel4.Controls.Add(txtLocalModelName, 1, 0);
            tableLayoutPanel4.Controls.Add(labelCloudModel, 0, 1);
            tableLayoutPanel4.Controls.Add(txtCloudModelName, 1, 1);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(5, 30);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Size = new Size(920, 65);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // labelLocalModel
            // 
            labelLocalModel.AutoSize = true;
            labelLocalModel.Dock = DockStyle.Fill;
            labelLocalModel.Font = new Font("Segoe UI", 11F);
            labelLocalModel.Location = new Point(3, 0);
            labelLocalModel.Name = "labelLocalModel";
            labelLocalModel.RightToLeft = RightToLeft.No;
            labelLocalModel.Size = new Size(194, 32);
            labelLocalModel.TabIndex = 0;
            labelLocalModel.Text = "Local Model Name:";
            labelLocalModel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtLocalModelName
            // 
            txtLocalModelName.BackColor = Color.FromArgb(37, 37, 38);
            txtLocalModelName.BorderStyle = BorderStyle.FixedSingle;
            txtLocalModelName.Dock = DockStyle.Fill;
            txtLocalModelName.Font = new Font("Segoe UI", 12F);
            txtLocalModelName.ForeColor = Color.White;
            txtLocalModelName.Location = new Point(203, 3);
            txtLocalModelName.Name = "txtLocalModelName";
            txtLocalModelName.Size = new Size(714, 29);
            txtLocalModelName.TabIndex = 1;
            // 
            // labelCloudModel
            // 
            labelCloudModel.AutoSize = true;
            labelCloudModel.Dock = DockStyle.Fill;
            labelCloudModel.Font = new Font("Segoe UI", 11F);
            labelCloudModel.Location = new Point(3, 32);
            labelCloudModel.Name = "labelCloudModel";
            labelCloudModel.RightToLeft = RightToLeft.No;
            labelCloudModel.Size = new Size(194, 33);
            labelCloudModel.TabIndex = 2;
            labelCloudModel.Text = "Secondary Cloud Model:";
            labelCloudModel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCloudModelName
            // 
            txtCloudModelName.BackColor = Color.FromArgb(37, 37, 38);
            txtCloudModelName.BorderStyle = BorderStyle.FixedSingle;
            txtCloudModelName.Dock = DockStyle.Fill;
            txtCloudModelName.Font = new Font("Segoe UI", 12F);
            txtCloudModelName.ForeColor = Color.White;
            txtCloudModelName.Location = new Point(203, 35);
            txtCloudModelName.Name = "txtCloudModelName";
            txtCloudModelName.Size = new Size(714, 29);
            txtCloudModelName.TabIndex = 3;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(btnSaveSettings, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Top;
            tableLayoutPanel5.Location = new Point(10, 640); // 🆕 مكان زرار الحفظ
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(930, 50); // 🆕 حجم زرار الحفظ
            tableLayoutPanel5.TabIndex = 9; // 🆕 ترتيب
            // 
            // btnSaveSettings
            // 
            btnSaveSettings.BackColor = Color.FromArgb(0, 100, 0); // 🆕 لون أخضر
            btnSaveSettings.Dock = DockStyle.Fill;
            btnSaveSettings.FlatAppearance.BorderSize = 0;
            btnSaveSettings.FlatStyle = FlatStyle.Flat;
            btnSaveSettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSaveSettings.ForeColor = Color.White;
            btnSaveSettings.Location = new Point(3, 3);
            btnSaveSettings.Name = "btnSaveSettings";
            btnSaveSettings.Size = new Size(924, 44);
            btnSaveSettings.TabIndex = 0;
            btnSaveSettings.Text = "💾 Save Settings";
            btnSaveSettings.UseVisualStyleBackColor = false;
            // 
            // JsonTranslator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(950, 775); // 🆕 زيادة طول الواجهة
            Controls.Add(tableLayoutPanel3);
            Controls.Add(tableLayoutPanel5); // 🆕 إضافة بانل زرار الحفظ
            Controls.Add(groupBoxModels);
            Controls.Add(groupBoxPrompt);
            Controls.Add(groupBoxKeys);
            Controls.Add(chkPreserveTags);
            Controls.Add(chkUseLocalOnly);
            Controls.Add(cmbFileRange);
            Controls.Add(labelFileRange);
            Controls.Add(groupBoxPaths);
            Font = new Font("Segoe UI", 11F);
            Name = "JsonTranslator";
            Padding = new Padding(10);
            RightToLeft = RightToLeft.No;
            Text = "🚀 LLM Translation Tool - Zelda JSON";
            groupBoxPaths.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupBoxKeys.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            groupBoxPrompt.ResumeLayout(false);
            groupBoxPrompt.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            groupBoxModels.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false); // 🆕
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox groupBoxPaths;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnBrowseLog;
        public TextBox txtLogFolder;
        private Label labelLogFolder;
        private Button btnBrowseOutput;
        public TextBox txtOutputFolder;
        private Label labelOutputFolder;
        private Button btnBrowseInput;
        public TextBox txtInputFolder;
        private Label labelInputFolder;
        private GroupBox groupBoxKeys;
        private TableLayoutPanel tableLayoutPanel2;
        public TextBox txtKeysFilePath;
        public Button btnBrowseKeys;
        private Label labelKeysFile;
        public Label labelKeysCount;
        public GroupBox groupBoxPrompt;
        public TextBox txtPromptFilePath;
        public Button btnBrowsePrompt;
        public Button btnViewEditPrompt;
        private TableLayoutPanel tableLayoutPanel3;
        public Button btnStartTranslation;
        private Label labelFileRange;
        public ComboBox cmbFileRange;
        public CheckBox chkUseLocalOnly;
        public CheckBox chkPreserveTags;

        // 🆕 [إضافة] تعريف المتغيرات الجديدة
        private GroupBox groupBoxModels;
        private TableLayoutPanel tableLayoutPanel4;
        private Label labelLocalModel;
        public TextBox txtLocalModelName;
        private Label labelCloudModel;
        public TextBox txtCloudModelName;

        // 🆕 [إضافة] زرار الحفظ
        private TableLayoutPanel tableLayoutPanel5;
        public Button btnSaveSettings;
    }
}