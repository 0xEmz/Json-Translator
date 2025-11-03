// PersistentSettings.cs (ملف جديد)
using System;
using System.IO;
using Newtonsoft.Json;

namespace WinFormsApp1
{
    /// <summary>
    /// كلاس بسيط يحتوي فقط على الإعدادات التي نريد حفظها واسترجاعها
    /// </summary>
    public class PersistentSettings
    {
        // --- المسارات ---
        public string InputPath { get; set; } = "";
        public string OutputPath { get; set; } = "";
        public string LogPath { get; set; } = "";
        public string KeysFilePath { get; set; } = "";
        public string PromptFilePath { get; set; } = "";

        // --- أسماء الموديلات ---
        public string LocalModelName { get; set; } = "gemma3:12b";
        public string CloudModelName { get; set; } = "gpt-oss:120b-cloud";
    }

    /// <summary>
    /// كلاس مساعد للتعامل مع حفظ وتحميل الإعدادات
    /// </summary>
    public static class SettingsManager
    {
        private static readonly string SettingsFileName = "translator_settings.json";
        private static string GetSettingsFilePath()
        {
            // نحفظ الملف بجوار ملف التشغيل (exe)
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
        }

        public static void SaveSettings(PersistentSettings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(GetSettingsFilePath(), json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static PersistentSettings LoadSettings()
        {
            string filePath = GetSettingsFilePath();
            if (!File.Exists(filePath))
            {
                return new PersistentSettings(); // رجع الإعدادات الافتراضية لو الملف مش موجود
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<PersistentSettings>(json);
                return settings ?? new PersistentSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}\nLoading default settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return new PersistentSettings(); // رجع الافتراضي لو حصل خطأ في القراية
            }
        }
    }
}