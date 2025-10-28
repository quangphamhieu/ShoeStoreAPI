using ShoeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.DTOs.Users
{
    public class UserDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public Gender Gender { get; set; }
        public string RoleName { get; set; } = null!;
        public string StatusName { get; set; } = null!;
        public int? StoreId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
