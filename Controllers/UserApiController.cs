using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebGiaoDichViecLam.Data;
using WebGiaoDichViecLam.Models;

namespace WebGiaoDichViecLam.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("getUserDetails/{userId}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            var user = await _context.tblProfileUser.Include(item => item.TblAccount)
            .FirstOrDefaultAsync(item => item.iProfileID == userId);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại." });
            }
            return Ok(new
            {
                FullName = user.sFullName,
                Email = user.TblAccount.sEmailAccount,
                Phone = user.sPhoneNumber,
                Address = user.sAddress, // Ví dụ thêm
                Experience = user.sExperience, // Ví dụ thêm
                Skill = user.sSkills,
                CV = user.sCV,
                Photo = user.sPhoto
            });
        }
        [HttpPost("updateUserDetails")]
        public async Task<IActionResult> UpdateUserDetails([FromBody] JsonElement data)
        {
            try
            {
                // Lấy dữ liệu từ body (dưới dạng JSON)
                var fullName = data.GetProperty("FullName").GetString();
                var phoneNumber = data.GetProperty("PhoneNumber").GetString();
                var email = data.GetProperty("Email").GetString();
                var address = data.GetProperty("Address").GetString();
                var skills = data.GetProperty("Skills").GetString();
                var experience = data.GetProperty("Experience").GetString();
                var education = data.GetProperty("Education").GetString();
                var userId = data.GetProperty("UserId").GetInt64();
                // Cập nhật thông tin người dùng
                var user = await _context.tblProfileUser.Include(u => u.TblAccount)
                                     .FirstOrDefaultAsync(u => u.iProfileID == userId);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại." });
                }

                user.sFullName = fullName;
                user.sPhoneNumber = phoneNumber;
                user.TblAccount.sEmailAccount = email;
                user.sAddress = address;
                user.sSkills = skills;
                user.sExperience = experience;
                user.sEducation = education;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thông tin đã được cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra.", error = ex.Message });
            }
        }
    }
}