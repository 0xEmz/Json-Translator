// TranslationProcessor.cs - (تعديل)
// 1. إضافة "ريتراي" لجيميني
// 2. إلغاء "الريتراي" من ميسترال
// 3. قراءة مفتاح جيميني المختار من الإعدادات

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;

namespace WinFormsApp1
{
    public delegate void LogAction(string filePath, string message, bool showInUI);

    public class TranslationProcessor
    {
        private readonly TranslationSettings _settings;
        private readonly LogAction _logger;
        private readonly HttpClient _httpClient;
        private readonly string _promptTemplate;

        public TranslationProcessor(TranslationSettings settings, LogAction logger)
        {
            _settings = settings;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(TranslationSettings.REQUEST_TIMEOUT_SECONDS);
            _promptTemplate = settings.PromptTemplate;
        }

        // --- 1. دوال Utility المساعدة ---
        private string CleanLlmResponse(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) return string.Empty;
            responseText = responseText.Trim();

            if (responseText.StartsWith('"') && responseText.EndsWith('"') && responseText.Length > 1) responseText = responseText.Substring(1, responseText.Length - 2);
            else if (responseText.StartsWith("'") && responseText.EndsWith("'") && responseText.Length > 1) responseText = responseText.Substring(1, responseText.Length - 2);

            responseText = responseText.Replace("**", "");
            responseText = responseText.Replace("*", "");

            responseText = Regex.Replace(responseText, @"[ \t]+", " ");

            return responseText.Trim();
        }

        // --- 2. الـ Data Models (تم إضافة موديلات جيميني) ---
        #region Data Models
        // Mistral
        private class MistralMessage { [JsonProperty("content")] public string Content { get; set; } = string.Empty; }
        private class MistralChoice { [JsonProperty("message")] public MistralMessage Message { get; set; } = new MistralMessage(); }
        private class MistralError { [JsonProperty("message")] public string Message { get; set; } = string.Empty; }
        private class MistralResponse { [JsonProperty("choices")] public List<MistralChoice> Choices { get; set; } = new List<MistralChoice>(); [JsonProperty("error")] public MistralError? Error { get; set; } }

        // Ollama
        private class OllamaResponse { [JsonProperty("response")] public string Response { get; set; } = string.Empty; }

        // 🆕 [إضافة] Gemini
        private class GeminiTextPart { [JsonProperty("text")] public string Text { get; set; } = string.Empty; }
        private class GeminiContent { [JsonProperty("parts")] public List<GeminiTextPart> Parts { get; set; } = new List<GeminiTextPart>(); }
        private class GeminiCandidate { [JsonProperty("content")] public GeminiContent Content { get; set; } = new GeminiContent(); }
        private class GeminiPromptFeedback { [JsonProperty("blockReason")] public string BlockReason { get; set; } = string.Empty; }
        private class GeminiResponse
        {
            [JsonProperty("candidates")] public List<GeminiCandidate> Candidates { get; set; } = new List<GeminiCandidate>();
            [JsonProperty("promptFeedback")] public GeminiPromptFeedback? PromptFeedback { get; set; }
        }

        // Config
        private class TranslationConfig
        {
            public string Model { get; set; } = string.Empty;
            public string Api { get; set; } = string.Empty;
            public string KeyLabel { get; set; } = string.Empty;
            public string? ApiKey { get; set; } // (ده هيفضل null في حالة جيميني)
            public int SleepSeconds { get; set; } = 1;
        }
        #endregion

        // --- 3. دوال API الفعلية (تم إضافة دالة جيميني) ---

