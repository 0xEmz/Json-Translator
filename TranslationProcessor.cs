// TranslationProcessor.cs - (تعديل) تم إلغاء نظام البليس هولدر

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

            // إزالة علامات التنصيص المحيطة
            if (responseText.StartsWith('"') && responseText.EndsWith('"') && responseText.Length > 1) responseText = responseText.Substring(1, responseText.Length - 2);
            else if (responseText.StartsWith("'") && responseText.EndsWith("'") && responseText.Length > 1) responseText = responseText.Substring(1, responseText.Length - 2);

            // 🆕 تنظيف المسافات الأفقية فقط، بدون المساس بـ \n
            responseText = Regex.Replace(responseText, @"[ \t]+", " ");

            return responseText.Trim();
        }

        // --- 2. الـ Data Models (تبقى كما هي) ---
        private class MistralMessage { [JsonProperty("content")] public string Content { get; set; } = string.Empty; }
        private class MistralChoice { [JsonProperty("message")] public MistralMessage Message { get; set; } = new MistralMessage(); }
        private class MistralError { [JsonProperty("message")] public string Message { get; set; } = string.Empty; }
        private class MistralResponse { [JsonProperty("choices")] public List<MistralChoice> Choices { get; set; } = new List<MistralChoice>(); [JsonProperty("error")] public MistralError? Error { get; set; } }
        private class OllamaResponse { [JsonProperty("response")] public string Response { get; set; } = string.Empty; }
        private class TranslationConfig
        {
            public string Model { get; set; } = string.Empty;
            public string Api { get; set; } = string.Empty;
            public string KeyLabel { get; set; } = string.Empty;
            public string? ApiKey { get; set; }
            public int SleepSeconds { get; set; } = 1;
        }

        // --- 3. دوال API الفعلية (تبقى كما هي) ---
        private async Task<(string translation, string errorType, string successSource)> TranslateWithMistral(string cleanText, string modelName, string apiKey, int attemptNum, string keyLabel, string fileLogPath)
        {
            string logPrefix = $"    Attempt {attemptNum} [Mistral/{keyLabel}]:";
            _logger(fileLogPath, $"{logPrefix} Sending request...", false);
            string fullPrompt = string.Format(_promptTemplate, cleanText);
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
                    return ($"[TRANSLATION ERROR: HTTP {(int)response.StatusCode} / {errorMessage}]", "NETWORK_ERROR", string.Empty);
                }

                var result = JsonConvert.DeserializeObject<MistralResponse>(responseString);
                string translation = result?.Choices.FirstOrDefault()?.Message.Content ?? "[TRANSLATION ERROR: No content found]";

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

        // --- 4. استراتيجية الترجمة (تبقى كما هي) ---
        private List<TranslationConfig> GetTranslationStrategy(bool isLongText)
        {
            var strategy = new List<TranslationConfig>();

            if (_settings.UseLocalOnly)
            {
                strategy.Add(new TranslationConfig { Model = TranslationSettings.LOCAL_MODEL, Api = "ollama", KeyLabel = "Local" });
                return strategy;
            }

            var mistralPrimary = new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Primary", ApiKey = _settings.MistralKey1 };
            var mistralSecondary = new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Secondary", ApiKey = _settings.MistralKey2 };

            if (isLongText)
            {
                strategy.Add(mistralPrimary);
                if (!string.IsNullOrEmpty(_settings.MistralKey2)) strategy.Add(mistralSecondary);

                if (!string.IsNullOrEmpty(_settings.MistralKey2))
                {
                    strategy.Add(new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Primary (Retry)", ApiKey = _settings.MistralKey1, SleepSeconds = 5 });
                    strategy.Add(new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Secondary (Retry)", ApiKey = _settings.MistralKey2, SleepSeconds = 1 });
                }
                else
                {
                    strategy.Add(new TranslationConfig { Model = TranslationSettings.MISTRAL_MODEL, Api = "mistral", KeyLabel = "Primary (Retry)", ApiKey = _settings.MistralKey1, SleepSeconds = 5 });
                }

                strategy.Add(new TranslationConfig { Model = TranslationSettings.SECONDARY_CLOUD_MODEL, Api = "ollama", KeyLabel = "Cloud", SleepSeconds = 1 });
                strategy.Add(new TranslationConfig { Model = TranslationSettings.LOCAL_MODEL, Api = "ollama", KeyLabel = "Local", SleepSeconds = 1 });
            }
            else
            {
                strategy.Add(new TranslationConfig { Model = TranslationSettings.LOCAL_MODEL, Api = "ollama", KeyLabel = "Local" });
            }
            return strategy;
        }


        // ---------------------------------------------------------------------
        //                                منطق التشغيل
        // ---------------------------------------------------------------------

        public async Task ProcessFiles(CancellationToken cancellationToken)
        {
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

            foreach (var inputFile in filesToProcess)
            {
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

                foreach (var entry in originalEntries)
                {
                    if (cancellationToken.IsCancellationRequested) { cancellationToken.ThrowIfCancellationRequested(); }

                    processedEntriesFile++;

                    string label = entry.Key;
                    string englishTextWithPh = entry.Value;

                    // 🆕 1. تم إلغاء منطق الترميز (Tokenization)
                    // النص يُرسل كما هو
                    string textToSendToAPI = englishTextWithPh;

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
                        int mainAttemptNum = 1;

                        foreach (var config in modelsToTry)
                        {
                            if (translationApiSuccess) break;
                            if (config.SleepSeconds > 0)
                            {
                                try { await Task.Delay(config.SleepSeconds * 1000, cancellationToken); }
                                catch (TaskCanceledException) { cancellationToken.ThrowIfCancellationRequested(); }
                            }

                            string currentModel = config.Model;
                            string currentApi = config.Api;
                            string keyLabel = config.KeyLabel;
                            string apiKey = config.ApiKey ?? string.Empty;

                            (string translationResult, string errorType, string source) = await (
                                currentApi == "mistral"
                                ? TranslateWithMistral(textToSendToAPI, currentModel, apiKey, mainAttemptNum, keyLabel, logFilePath)
                                : TranslateWithOllama(textToSendToAPI, currentModel, mainAttemptNum, logFilePath));

                            lastErrorType = errorType;

                            if (string.IsNullOrEmpty(errorType))
                            {
                                cleanArabicTranslationAttempt = translationResult;
                                translationApiSuccess = true;
                                successfulSource = source;
                                break;
                            }
                            mainAttemptNum++;
                        }

                        if (!translationApiSuccess && string.IsNullOrEmpty(cleanArabicTranslationAttempt)) cleanArabicTranslationAttempt = $"[TRANSLATION FAILED: {lastErrorType ?? "UNKNOWN"}]";
                        finalPureArabicTranslation = cleanArabicTranslationAttempt;
                    }

                    // 🆕 2. تم إلغاء منطق فك الترميز (Detokenization)

                    string statusSuffix = successfulSource != "[FAILED]" ? $" ({successfulSource})" : " [FAILED]";
                    _logger(logFilePath, $"    -> Result: {statusSuffix}", true);

                    translatedEntries[label] = finalPureArabicTranslation;
                }

                try
                {
                    string finalJson = JsonConvert.SerializeObject(translatedEntries, Formatting.Indented);
                    File.WriteAllText(outputFilePath, finalJson, Encoding.UTF8);
                    _logger(logFilePath, $"SUCCESS: تم حفظ ملف JSON المترجم إلى {outputFilePath}.", true);
                    processedFilesCount++;
                }
                catch (Exception e) { _logger(logFilePath, $"ERROR: فشل حفظ ملف JSON المترجم {outputFilePath}. التفاصيل: {e.Message}", true); }
            }

            // الملخص النهائي
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
        }
    }
}