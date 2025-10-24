namespace ShoeStore.Application.Dtos.Comment
{
    public class CreateCommentDto
    {
        public long UserId { get; set; }
        public int ProductId { get; set; }
        public string Content { get; set; } = null!;
        public byte? Rating { get; set; }
    }
}
