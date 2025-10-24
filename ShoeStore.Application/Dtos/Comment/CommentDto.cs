namespace ShoeStore.Application.Dtos.Comment
{
    public class CommentDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public byte? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
