// JsonTranslator.cs - تعديلات منطق دوران مفاتيح Gemini

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json; // 🆕 [إضافة] لاستخدام JSON

namespace WinFormsApp1
{
    // (تم نقل كلاسات الإعدادات لملف PersistentSettings.cs)

    public partial class JsonTranslator : Form
    {
        // --- إعدادات الخط المخصص ---
        private PrivateFontCollection privateFonts = new PrivateFontCollection();
        private Font customBaseFont;
        private const string FONT_FILENAME = "hacen.ttf";

        // 🆕 Job Manager Variables
        private HashSet<string> _usedKeys = new HashSet<string>(); // (دي خاصة بميسترال بس)
        private List<string> _rangeQueue = new List<string>();
        private Dictionary<LogWindow, CancellationTokenSource> _ctsMap = new Dictionary<LogWindow, CancellationTokenSource>();
        private List<LogWindow> _activeLogWindows = new List<LogWindow>();

        // 🆕 [إضافة] مؤشر دوران مفاتيح جيميني (ثابت)
        private static int _nextGeminiKeyIndex = 0;
        private static readonly object _geminiKeyRotationLock = new object(); // للقفل أثناء اختيار المفتاح

        private const string FALLBACK_PROMPT_MESSAGE = "ERROR: No valid prompt file loaded. Using a minimal fallback instruction.";
        private string _currentPromptContent = FALLBACK_PROMPT_MESSAGE;


