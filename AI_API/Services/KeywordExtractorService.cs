using AI_API.Models;
using System.Text.RegularExpressions;

namespace AI_API.Services
{
    /// <summary>
    /// Trích xuất keyword từ nội dung bài viết và gợi ý tags.
    /// Hỗ trợ tiếng Việt + tiếng Anh, không cần OpenAI.
    /// </summary>
    public class KeywordExtractorService : ITagExtractorService
    {
        private readonly LearningCacheService _cache;

        // ── Stopwords tiếng Việt ─────────────────────────────────────────────────
        private static readonly HashSet<string> ViStopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "và","của","là","trong","với","các","có","được","này","đó","để","khi","từ",
            "một","những","như","về","theo","sẽ","đã","đang","vào","ra","lên","xuống",
            "không","tại","sau","trước","hay","hoặc","nên","mà","nếu","thì","vì","do",
            "bởi","qua","cho","đến","rằng","bằng","cũng","chỉ","nào","nhưng","còn",
            "hơn","nhất","rất","quá","lại","thêm","đây","đó","người","nhiều","ít","hết",
            "thế","vậy","chúng","họ","bạn","tôi","mình","ta","chúng ta","anh","chị",
            "em","ông","bà","cô","thầy","giáo","trường","lớp","học","sinh","viên",
            "năm","ngày","tháng","giờ","phút","giây","tuần","thứ","tổ","và","trên","dưới"
        };

        // ── Stopwords tiếng Anh ──────────────────────────────────────────────────
        private static readonly HashSet<string> EnStopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the","a","an","and","or","but","in","on","at","to","for","of","with",
            "is","are","was","were","be","been","being","have","has","had","do","does",
            "did","will","would","could","should","may","might","shall","can","need",
            "this","that","these","those","it","its","he","she","they","we","i","you",
            "his","her","their","our","my","your","its","from","by","about","as","into",
            "through","during","before","after","above","below","between","each","more",
            "most","other","some","such","no","nor","not","only","same","so","than",
            "too","very","just","over","also","back","much","well","still","then",
            "there","here","where","when","how","what","who","which","all","any",
            "both","each","few","if","while","although","because","since","unless"
        };

        private static bool IsStopword(string word) =>
            ViStopwords.Contains(word) || EnStopwords.Contains(word);

        public KeywordExtractorService(LearningCacheService cache)
        {
            _cache = cache;
        }

        public List<TagSuggestion> ExtractTags(string content, int topN = 10)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<TagSuggestion>();

            // 1. Tokenize: lowercase, giữ chữ cái + số, split whitespace/punct
            var tokens = Tokenize(content);

            // 2. Đếm tần suất (TF)
            var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in tokens)
            {
                if (IsStopword(token)) continue;
                if (token.Length < 2) continue;       // bỏ ký tự đơn

                freq[token] = freq.TryGetValue(token, out var c) ? c + 1 : 1;
            }

            if (freq.Count == 0) return new List<TagSuggestion>();

            // 3. Tính raw score = TF, boost nếu có trong Learning Cache
            var maxFreq = (double)freq.Values.Max();

            var suggestions = freq.Select(kv =>
            {
                var tf = kv.Value / maxFreq;                      // normalize 0–1
                var boost = _cache.GetBoost(kv.Key);              // 1.0 – 2.0
                var confidence = Math.Min(tf * boost, 1.0);

                return new TagSuggestion
                {
                    Tag = Capitalize(kv.Key),
                    Confidence = Math.Round(confidence, 2)
                };
            })
            .OrderByDescending(s => s.Confidence)
            .ThenBy(s => s.Tag)
            .Take(topN)
            .ToList();

            return suggestions;
        }

        // ── Tokenize ─────────────────────────────────────────────────────────────
        private static List<string> Tokenize(string text)
        {
            // Chuẩn hóa: lowercase
            text = text.ToLowerInvariant();

            // Remove URLs
            text = Regex.Replace(text, @"https?://\S+", " ");

            // Remove HTML tags
            text = Regex.Replace(text, @"<[^>]+>", " ");

            // Giữ chữ cái (cả Unicode tiếng Việt), số, khoảng trắng; bỏ punct
            // Dùng \w vì .NET \w hỗ trợ Unicode (bao gồm tiếng Việt có dấu)
            var tokens = Regex.Matches(text, @"[\p{L}\p{N}]+")
                              .Cast<Match>()
                              .Select(m => m.Value)
                              .ToList();

            return tokens;
        }

        private static string Capitalize(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
    }
}
