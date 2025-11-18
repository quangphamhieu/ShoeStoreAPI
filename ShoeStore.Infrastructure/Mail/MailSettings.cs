namespace ShoeStore.Infrastructure.Mail
{
    public class MailSettings
    {
        public string DisplayName { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Mail { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Port { get; set; }
    }
}