        public JsonTranslator()
        {
            LoadCustomFont();

            InitializeComponent();

            this.Text = "🚀 LLM Translation Tool - Zelda JSON";

            // تطبيق الخطوط وتعديل الأحجام
            if (customBaseFont != null)
            {
                ApplyCustomFont(this.Controls);

                this.groupBoxPaths.Font = new Font(customBaseFont.FontFamily, 14F, FontStyle.Bold);
                this.groupBoxKeys.Font = new Font(customBaseFont.FontFamily, 14F, FontStyle.Bold);
                this.groupBoxPrompt.Font = new Font(customBaseFont.FontFamily, 14F, FontStyle.Bold);
                this.groupBoxModels.Font = new Font(customBaseFont.FontFamily, 14F, FontStyle.Bold);

                this.btnStartTranslation.Font = new Font(customBaseFont.FontFamily, 16F, FontStyle.Bold);
                this.btnViewEditPrompt.Font = new Font(customBaseFont.FontFamily, 12F, FontStyle.Bold);
                this.btnSaveSettings.Font = new Font(customBaseFont.FontFamily, 12F, FontStyle.Bold);

                this.labelKeysCount.Font = new Font(customBaseFont.FontFamily, 12F, FontStyle.Bold);

                this.txtInputFolder.Font = new Font("Segoe UI", 12F);
                this.txtOutputFolder.Font = new Font("Segoe UI", 12F);
                this.txtLogFolder.Font = new Font("Segoe UI", 12F);
                this.txtKeysFilePath.Font = new Font("Segoe UI", 12F);
                this.txtPromptFilePath.Font = new Font("Segoe UI", 12F);
                this.cmbFileRange.Font = new Font("Segoe UI", 12F);
                this.labelFileRange.Font = new Font(customBaseFont.FontFamily, 12F);
                this.chkUseLocalOnly.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                this.chkPreserveTags.Font = new Font("Segoe UI", 12F);

                this.txtLocalModelName.Font = new Font("Segoe UI", 12F);
                this.txtCloudModelName.Font = new Font("Segoe UI", 12F);
            }

            // تحميل الإعدادات عند البداية
            LoadSettingsFromFile();

            // التأكد من وجود مفتاح جيميني
            bool geminiReady = TranslationSettings.AllGeminiKeys.Any(k => !string.IsNullOrWhiteSpace(k) && !k.StartsWith("PUT_YOUR_GEMINI_KEY"));
            if (!geminiReady)
            {
                MessageBox.Show("Warning: No valid Gemini API Keys found in TranslationSettings.cs. Gemini model will be skipped.", "API Key Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // --- ربط الأحداث (Event Handlers) ---
            btnBrowseInput.Click += (s, e) => btnBrowse_Click(txtInputFolder);
            btnBrowseOutput.Click += (s, e) => btnBrowse_Click(txtOutputFolder);
            btnBrowseLog.Click += (s, e) => btnBrowse_Click(txtLogFolder);
            btnStartTranslation.Click += async (s, e) => await btnStartTranslation_Click(s, e);
            btnBrowseKeys.Click += btnBrowseKeys_Click;
            btnBrowsePrompt.Click += btnBrowsePrompt_Click;
            btnViewEditPrompt.Click += btnViewEditPrompt_Click;

            btnSaveSettings.Click += BtnSaveSettings_Click;
        }

        // --- 🆕 [دالة جديدة] منطق دوران مفاتيح جيميني ---
        /// <summary>
        /// يختار المفتاح التالي المتاح من قائمة مفاتيح Gemini الثابتة.
        /// </summary>
        /// <returns>Tuple يحتوي على المفتاح المختار والـ Label الخاص به (مثل: "Gemini Key #3").</returns>
        private (string key, string label) GetNextGeminiKey()
        {
            // نستخدم القفل لضمان أن مفتاحًا واحدًا فقط يتم اختياره في المرة الواحدة
            lock (_geminiKeyRotationLock)
            {
                // نفلتر المفاتيح غير الصالحة (المكان اللي لسه محطش فيه مفتاح)
                var availableKeys = TranslationSettings.AllGeminiKeys
                    .Where(k => !string.IsNullOrWhiteSpace(k) && !k.StartsWith("PUT_YOUR_GEMINI_KEY"))
                    .ToList();

                if (availableKeys.Count == 0)
                {
                    return (string.Empty, "No Valid Gemini Keys");
                }

                // نستخدم باقي القسمة (Modulo) عشان نرجع للبداية لما نوصل للآخر
                int indexInList = _nextGeminiKeyIndex % availableKeys.Count;
                string selectedKey = availableKeys[indexInList];

                // نحدد الـ Index الأصلي اللي هيظهر في اللوج (من 1)
                int originalPoolIndex = TranslationSettings.AllGeminiKeys.IndexOf(selectedKey) + 1;

                // نلف على المفتاح التالي للشغلانة القادمة
                _nextGeminiKeyIndex++;

                return (selectedKey, $"Gemini Key #{originalPoolIndex}");
            }
        }


        // --- دوال الخطوط ---
        private void LoadCustomFont()
        {
            try
            {
                if (File.Exists(FONT_FILENAME))
                {
                    privateFonts.AddFontFile(FONT_FILENAME);
                    customBaseFont = new Font(privateFonts.Families[0], 12F, FontStyle.Regular);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading custom font: {ex.Message}");
            }
        }

        private void ApplyCustomFont(Control.ControlCollection controls)
        {
            if (customBaseFont == null) return;

            foreach (Control control in controls)
            {
                if (control is TextBox || control is RichTextBox || control is ComboBox) { }
                else { control.Font = new Font(customBaseFont.FontFamily, control.Font.Size, control.Font.Style); }

                if (control.HasChildren) { ApplyCustomFont(control.Controls); }
            }
        }

        // ---------------------------------------------------------------------
        //                           دوال الـ Utility للواجهة 
        // ---------------------------------------------------------------------

        // دوال حفظ وتحميل الإعدادات
        private void LoadSettingsFromFile()
        {
            var settings = SettingsManager.LoadSettings();
            txtInputFolder.Text = settings.InputPath;
            txtOutputFolder.Text = settings.OutputPath;
            txtLogFolder.Text = settings.LogPath;
            txtKeysFilePath.Text = settings.KeysFilePath;
            txtPromptFilePath.Text = settings.PromptFilePath;
            txtLocalModelName.Text = settings.LocalModelName;
            txtCloudModelName.Text = settings.CloudModelName;

            if (!string.IsNullOrWhiteSpace(settings.InputPath))
            {
                PopulateRangeComboBox(settings.InputPath);
            }
            if (!string.IsNullOrWhiteSpace(settings.KeysFilePath))
            {
                LoadMistralKeysFromFile(settings.KeysFilePath);
            }
            if (!string.IsNullOrWhiteSpace(settings.PromptFilePath))
            {
                LoadPromptContentFromFile(settings.PromptFilePath, true);
            }
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            var settings = new PersistentSettings
            {
                InputPath = txtInputFolder.Text.Trim(),
                OutputPath = txtOutputFolder.Text.Trim(),
                LogPath = txtLogFolder.Text.Trim(),
                KeysFilePath = txtKeysFilePath.Text.Trim(),
                PromptFilePath = txtPromptFilePath.Text.Trim(),
                LocalModelName = txtLocalModelName.Text.Trim(),
                CloudModelName = txtCloudModelName.Text.Trim()
            };
            SettingsManager.SaveSettings(settings);
        }

        private void btnBrowse_Click(TextBox targetTextBox)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = fbd.SelectedPath;
                    if (targetTextBox == txtInputFolder) { PopulateRangeComboBox(fbd.SelectedPath); }
                }
            }
        }

