namespace Assignmen_PRN232__.Dto.Common
{
    public class ErrorDto
    {
        public string Code { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Details { get; set; }
    }
}
