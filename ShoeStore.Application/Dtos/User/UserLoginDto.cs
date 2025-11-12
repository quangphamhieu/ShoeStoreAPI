namespace ShoeStore.Application.DTOs.Users
{
    public class UserLoginDto
    {
        public string PhoneOrEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    public class UserLoginResponseDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}