        private void btnBrowseKeys_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt";
                ofd.Title = "Select Mistral API Keys File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtKeysFilePath.Text = ofd.FileName;
                    LoadMistralKeysFromFile(ofd.FileName);
                }
            }
        }

        private void LoadMistralKeysFromFile(string path)
        {
            TranslationSettings.AllMistralKeys.Clear();
            _usedKeys.Clear();

            if (!File.Exists(path))
            {
                labelKeysCount.Text = "Loaded Keys: 0 (File Not Found)";
                labelKeysCount.ForeColor = Color.Yellow;
                return;
            }

            try
            {
                var keys = File.ReadAllLines(path)
                               .Select(key => key.Trim())
                               .Where(key => !string.IsNullOrEmpty(key) && !key.StartsWith("//") && key.Length > 10)
                               .ToList();

                TranslationSettings.AllMistralKeys.AddRange(keys);

                labelKeysCount.Text = $"Loaded Keys: {TranslationSettings.AllMistralKeys.Count}";
                labelKeysCount.ForeColor = TranslationSettings.AllMistralKeys.Count > 0 ? Color.LimeGreen : Color.Yellow;
            }
            catch (Exception ex)
            {
                labelKeysCount.Text = "Loaded Keys: 0 (Error Reading File)";
                labelKeysCount.ForeColor = Color.Red;
                MessageBox.Show($"Error reading keys file: {ex.Message}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowsePrompt_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt";
                ofd.Title = "Select Translation Prompt File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPromptFilePath.Text = ofd.FileName;
                    LoadPromptContentFromFile(ofd.FileName);
                }
            }
        }

        private void btnViewEditPrompt_Click(object sender, EventArgs e)
        {
            string path = txtPromptFilePath.Text.Trim();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show("Please select a valid prompt file first using the Browse button.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var editForm = new EditPromptForm(path))
                {
                    editForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open editor. Error: {ex.Message}", "Editor Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadPromptContentFromFile(path);
        }

        private void LoadPromptContentFromFile(string path, bool initialLoad = false)
        {
            string systemLogPath = Path.Combine(string.IsNullOrWhiteSpace(txtLogFolder.Text.Trim()) ? AppDomain.CurrentDomain.BaseDirectory : txtLogFolder.Text.Trim(), "System_Log.txt");

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                _currentPromptContent = FALLBACK_PROMPT_MESSAGE;
                if (!initialLoad && !string.IsNullOrWhiteSpace(txtLogFolder.Text.Trim()))
                {
                    LogSystemMessage(systemLogPath, "Warning: Prompt file path is empty or file not found. Using minimal fallback instruction.", false);
                }
                return;
            }

            try
            {
                _currentPromptContent = File.ReadAllText(path, System.Text.Encoding.UTF8).Trim();
                if (string.IsNullOrWhiteSpace(_currentPromptContent))
                {
                    _currentPromptContent = FALLBACK_PROMPT_MESSAGE;
                    LogSystemMessage(systemLogPath, "Warning: Prompt file is empty. Using minimal fallback instruction.", false);
                }
                if (!initialLoad)
                {
                    LogSystemMessage(systemLogPath, $"Successfully loaded prompt from: {Path.GetFileName(path)}", false);
                }
            }
            catch (Exception ex)
            {
                _currentPromptContent = FALLBACK_PROMPT_MESSAGE;
                LogSystemMessage(systemLogPath, $"ERROR: Failed to read prompt file. Using fallback. Detail: {ex.Message}", false);
            }
        }


        private void PopulateRangeComboBox(string inputPath)
        {
            if (cmbFileRange.InvokeRequired)
            {
                cmbFileRange.Invoke(new Action(() => PopulateRangeComboBox(inputPath)));
                return;
            }

            cmbFileRange.Items.Clear();
            cmbFileRange.Items.Add("الكل");
            labelFileRange.Text = "File Range to Process (per 400):";

            if (string.IsNullOrWhiteSpace(inputPath) || !Directory.Exists(inputPath))
            {
                cmbFileRange.SelectedIndex = 0;
                return;
            }

            try
            {
                var jsonFiles = Directory.GetFiles(inputPath, "*.json");
                Array.Sort(jsonFiles);
                int totalFiles = jsonFiles.Length;
                const int chunkSize = 400;

                for (int i = 0; i < totalFiles; i += chunkSize)
                {
                    int start = i + 1;
                    int end = Math.Min(i + chunkSize, totalFiles);
                    string rangeText = $"من {start} إلى {end}";
                    cmbFileRange.Items.Add(rangeText);
                }

                cmbFileRange.SelectedIndex = 0;
            }
            catch (Exception) { }
        }

        private bool ValidateInputs()
        {
            if (!Directory.Exists(txtInputFolder.Text.Trim())) { MessageBox.Show("من فضلك تأكد من إدخال مسار مجلد الإدخال صحيح.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
            if (!Directory.Exists(txtOutputFolder.Text.Trim())) { MessageBox.Show("من فضلك تأكد من إدخال مسار مجلد الإخراج صحيح.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
            if (!Directory.Exists(txtLogFolder.Text.Trim())) { MessageBox.Show("من فضلك تأكد من إدخال مسار مجلد السجل صحيح.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }

            if (!chkUseLocalOnly.Checked)
            {
                bool geminiReady = TranslationSettings.AllGeminiKeys.Any(k => !string.IsNullOrWhiteSpace(k) && !k.StartsWith("PUT_YOUR_GEMINI_KEY"));
                bool mistralReady = TranslationSettings.AllMistralKeys.Count > 0;

                if (!geminiReady && !mistralReady)
                {
                    MessageBox.Show("لم يتم تحميل مفاتيح Mistral أو إدخال مفتاح Gemini. من فضلك قم بتحميل المفاتيح أولاً (أو اختر 'Use Local Model Only').", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(txtPromptFilePath.Text.Trim()) || !File.Exists(txtPromptFilePath.Text.Trim())) { MessageBox.Show("من فضلك تأكد من تحديد ملف البرومبت (.txt) موجود وصحيح.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }

            if (string.IsNullOrWhiteSpace(txtLocalModelName.Text.Trim())) { MessageBox.Show("من فضلك أدخل اسم الموديل المحلي (Local Model Name).", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
            if (!chkUseLocalOnly.Checked && string.IsNullOrWhiteSpace(txtCloudModelName.Text.Trim())) { MessageBox.Show("من فضلك أدخل اسم الموديل الكلاود (Secondary Cloud Model) طالما أنك لا تستخدم الوضع المحلي فقط.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }

            return true;
        }

        private void LogSystemMessage(string filePath, string message, bool showInUI = true)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                File.AppendAllText(filePath, logEntry + Environment.NewLine, System.Text.Encoding.UTF8);

                if (showInUI && _activeLogWindows.Count > 0)
                {
                    _activeLogWindows.First().AppendLog(logEntry, Color.LightGray);
                }
            }
            catch (Exception) { }
        }

        private void StopTranslationJob(LogWindow windowToStop)
        {
            if (_ctsMap.ContainsKey(windowToStop))
            {
                _ctsMap[windowToStop].Cancel();
                LogSystemMessage(Path.Combine(txtLogFolder.Text.Trim(), "System_Log.txt"),
                           $"🛑 Cancellation requested for job in window: {windowToStop.Text}");

                windowToStop.BeginInvoke(new Action(() =>
                {
                    windowToStop.DisableStopButton();
                }));
            }
        }

        private void RollNextInstance()
        {
            if (_rangeQueue.Count > 0) { btnStartTranslation_Click(null, EventArgs.Empty); }
        }

        private void SetupRangeQueue()
        {
            _rangeQueue.Clear();
            if (!Directory.Exists(txtInputFolder.Text.Trim())) return;

            try
            {
                var jsonFiles = Directory.GetFiles(txtInputFolder.Text.Trim(), "*.json");
                Array.Sort(jsonFiles);
                int totalFiles = jsonFiles.Length;
                const int chunkSize = 400;

                for (int i = 0; i < totalFiles; i += chunkSize)
                {
                    int start = i + 1;
                    int end = Math.Min(i + chunkSize, totalFiles);
                    _rangeQueue.Add($"من {start} إلى {end}");
                }
            }
            catch (Exception) { }
        }

        private (string key1, string key2) ShowKeySelectionDialog()
        {
            if (chkUseLocalOnly.Checked)
            {
                return (string.Empty, string.Empty);
            }

            var availableKeys = TranslationSettings.AllMistralKeys.Except(_usedKeys).ToList();
            // 🆕 [تعديل] لو مفيش مفاتيح ميسترال، متسألش
            if (availableKeys.Count == 0)
            {
                return (string.Empty, string.Empty);
            }

            using (var selectionForm = new KeySelectionForm(availableKeys))
            {
                if (selectionForm.ShowDialog(this) == DialogResult.OK) { return (selectionForm.PrimaryKey, selectionForm.SecondaryKey); }
                return (null, null);
            }
        }


        private async Task btnStartTranslation_Click(object sender, EventArgs e)
        {
            string selectedRange = null;
            int? startIdx = null;
            int? endIdx = null;

            LogWindow currentLogWindow = null;
            CancellationTokenSource cts = null;
            string key1 = null, key2 = null;

            // 🆕 [إضافة] متغيرات لجيميني
            string geminiKey = string.Empty;
            string geminiLabel = "Gemini Disabled";

            var settings = new TranslationSettings();

            try
            {
                // --- 1. التحقق وتحديد النطاق (Queue Management) ---
                if (sender == null)
                {
                    if (_rangeQueue.Count == 0) return;
                    selectedRange = _rangeQueue[0];
                    _rangeQueue.RemoveAt(0);
                }
                else
                {
                    if (!ValidateInputs()) return;

                    SetupRangeQueue();
                    if (_rangeQueue.Count == 0)
                    {
                        MessageBox.Show("لا توجد ملفات JSON في مجلد المدخلات المختار.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string initialRange = cmbFileRange.SelectedItem.ToString();
                    if (initialRange != "الكل")
                    {
                        int selectedIndex = cmbFileRange.Items.IndexOf(initialRange);
                        if (selectedIndex > 0)
                        {
                            _rangeQueue = _rangeQueue.Skip(selectedIndex - 1).ToList();
                        }
                    }

                    selectedRange = _rangeQueue[0];
                    _rangeQueue.RemoveAt(0);

                    this.Enabled = false;
                    btnStartTranslation.Enabled = false;
                }

                LoadPromptContentFromFile(txtPromptFilePath.Text);

                if (_currentPromptContent == FALLBACK_PROMPT_MESSAGE)
                {
                    MessageBox.Show("Cannot start job: Prompt content is empty or failed to load. Please check the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new OperationCanceledException("Cannot start job due to missing prompt content.");
                }


                // --- 2. اختيار المفاتيح (Key Selection) ---
                if (!chkUseLocalOnly.Checked)
                {
                    // 🆕 [إضافة] اختيار مفتاح جيميني بالدوران
                    (geminiKey, geminiLabel) = GetNextGeminiKey();

                    // 🆕 [تعديل] سؤال بس لو فيه مفاتيح ميسترال
                    if (TranslationSettings.AllMistralKeys.Count > 0)
                    {
                        var keys = ShowKeySelectionDialog();
                        key1 = keys.key1;
                        key2 = keys.key2;

                        if (key1 == null) // (لو المستخدم قفل النافذة)
                        {
                            if (sender == null) _rangeQueue.Insert(0, selectedRange);
                            throw new OperationCanceledException("Key selection cancelled by user.");
                        }

                        if (!string.IsNullOrEmpty(key1)) _usedKeys.Add(key1);
                        if (!string.IsNullOrEmpty(key2)) _usedKeys.Add(key2);
                    }
                    else
                    {
                        key1 = string.Empty;
                        key2 = string.Empty;
                    }
                }

                // --- 3. تجهيز الإعدادات والـ CancellationToken ---

                cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;

                string userPrompt = _currentPromptContent;
                const string INPUT_OUTPUT_TEMPLATE =
                    @"\n
**English text to translate:**
""{0}""

**Arabic Translation:**
";
                string finalPromptTemplate = userPrompt + INPUT_OUTPUT_TEMPLATE;

                if (selectedRange != "الكل")
                {
                    string[] parts = selectedRange.Split(' ');
                    if (parts.Length == 4 && parts[0] == "من" && parts[2] == "إلى"
                        && int.TryParse(parts[1], out int start) && int.TryParse(parts[3], out int end))
                    {
                        startIdx = start;
                        endIdx = end;
                    }
                }

                bool useLocalOnly = chkUseLocalOnly.Checked;
                bool preserveTags = chkPreserveTags.Checked;

                settings.InputPath = txtInputFolder.Text.Trim();
                settings.OutputPath = txtOutputFolder.Text.Trim();
                settings.LogPath = txtLogFolder.Text.Trim();
                settings.MistralKey1 = key1;
                settings.MistralKey2 = key2;
                settings.SelectedGeminiKey = geminiKey; // 🆕 [إضافة]
                settings.StartIndex = startIdx;
                settings.EndIndex = endIdx;
                settings.PromptTemplate = finalPromptTemplate;
                settings.UseLocalOnly = useLocalOnly;
                settings.PreserveTags = preserveTags;

                settings.LocalModelName = txtLocalModelName.Text.Trim();
                settings.CloudModelName = txtCloudModelName.Text.Trim();


                // --- 4. فتح النافذة وبدء العملية ---
                // 🆕 [تعديل] تحديث العنوان عشان يعرض مفتاح جيميني
                string keyInfo = chkUseLocalOnly.Checked ? "[Local Only]" :
                                 (!string.IsNullOrWhiteSpace(geminiKey) ? $"[{geminiLabel}]" : "[Gemini OFF]") +
                                 (!string.IsNullOrWhiteSpace(key1) ? $" [M: {key1.Substring(0, 4)}...]" : "");

                string jobTitle = $"Job: {selectedRange} | {keyInfo} | Rem: {_rangeQueue.Count}";
                bool canRollNext = _rangeQueue.Count > 0;

                currentLogWindow = new LogWindow(
                    jobTitle,
                    () => StopTranslationJob(currentLogWindow),
                    RollNextInstance,
                    canRollNext
                );
                currentLogWindow.Show();

                _activeLogWindows.Add(currentLogWindow);
                _ctsMap.Add(currentLogWindow, cts);

                // -------------------------------------------------------------
                // 🔑 تعريف دالة التسجيل الخاصة بهذه المهمة (Closure)
                // -------------------------------------------------------------
                LogAction jobSpecificLogger = (filePath, message, showInUI) =>
                {
                    try
                    {
                        LogSystemMessage(filePath, message, false);
                        if (showInUI && currentLogWindow != null)
                        {
                            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                            currentLogWindow.AppendLog(logEntry, Color.LightGray);
                        }
                    }
                    catch (Exception) { }
                };
                // -------------------------------------------------------------

                LogSystemMessage(Path.Combine(txtLogFolder.Text.Trim(), "System_Log.txt"), $"✅ Starting new job for range: {selectedRange}");
                // 🆕 [تعديل] تسجيل مفتاح جيميني في اللوج
                if (!useLocalOnly)
                {
                    LogSystemMessage(Path.Combine(txtLogFolder.Text.Trim(), "System_Log.txt"), $"🔑 Using {geminiLabel} (Key: {geminiKey.Substring(0, 4)}...)");
                    if (!string.IsNullOrEmpty(key1))
                    {
                        LogSystemMessage(Path.Combine(txtLogFolder.Text.Trim(), "System_Log.txt"), $"🔑 Using Mistral Keys: Primary ({key1.Substring(0, 4)}...) / Secondary ({(string.IsNullOrEmpty(key2) ? "None" : key2.Substring(0, 4) + "...")})");
                    }
                }
                currentLogWindow.AppendLog($"Starting job for range: {selectedRange}. {(useLocalOnly ? "(Local Model Only)" : $"(Using {geminiLabel})")}", Color.Yellow);


                var processor = new TranslationProcessor(settings, jobSpecificLogger);
                await Task.Run(() => processor.ProcessFiles(token));

                if (token.IsCancellationRequested)
                {
                    LogSystemMessage(Path.Combine(settings.LogPath, "System_Log.txt"), "\n🛑 تم إلغاء العملية بناءً على طلب المستخدم.");
                }
                else
                {
                    LogSystemMessage(Path.Combine(settings.LogPath, "System_Log.txt"), "\n--- انتهت عملية الترجمة بالكامل بنجاح ---");
                }
            }
            catch (OperationCanceledException)
            {
                if (selectedRange != null) LogSystemMessage(Path.Combine(txtLogFolder.Text.Trim(), "System_Log.txt"), $"Job {selectedRange} cancelled.");
            }
            catch (Exception ex)
            {
                LogSystemMessage(Path.Combine(txtLogFolder.Text.Trim(), "Main_Error.txt"), $"خطأ قاتل في البرنامج: {ex.Message}");
            }
            finally
            {
                if (key1 != null) _usedKeys.Remove(key1);
                if (key2 != null) _usedKeys.Remove(key2);
                // (مفاتيح جيميني مش محتاجة "تحرير" لأنها بتلف لوحدها)

                if (currentLogWindow != null)
                {
                    _activeLogWindows.Remove(currentLogWindow);
                    if (cts != null) _ctsMap.Remove(currentLogWindow);

                    currentLogWindow.BeginInvoke(new Action(() => {
                        currentLogWindow.CloseWindow();
                    }));
                }

                if (cts != null) cts.Dispose();

                if (_activeLogWindows.Count == 0)
                {
                    this.Enabled = true;
                    btnStartTranslation.Enabled = true;
                }

                bool stillCanRoll = _rangeQueue.Count > 0;
                foreach (var win in _activeLogWindows)
                {
                    win.BeginInvoke(new Action(() => {
                        if (stillCanRoll) win.btnRollNext.Enabled = true;
                        else win.btnRollNext.Enabled = false;
                    }));
                }
            }
        }
    }
}