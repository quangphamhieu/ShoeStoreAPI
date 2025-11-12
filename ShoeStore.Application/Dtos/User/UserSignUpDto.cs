using ShoeStore.Domain.Entities;

namespace ShoeStore.Application.DTOs.Users
{
    public class UserSignUpDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public Gender Gender { get; set; }
    }
}