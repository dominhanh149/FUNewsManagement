using AI_API.Models;

namespace AI_API.Services
{
    public interface ITagExtractorService
    {
        /// <summary>
        /// Trích xuất và gợi ý tags từ nội dung bài viết.
        /// </summary>
        List<TagSuggestion> ExtractTags(string content, int topN = 10);
    }
}
