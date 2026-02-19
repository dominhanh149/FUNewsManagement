using System.Collections.Concurrent;

namespace AI_API.Services
{
    /// <summary>
    /// Learning Cache: ghi nhớ các tag được user chọn nhiều lần.
    /// Singleton – tồn tại suốt vòng đời ứng dụng (in-memory).
    /// </summary>
    public class LearningCacheService
    {
        // tag (lowercase) → số lần được chọn
        private readonly ConcurrentDictionary<string, int> _frequency = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Ghi nhận các tag user đã chọn từ gợi ý.</summary>
        public void RecordSelection(IEnumerable<string> selectedTags)
        {
            foreach (var tag in selectedTags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var key = tag.Trim().ToLowerInvariant();
                _frequency.AddOrUpdate(key, 1, (_, old) => old + 1);
            }
        }

        /// <summary>
        /// Trả về boost multiplier cho tag.
        /// Tag chưa được học → 1.0
        /// Tag được chọn nhiều → lên đến 2.0
        /// </summary>
        public double GetBoost(string tag)
        {
            var key = tag.Trim().ToLowerInvariant();
            if (!_frequency.TryGetValue(key, out var count)) return 1.0;

            // Mỗi 5 lần chọn thêm 0.2, tối đa 2.0
            var boost = 1.0 + Math.Min(count / 5.0, 1.0) * 1.0;
            return Math.Min(boost, 2.0);
        }

        /// <summary>Trả toàn bộ cache để debug / API trả về.</summary>
        public IReadOnlyDictionary<string, int> GetAll() => _frequency;
    }
}
