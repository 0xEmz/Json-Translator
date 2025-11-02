// TranslationSettings.cs
using System;
using System.Collections.Generic;

public class TranslationSettings
{
    // مسارات العمل
    public string InputPath { get; set; }
    public string OutputPath { get; set; }
    public string LogPath { get; set; }

    // نطاق المعالجة (من الـ ComboBox)
    public int? StartIndex { get; set; }
    public int? EndIndex { get; set; }

    // مفاتيح API (سيتم تعيينها قبل بدء كل عملية)
    public string MistralKey1 { get; set; }
    public string MistralKey2 { get; set; }

    // الخاصية الناقصة التي سببت الخطأ 
    public string PromptTemplate { get; set; }

    // 🆕 خاصية جديدة: استخدام الموديل المحلي فقط
    public bool UseLocalOnly { get; set; }

    // 🆕 الخاصية الجديدة لتفعيل الحفاظ على التاجز
    public bool PreserveTags { get; set; }

    // قائمة بجميع المفاتيح المُستوردة من الملف (Pool)
    public static List<string> AllMistralKeys { get; set; } = new List<string>();

    // الثوابت الثابتة
    public const string OLLAMA_ENDPOINT = "http://localhost:11434/api/generate";
    public const string LOCAL_MODEL = "gemma3:12b";
    //public const string LOCAL_MODEL = "gpt-oss:120b-cloud";
    public const string MISTRAL_API_ENDPOINT = "https://api.mistral.ai/v1/chat/completions";
    public const string MISTRAL_MODEL = "mistral-large-latest";
    public const string SECONDARY_CLOUD_MODEL = "gpt-oss:120b-cloud";

    public const int WORD_COUNT_THRESHOLD = 10;
    public const int REQUEST_TIMEOUT_SECONDS = 90;

    public static readonly List<string> RateLimitKeywords = new List<string>
    {
        "limit", "exceeded", "rate", "hourly", "hour", "please", "quota"
    };
}