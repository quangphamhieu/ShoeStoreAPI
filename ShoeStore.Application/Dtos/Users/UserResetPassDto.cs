using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.DTOs.Users
{
    public class UserResetPassDto
    {
        public string PhoneOrEmail { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string? OldPassword { get; set; } // dùng khi người dùng tự đổi
        public string? OtpCode { get; set; }
    }
}