        // 🆕 [إضافة] دالة جديدة خاصة بجيميني
        private async Task<(string translation, string errorType, string successSource)> TranslateWithGemini(string cleanText, string modelName, int attemptNum, string fileLogPath)
        {
            // 🆕 [تعديل] قراءة المفتاح المختار من الإعدادات
            string apiKey = _settings.SelectedGeminiKey;
            // 🆕 [تعديل] استخدام اسم الموديل من الإعدادات
            string logPrefix = $"    Attempt {attemptNum} [Gemini/{modelName}]:";
            _logger(fileLogPath, $"{logPrefix} Sending request...", false);
            string fullPrompt = string.Format(_promptTemplate, cleanText);

            _logger(fileLogPath, $"    --- Payload Sent (To {modelName}) --- \n{cleanText}\n    --- End Payload ---", false);

            var payload = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = fullPrompt }
                        }
                    }
                }
                // (إضافة إعدادات الأمان عشان نتجنب البلوك)
                ,
                safetySettings = new[] {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                }
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);

            string fullUrl = $"{TranslationSettings.GEMINI_API_ENDPOINT}?key={apiKey}";

            _httpClient.DefaultRequestHeaders.Clear();
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(fullUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (responseString.ToLower().Contains("quota")) return (string.Empty, "RATE_LIMIT", string.Empty);
                    // 🆕 [إضافة] معالجة خطأ المفتاح الغلط
                    if (responseString.ToLower().Contains("apikey")) return (string.Empty, "INVALID_KEY", string.Empty);
                    return ($"[TRANSLATION ERROR: HTTP {(int)response.StatusCode}]", "NETWORK_ERROR", string.Empty);
                }

                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseString);

                if (result?.Candidates == null || result.Candidates.Count == 0)
                {
                    if (result?.PromptFeedback?.BlockReason != null)
                    {
                        return ($"[TRANSLATION ERROR: Gemini blocked prompt. Reason: {result.PromptFeedback.BlockReason}]", "SAFETY_BLOCK", string.Empty);
                    }
                    return ("[TRANSLATION ERROR: No content found]", "NO_CONTENT", string.Empty);
                }

                string translation = result.Candidates.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "[TRANSLATION ERROR: No content found]";

                _logger(fileLogPath, $"    --- Raw Response Received (From {modelName}) --- \n{translation}\n    --- End Raw Response ---", false);

                string cleanedTranslation = CleanLlmResponse(translation);
                string successSource = $"Gemini/{modelName}";

                if (string.IsNullOrEmpty(cleanedTranslation) && !string.IsNullOrEmpty(translation) && !translation.StartsWith("[TRANSLATION ERROR")) return ("[TRANSLATION ERROR: Cleaned response was empty]", "CLEANING_ERROR", string.Empty);
                return (cleanedTranslation, string.Empty, successSource);
            }
            catch (TaskCanceledException) { return ("[TRANSLATION ERROR: Timeout]", "TIMEOUT", string.Empty); }
            catch (Exception ex)
            {
                if (TranslationSettings.RateLimitKeywords.Any(keyword => ex.Message.ToLower().Contains(keyword))) return (string.Empty, "RATE_LIMIT", string.Empty);
                _logger(fileLogPath, $"{logPrefix} FAILED (Unexpected): {ex.Message}", true);
                return ($"[TRANSLATION ERROR: Unexpected {ex.Message}]", "UNKNOWN_ERROR", string.Empty);
            }
        }

        private async Task<(string translation, string errorType, string successSource)> TranslateWithMistral(string cleanText, string modelName, string apiKey, int attemptNum, string keyLabel, string fileLogPath)
        {
            string logPrefix = $"    Attempt {attemptNum} [Mistral/{keyLabel}]:";
            _logger(fileLogPath, $"{logPrefix} Sending request...", false);
            string fullPrompt = string.Format(_promptTemplate, cleanText);

            _logger(fileLogPath, $"    --- Payload Sent (To {keyLabel}) --- \n{cleanText}\n    --- End Payload ---", false);

            var payload = new { model = modelName, messages = new[] { new { role = "user", content = fullPrompt } } };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(TranslationSettings.MISTRAL_API_ENDPOINT, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonConvert.DeserializeObject<MistralResponse>(responseString);
                    string errorMessage = errorResponse?.Error?.Message.ToLower() ?? response.ReasonPhrase?.ToLower() ?? "unknown error";
                    if (TranslationSettings.RateLimitKeywords.Any(keyword => errorMessage.Contains(keyword))) return (string.Empty, "RATE_LIMIT", string.Empty);
                    // 🆕 [إضافة] معالجة خطأ المفتاح الغلط
                    if (errorMessage.Contains("invalid api key") || errorMessage.Contains("forbidden")) return (string.Empty, "INVALID_KEY", string.Empty);
                    return ($"[TRANSLATION ERROR: HTTP {(int)response.StatusCode} / {errorMessage}]", "NETWORK_ERROR", string.Empty);
                }

                var result = JsonConvert.DeserializeObject<MistralResponse>(responseString);
                string translation = result?.Choices.FirstOrDefault()?.Message.Content ?? "[TRANSLATION ERROR: No content found]";

                _logger(fileLogPath, $"    --- Raw Response Received (From {keyLabel}) --- \n{translation}\n    --- End Raw Response ---", false);

                string cleanedTranslation = CleanLlmResponse(translation);
                string successSource = $"Mistral/{keyLabel}";

                if (string.IsNullOrEmpty(cleanedTranslation) && !string.IsNullOrEmpty(translation) && !translation.StartsWith("[TRANSLATION ERROR")) return ("[TRANSLATION ERROR: Cleaned response was empty]", "CLEANING_ERROR", string.Empty);
                return (cleanedTranslation, string.Empty, successSource);
            }
            catch (TaskCanceledException) { return ("[TRANSLATION ERROR: Timeout]", "TIMEOUT", string.Empty); }
            catch (Exception ex)
            {
                if (TranslationSettings.RateLimitKeywords.Any(keyword => ex.Message.ToLower().Contains(keyword))) return (string.Empty, "RATE_LIMIT", string.Empty);
                _logger(fileLogPath, $"{logPrefix} FAILED (Unexpected): {ex.Message}", true);
                return ($"[TRANSLATION ERROR: Unexpected {ex.Message}]", "UNKNOWN_ERROR", string.Empty);
            }
        }

        private async Task<(string translation, string errorType, string successSource)> TranslateWithOllama(string cleanText, string modelName, int attemptNum, string fileLogPath)
        {
            string logPrefix = $"    Attempt {attemptNum} [Ollama/{modelName}]:";
            _logger(fileLogPath, $"{logPrefix} Sending request...", false);
            string fullPrompt = string.Format(_promptTemplate, cleanText);

            _logger(fileLogPath, $"    --- Payload Sent (To {modelName}) --- \n{cleanText}\n    --- End Payload ---", false);

            var payload = new { model = modelName, prompt = fullPrompt, stream = false };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                HttpResponseMessage response = await _httpClient.PostAsync(TranslationSettings.OLLAMA_ENDPOINT, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = responseString.ToLower();
                    if (TranslationSettings.RateLimitKeywords.Any(keyword => errorMessage.Contains(keyword))) return (string.Empty, "RATE_LIMIT", string.Empty);
                    return ($"[TRANSLATION ERROR: HTTP {(int)response.StatusCode}]", "NETWORK_ERROR", string.Empty);
                }

                var result = JsonConvert.DeserializeObject<OllamaResponse>(responseString);
                string translation = result?.Response ?? "[TRANSLATION ERROR: No response field]";

                _logger(fileLogPath, $"    --- Raw Response Received (From {modelName}) --- \n{translation}\n    --- End Raw Response ---", false);

                string cleanedTranslation = CleanLlmResponse(translation);
                string successSource = $"Ollama/{modelName}";

                if (string.IsNullOrEmpty(cleanedTranslation) && !string.IsNullOrEmpty(translation) && !translation.StartsWith("[TRANSLATION ERROR")) return ("[TRANSLATION ERROR: Cleaned response was empty]", "CLEANING_ERROR", string.Empty);
                return (cleanedTranslation, string.Empty, successSource);
            }
            catch (TaskCanceledException) { return ("[TRANSLATION ERROR: Timeout]", "TIMEOUT", string.Empty); }
            catch (Exception ex)
            {
                if (TranslationSettings.RateLimitKeywords.Any(keyword => ex.Message.ToLower().Contains(keyword))) return (string.Empty, "RATE_LIMIT", string.Empty);
                _logger(fileLogPath, $"{logPrefix} FAILED (Unexpected): {ex.Message}", true);
                return ($"[TRANSLATION ERROR: Unexpected {ex.Message}]", "UNKNOWN_ERROR", string.Empty);
            }
        }

        // --- 4. استراتيجية الترجمة (هنا التعديل) ---
        private List<TranslationConfig> GetTranslationStrategy(bool isLongText)
        {
            var strategy = new List<TranslationConfig>();

            if (_settings.UseLocalOnly)
            {
                strategy.Add(new TranslationConfig { Model = _settings.LocalModelName, Api = "ollama", KeyLabel = "Local" });
                return strategy;
            }

            // --- تعريف الموديلات ---
            // 🆕 [تعديل] إضافة جيميني ريتراي
            var gemini = new TranslationConfig { Model = TranslationSettings.GEMINI_MODEL, Api = "gemini", KeyLabel = "Gemini", ApiKey = null };
            var geminiRetry = new TranslationConfig { Model = TranslationSettings.GEMINI_MODEL, Api = "gemini", KeyLabel = "Gemini (Retry)", ApiKey = null, SleepSeconds = 5 };

            // ميسترال (بدون ريتراي)
            var mistralPrimary = new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Primary", ApiKey = _settings.MistralKey1 };
            var mistralSecondary = new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Secondary", ApiKey = _settings.MistralKey2 };

            // أولاما
            var ollamaCloud = new TranslationConfig { Model = _settings.CloudModelName, Api = "ollama", KeyLabel = "Cloud", SleepSeconds = 1 };
            var ollamaLocal = new TranslationConfig { Model = _settings.LocalModelName, Api = "ollama", KeyLabel = "Local", SleepSeconds = 1 };

            // فحص المفاتيح
            bool geminiEnabled = !string.IsNullOrWhiteSpace(_settings.SelectedGeminiKey);
            bool mistralP1Enabled = !string.IsNullOrEmpty(mistralPrimary.ApiKey);
            bool mistralP2Enabled = !string.IsNullOrEmpty(mistralSecondary.ApiKey);

            if (isLongText)
            {
                // 1. جيميني + ريتراي
                if (geminiEnabled)
                {
                    strategy.Add(gemini);
                    strategy.Add(geminiRetry);
                }

                // 2. ميسترال (بدون ريتراي)
                if (mistralP1Enabled) strategy.Add(mistralPrimary);
                if (mistralP2Enabled) strategy.Add(mistralSecondary);

                // 3. أولاما
                strategy.Add(ollamaCloud);
                strategy.Add(ollamaLocal);
            }
            else // (نص قصير)
            {
                strategy.Add(ollamaLocal); // 1. المحلي أولاً

                // 2. جيميني + ريتراي
                if (geminiEnabled)
                {
                    strategy.Add(gemini);
                    strategy.Add(geminiRetry);
                }

                // 3. ميسترال (بدون ريتراي)
                if (mistralP1Enabled) strategy.Add(mistralPrimary);
                if (mistralP2Enabled) strategy.Add(mistralSecondary);

                strategy.Add(ollamaCloud); // 4. الكلاود الاحتياطي
            }
            return strategy;
        }


        // ---------------------------------------------------------------------
        //                                منطق التشغيل
        // ---------------------------------------------------------------------
        #region ProcessFiles
        public async Task ProcessFiles(CancellationToken cancellationToken)
        {
            #region File/Folder Setup (No Changes)
            string systemLogPath = Path.Combine(_settings.LogPath, "System_Log.txt");
            _logger(systemLogPath, $"بدء معالجة الملفات من: {_settings.InputPath}", true);

            DirectoryInfo inputDir = new DirectoryInfo(_settings.InputPath);
            if (!inputDir.Exists) { _logger(systemLogPath, "ERROR: مجلد المدخلات غير موجود. توقف.", true); return; }

            try { Directory.CreateDirectory(_settings.OutputPath); Directory.CreateDirectory(_settings.LogPath); }
            catch (Exception ex) { _logger(systemLogPath, $"ERROR: فشل إنشاء مجلدات المخرجات/اللوج: {ex.Message}", true); return; }

            var allJsonFiles = inputDir.GetFiles("*.json").OrderBy(f => f.Name).ToList();
            int totalFiles = allJsonFiles.Count;
            List<FileInfo> filesToProcess = allJsonFiles;

            if (_settings.StartIndex.HasValue || _settings.EndIndex.HasValue)
            {
                int startIndex = _settings.StartIndex.GetValueOrDefault(1);
                int endIndex = _settings.EndIndex.GetValueOrDefault(totalFiles);
                int startZeroBased = Math.Max(0, startIndex - 1);
                int endZeroBased = Math.Min(totalFiles - 1, endIndex - 1);

                if (startZeroBased > endZeroBased) { _logger(systemLogPath, $"ERROR: نطاق المعالجة غير صحيح ({startIndex} > {endIndex}). توقف.", true); return; }
                int count = endZeroBased - startZeroBased + 1;
                filesToProcess = allJsonFiles.GetRange(startZeroBased, count);
                _logger(systemLogPath, $"تم تطبيق نطاق المعالجة. سيبدأ من الملف رقم {startIndex} وينتهي عند الملف رقم {endIndex} ({filesToProcess.Count} ملف).", true);
            }

            int processedFilesCount = 0;
            int skippedFilesCount = 0;
            #endregion

            foreach (var inputFile in filesToProcess)
            {
                #region Per-File Setup (No Changes)
                if (cancellationToken.IsCancellationRequested) { cancellationToken.ThrowIfCancellationRequested(); }

                int fileIndexTotal = allJsonFiles.IndexOf(inputFile) + 1;
                string outputFilePath = Path.Combine(_settings.OutputPath, inputFile.Name);
                string logFilePath = Path.Combine(_settings.LogPath, Path.GetFileNameWithoutExtension(inputFile.Name) + "_log.txt");

                _logger(logFilePath, $"\n--- معالجة الملف {fileIndexTotal}/{totalFiles}: {inputFile.Name} ---", true);

                if (File.Exists(outputFilePath)) { skippedFilesCount++; continue; }

                Dictionary<string, string> originalEntries;
                try
                {
                    string jsonString = File.ReadAllText(inputFile.FullName, Encoding.UTF8);
                    originalEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                }
                catch (Exception e) { _logger(logFilePath, $"  ERROR: فشل قراءة JSON من {inputFile.Name}. التفاصيل: {e.Message}", true); continue; }

                Dictionary<string, string> translatedEntries = new Dictionary<string, string>();
                int totalEntries = originalEntries.Count;
                int processedEntriesFile = 0;
                #endregion

                // --- بداية اللوب على السطور ---
                foreach (var entry in originalEntries)
                {
                    if (cancellationToken.IsCancellationRequested) { cancellationToken.ThrowIfCancellationRequested(); }

                    processedEntriesFile++;

                    string label = entry.Key;
                    string englishTextWithPh = entry.Value;
                    string textToSendToAPI = englishTextWithPh;

                    var regex = new Regex(@"(\[[^\]]+\])");
                    string[] originalParts = null;
                    List<string> textSlices = null;

                    if (_settings.PreserveTags)
                    {
                        originalParts = regex.Split(englishTextWithPh);
                        textSlices = originalParts.Where(p => !regex.IsMatch(p) && !string.IsNullOrWhiteSpace(p)).ToList();

                        if (textSlices.Count <= 1)
                        {
                            textToSendToAPI = englishTextWithPh;
                            textSlices = null;
                        }
                        else
                        {
                            textToSendToAPI = string.Join("\n", textSlices);
                            _logger(logFilePath, $"    -> Sliced text into {textSlices.Count} segments.", false);
                        }
                    }


                    int wordCount = textToSendToAPI.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    string successfulSource = "[FAILED]";
                    _logger(logFilePath, $"    -> Processing Key {processedEntriesFile}/{totalEntries} ({wordCount} words): {label}", true);

                    string finalPureArabicTranslation = string.Empty;

                    if (string.IsNullOrWhiteSpace(englishTextWithPh)) { finalPureArabicTranslation = string.Empty; }
                    else
                    {
                        bool isLongText = wordCount > TranslationSettings.WORD_COUNT_THRESHOLD;
                        var modelsToTry = GetTranslationStrategy(isLongText);
                        string cleanArabicTranslationAttempt = string.Empty;
                        string lastErrorType = string.Empty;
                        bool translationApiSuccess = false;

                        // --- بداية اللوب اللانهائي ---
                        while (!translationApiSuccess)
                        {
                            if (cancellationToken.IsCancellationRequested) { cancellationToken.ThrowIfCancellationRequested(); }

                            int mainAttemptNum = 1;

                            // --- بداية لوب المحاولات (الموديلات) ---
                            foreach (var config in modelsToTry)
                            {
                                if (translationApiSuccess) break;
                                if (cancellationToken.IsCancellationRequested) { cancellationToken.ThrowIfCancellationRequested(); }

                                if (config.SleepSeconds > 0)
                                {
                                    try { await Task.Delay(config.SleepSeconds * 1000, cancellationToken); }
                                    catch (TaskCanceledException) { cancellationToken.ThrowIfCancellationRequested(); }
                                }

                                string currentModel = config.Model;
                                string currentApi = config.Api;
                                string keyLabel = config.KeyLabel;
                                string apiKey = config.ApiKey ?? string.Empty;

                                // 🆕 [تعديل] Switch Expression محدث
                                (string translationResult, string errorType, string source) = config.Api switch
                                {
                                    "mistral" => await TranslateWithMistral(textToSendToAPI, currentModel, apiKey, mainAttemptNum, keyLabel, logFilePath),
                                    "ollama" => await TranslateWithOllama(textToSendToAPI, currentModel, mainAttemptNum, logFilePath),
                                    // (مبقاش محتاج يبعت المفتاح)
                                    "gemini" => await TranslateWithGemini(textToSendToAPI, currentModel, mainAttemptNum, logFilePath),
                                    _ => (string.Empty, "UNKNOWN_API", string.Empty)
                                };

                                lastErrorType = errorType;

                                if (string.IsNullOrEmpty(errorType))
                                {
                                    bool validationPassed = true;

                                    if (_settings.PreserveTags && textSlices != null)
                                    {
                                        var translatedSlices = translationResult.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                        if (translatedSlices.Length != textSlices.Count)
                                        {
                                            validationPassed = false;
                                            lastErrorType = "LINE_COUNT_MISMATCH";
                                            _logger(logFilePath, $"    -> WARNING: Model {source} failed SLICE validation. Expected {textSlices.Count} lines, Got {translatedSlices.Length} (after filtering empty). Retrying...", true);
                                        }
                                        else
                                        {
                                            var result = new StringBuilder();
                                            int sliceIndex = 0;
                                            foreach (var part in originalParts)
                                            {
                                                if (regex.IsMatch(part))
                                                {
                                                    result.Append(part);
                                                }
                                                else if (!string.IsNullOrWhiteSpace(part))
                                                {
                                                    result.Append(translatedSlices[sliceIndex++]);
                                                }
                                            }
                                            cleanArabicTranslationAttempt = result.ToString();
                                        }
                                    }
                                    else
                                    {
                                        cleanArabicTranslationAttempt = translationResult;
                                    }

                                    if (validationPassed)
                                    {
                                        translationApiSuccess = true;
                                        successfulSource = source;
                                        break;
                                    }
                                }
                                else
                                {
                                    _logger(logFilePath, $"    -> INFO: Model {config.KeyLabel} (or {config.Model}) failed API call. Error: {lastErrorType}. Retrying...", true);
                                }

                                mainAttemptNum++;
                            } // --- نهاية لوب المحاولات (الموديلات) ---

                            if (!translationApiSuccess)
                            {
                                _logger(logFilePath, $"    -> ALL ATTEMPTS FAILED. Last error: {lastErrorType ?? "UNKNOWN"}. Restarting retry loop immediately...", true);
                            }

                        } // --- نهاية اللوب اللانهائي ---

                        finalPureArabicTranslation = cleanArabicTranslationAttempt;
                    }

                    string statusSuffix = $" ({successfulSource})";
                    _logger(logFilePath, $"    -> Result:{statusSuffix}", true);

                    translatedEntries[label] = finalPureArabicTranslation;
                }
                // --- نهاية اللوب على السطور ---

                #region File Saving (No Changes)
                try
                {
                    string finalJson = JsonConvert.SerializeObject(translatedEntries, Formatting.Indented);
                    File.WriteAllText(outputFilePath, finalJson, Encoding.UTF8);
                    _logger(logFilePath, $"SUCCESS: تم حفظ ملف JSON المترجم إلى {outputFilePath}.", true);
                    processedFilesCount++;
                }
                catch (Exception e) { _logger(logFilePath, $"ERROR: فشل حفظ ملف JSON المترجم {outputFilePath}. التفاصيل: {e.Message}", true); }
                #endregion
            }

            // ... (الملخص النهائي) ...
            #region Final Summary (No Changes)
            int totalProcessedOrSkipped = processedFilesCount + skippedFilesCount;
            int failedFiles = totalFiles - totalProcessedOrSkipped;

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger(systemLogPath, $"\nملخص العملية:", true);
                _logger(systemLogPath, $"   إجمالي ملفات JSON: {totalFiles}", true);
                _logger(systemLogPath, $"   الملفات التي تمت معالجتها بنجاح: {processedFilesCount}", true);
                _logger(systemLogPath, $"   الملفات التي تم تخطيها (موجودة): {skippedFilesCount}", true);
                _logger(systemLogPath, $"   الملفات التي فشلت (قراءة/كتابة): {failedFiles}", true);
            }
            else
            {
                _logger(systemLogPath, $"تم إيقاف العملية يدوياً. عدد الملفات المكتملة حتى الآن: {processedFilesCount}", true);
            }
            #endregion
        }
        #endregion
    }
}