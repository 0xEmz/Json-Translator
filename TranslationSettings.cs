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

    // 🆕 [إضافة] مفتاح جيميني (اللي اختاره الفورم للشغلانة دي)
    public string SelectedGeminiKey { get; set; }

    // الخاصية الناقصة التي سببت الخطأ 
    public string PromptTemplate { get; set; }

    // خاصية جديدة: استخدام الموديل المحلي فقط
    public bool UseLocalOnly { get; set; }

    // الخاصية الجديدة لتفعيل الحفاظ على التاجز
    public bool PreserveTags { get; set; }

    // خصائص لأسماء موديلات Ollama
    public string LocalModelName { get; set; }
    public string CloudModelName { get; set; }

    // 🆕 [تم الإلغاء] المفتاح الثابت بتاع جيميني اتشال
    // public static string GeminiApiKey { get; set; } = "YOUR_GEMINI_API_KEY_HERE";

    // --- مجمعات المفاتيح الثابتة (Static Pools) ---

    // 🆕 [إضافة] قايمة جديدة لمفاتيح جيميني (هتتملي من هنا مانيوال)
    public static List<string> AllGeminiKeys { get; set; } = new List<string>
    {
        "AIzaSyAaMz0rlcL5Pj1fPJzd2gsNbWYGBXtVnoQ",
        "AIzaSyBEPB9v5CE7mo5qvGvad090QJsOOx3hws0",
        "AIzaSyB04sFdcomj8XLDL7wknrlhREyXu1bdl_Q",
        "AIzaSyDNuF6FU4p5xOtgezBXR0NHUicfP8KHrPQ"
        // (تقدر تضيف أكتر لو حبيت)
    };

    // قائمة بجميع المفاتيح المُستوردة من الملف (Pool)
    public static List<string> AllMistralKeys { get; set; } = new List<string>();

    // الثوابت الثابتة
    public const string OLLAMA_ENDPOINT = "http://localhost:11434/api/generate";
    public const string MISTRAL_API_ENDPOINT = "https://api.mistral.ai/v1/chat/completions";
    public const string MISTRAL_MODEL = "mistral-large-latest";

    // ثوابت جيميني
    public const string GEMINI_API_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent";
    public const string GEMINI_MODEL = "gemini-2.5-flash-lite";

    public const int WORD_COUNT_THRESHOLD = 10;
    public const int REQUEST_TIMEOUT_SECONDS = 90;

    public static readonly List<string> RateLimitKeywords = new List<string>
    {
        "limit", "exceeded", "rate", "hourly", "hour", "please", "quota"
    };
}