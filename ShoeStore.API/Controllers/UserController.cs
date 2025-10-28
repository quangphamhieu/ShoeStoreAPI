using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.DTOs.Users;
using ShoeStore.Application.Interfaces;

namespace ShoeStore.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // -------------------- GET ALL --------------------
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // -------------------- GET BY ID --------------------
        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");
            return Ok(user);
        }

        // -------------------- CREATE --------------------
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // -------------------- UPDATE --------------------
        [HttpPut("{id:long}")]
        [Authorize]
        public async Task<IActionResult> Update(long id, [FromBody] UserUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id không khớp.");

            var result = await _userService.UpdateAsync(dto);
            return Ok(result);
        }

        // -------------------- DELETE --------------------
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
                return NotFound("Không tìm thấy người dùng.");
            return Ok("Đã xóa người dùng thành công.");
        }

        // -------------------- SIGNUP --------------------
        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromBody] UserSignUpDto dto)
        {
            var result = await _userService.SignupAsync(dto);
            return Ok(result);
        }

        // -------------------- LOGIN --------------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            if (result == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu.");
            return Ok(result);
        }

        // -------------------- RESET PASSWORD --------------------
        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPassDto dto)
        {
            var result = await _userService.ResetPasswordAsync(dto);
            if (!result)
                return BadRequest("Đổi mật khẩu thất bại.");
            return Ok("Đổi mật khẩu thành công.");
        }
    }
}
