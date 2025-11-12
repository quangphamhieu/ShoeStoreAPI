using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.DTOs.Users;
using ShoeStore.Application.Interfaces;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;
using ShoeStore.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ShoeStoreDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly PasswordHelper _passwordHelper;
        public UserService(ShoeStoreDbContext context, JwtTokenGenerator jwtTokenGenerator, PasswordHelper passwordHelper)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHelper = passwordHelper;
        }
        public async Task<UserDto> CreateAsync(UserCreateDto dto)
        {
            var user = new User
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                Gender = dto.Gender,
                PasswordHash = _passwordHelper.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Gender = user.Gender
            };
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Email = u.Email,
                    Gender = u.Gender,
                    RoleName = u.Role.Name,
                    StatusName = u.Status.Name,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserDto?> GetByIdAsync(long id)
        {
            var u = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Phone = u.Phone,
                Email = u.Email,
                Gender = u.Gender,
                RoleName = u.Role.Name,
                StatusName = u.Status.Name,
                CreatedAt = u.CreatedAt
            };
        }

        public async Task<UserLoginResponseDto?> LoginAsync(UserLoginDto dto)
        {
            var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Phone == dto.PhoneOrEmail || u.Email == dto.PhoneOrEmail);

            if (user == null)
                return null;

            if (!_passwordHelper.VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.FullName, user.Role.Name);

            return new UserLoginResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                RoleName = user.Role.Name,
                Token = token
            };
        }

        public async Task<bool> ResetPasswordAsync(UserResetPassDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == dto.PhoneOrEmail || u.Email == dto.PhoneOrEmail);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");

            // Nếu yêu cầu đổi mật khẩu cũ → mới:
            if (!_passwordHelper.VerifyPassword(dto.OldPassword, user.PasswordHash))
                throw new Exception("Mật khẩu cũ không chính xác.");

            user.PasswordHash = _passwordHelper.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UserDto> SignupAsync(UserSignUpDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Phone == dto.Phone || u.Email == dto.Email))
                throw new Exception("Số điện thoại đã tồn tại.");

            var user = new User
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                Gender = dto.Gender,
                PasswordHash = _passwordHelper.HashPassword(dto.Password),
                RoleId = 4, // user thường
                StatusId = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email
            };
        }

        public async Task<UserDto?> UpdateAsync(UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.Gender = dto.Gender;
            user.RoleId = dto.RoleId;
            user.StatusId = dto.StatusId;

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Gender = user.Gender
            };
        }
    }
}