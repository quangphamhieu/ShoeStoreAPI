using ShoeStore.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(long id);

        Task<UserDto> CreateAsync(UserCreateDto dto);

        Task<UserDto?> UpdateAsync(UserUpdateDto dto);

        Task<bool> DeleteAsync(long id);

        Task<UserLoginResponseDto?> LoginAsync(UserLoginDto dto);

        Task<UserDto> SignupAsync(UserSignUpDto dto);

        Task<bool> ResetPasswordAsync(UserResetPassDto dto);
    }
}