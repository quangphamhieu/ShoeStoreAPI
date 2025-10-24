namespace ShoeStore.Application.Dtos.Comment
{
    public class UpdateCommentDto
    {
        public string Content { get; set; } = null!;
        public byte? Rating { get; set; }
    }
}
