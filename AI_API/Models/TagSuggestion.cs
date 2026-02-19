namespace AI_API.Models
{
    public class TagSuggestion
    {
        /// <summary>Tên tag được gợi ý</summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>Mức độ tự tin: 0.0 – 1.0</summary>
        public double Confidence { get; set; }
    }

    public class SuggestTagsResponse
    {
        public List<TagSuggestion> Tags { get; set; } = new();
        public bool FromCache { get; set; }
    }
}
