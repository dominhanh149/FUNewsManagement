namespace AI_API.Models
{
    public class SuggestTagsRequest
    {
        /// <summary>Nội dung bài viết cần gợi ý tag</summary>
        public string Content { get; set; } = string.Empty;
    }

    public class LearnTagsRequest
    {
        /// <summary>Danh sách tag user đã chọn (để cập nhật Learning Cache)</summary>
        public List<string> SelectedTags { get; set; } = new();
    }
}
