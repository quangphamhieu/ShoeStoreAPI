namespace ShoeStore.Application.Dtos.Notification
{
    public class CreateNotificationDto
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Type { get; set; }
    }
}
