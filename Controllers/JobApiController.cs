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
    [Route("api/job")]
    public class JobController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyJob([FromBody] JsonElement data)
        {
            int jobId = int.Parse(data.GetProperty("jobID").GetString());
            string message = data.GetProperty("message").GetString();
            // Kiểm tra xem job có tồn tại không
            var job = await _context.tblJob.FindAsync(jobId);
            if (job == null)
            {
                return NotFound("Công việc không tồn tại.");
            }
            // Lấy thông tin người dùng hiện tại
            var profile = await _context.tblProfileUser
                .FirstOrDefaultAsync(p => p.iAccountID.ToString() == User.FindFirstValue(ClaimTypes.Sid)); //User.FindFirstValue(ClaimTypes.Sid) là ID người dùng.
            if (profile == null)
            {
                return BadRequest("Không tìm thấy thông tin hồ sơ người dùng.");
            }
            // Tạo đối tượng tblJobUser
            var jobUser = new tblJobUser
            {
                iJobID = jobId,
                iProfileID = profile.iProfileID,
                sType = "Apply",
                sMessage = message
            };

            // Lưu vào cơ sở dữ liệu
            _context.tblJobUser.Add(jobUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ứng tuyển thành công!" });
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveJob([FromBody] JsonElement data)
        {
            int jobId = int.Parse(data.GetProperty("jobID").GetString());

            // Kiểm tra xem công việc có tồn tại không
            var job = await _context.tblJob.FindAsync(jobId);
            if (job == null)
            {
                return NotFound("Công việc không tồn tại.");
            }

            // Lấy thông tin người dùng hiện tại
            var profile = await _context.tblProfileUser
                .FirstOrDefaultAsync(p => p.iAccountID.ToString() == User.FindFirstValue(ClaimTypes.Sid)); // User.FindFirstValue(ClaimTypes.Sid) là ID người dùng.
            if (profile == null)
            {
                return BadRequest("Không tìm thấy thông tin hồ sơ người dùng.");
            }

            // Kiểm tra nếu công việc đã được lưu
            var existingSavedJob = await _context.tblSavedJob
                .FirstOrDefaultAsync(s => s.iJobID == jobId && s.iProfileID == profile.iProfileID);

            if (existingSavedJob != null)
            {
                return BadRequest("Công việc này đã được lưu trước đó.");
            }

            // Tạo đối tượng tblSavedJob để lưu công việc
            var savedJob = new tblSavedJob
            {
                iJobID = jobId,
                iProfileID = profile.iProfileID
            };

            // Lưu vào cơ sở dữ liệu
            _context.tblSavedJob.Add(savedJob);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Công việc đã được lưu thành công!" });
        }

        [HttpGet("getJobDetail/{jobId}")]
        public async Task<IActionResult> GetDetailJob(int jobId)
        {
            var job = await _context.tblJob.Include(item => item.TblCategory)
            .FirstOrDefaultAsync(item => item.iJobID == jobId);
            return Ok(new
            {
                NameJob = job.sNameJob,
                SalaryJob = job.fSalaryJob,
                TypeJob = job.sTypeJob,
                PostedDate = job.dPostedDate, // Ví dụ thêm
                ExpiryDate = job.dExpiryDate, // Ví dụ thêm
                DescriptionJob = job.sDescriptionJob,
                CategoryName = job.TblCategory.sCategoryName,
                Applicants = _context.tblJobUser.Count(ju => ju.iJobID == job.iJobID)
            });
        }
        [HttpPost("unsave")]
        public async Task<IActionResult> UnsaveJob([FromBody] JsonElement data)
        {
            int jobId = int.Parse(data.GetProperty("jobID").GetString());

            // Kiểm tra xem công việc có tồn tại không
            var job = await _context.tblJob.FindAsync(jobId);
            if (job == null)
            {
                return NotFound("Công việc không tồn tại.");
            }

            // Lấy thông tin người dùng hiện tại
            var profile = await _context.tblProfileUser
                .FirstOrDefaultAsync(p => p.iAccountID.ToString() == User.FindFirstValue(ClaimTypes.Sid)); // User.FindFirstValue(ClaimTypes.Sid) là ID người dùng.
            if (profile == null)
            {
                return BadRequest("Không tìm thấy thông tin hồ sơ người dùng.");
            }

            // Kiểm tra xem công việc đã được lưu chưa
            var savedJob = await _context.tblSavedJob
                .FirstOrDefaultAsync(s => s.iJobID == jobId && s.iProfileID == profile.iProfileID);

            if (savedJob == null)
            {
                return NotFound("Công việc này chưa được lưu.");
            }

            // Xóa công việc khỏi danh sách đã lưu
            _context.tblSavedJob.Remove(savedJob);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Công việc đã được hủy lưu thành công!" });
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddJob([FromBody] JsonElement data)
        {
            try
            {
                // Lấy dữ liệu từ JsonElement
                Console.WriteLine("Check data: " + data);
                var employ = await _context.tblEmployer
                .FirstOrDefaultAsync(p => p.iAccountID.ToString() == User.FindFirstValue(ClaimTypes.Sid));
                int iCompanyID = employ.iCompanyID;
                int iCategoryID = int.Parse(data.GetProperty("iCategoryID").GetString());
                string sNameJob = data.GetProperty("sNameJob").GetString() ?? string.Empty;
                double fSalaryJob = data.GetProperty("fSalaryJob").GetDouble();
                string sTypeJob = data.GetProperty("sTypeJob").GetString() ?? string.Empty;
                DateTime dExpiryDate = DateTime.Parse(data.GetProperty("dExpiryDate").GetString());
                string sDescriptionJob = data.GetProperty("sDescriptionJob").GetString() ?? string.Empty;

                // Kiểm tra công ty và danh mục có tồn tại không
                var companyExists = await _context.tblCompany.AnyAsync(c => c.iCompanyID == iCompanyID);
                var categoryExists = await _context.tblCategory.AnyAsync(c => c.iCategoryID == iCategoryID);
                var company = await _context.tblCompany.FirstOrDefaultAsync(c => c.iCompanyID == iCompanyID);
                if (!companyExists)
                    return NotFound(new { message = "Công ty không tồn tại." });
                if (!categoryExists)
                    return NotFound(new { message = "Danh mục không tồn tại." });
                // Tạo đối tượng tblJob
                var job = new tblJob
                {
                    iCompanyID = iCompanyID,
                    iCategoryID = iCategoryID,
                    sNameJob = sNameJob,
                    sAddressJob = company.sAddressCompany,
                    fSalaryJob = fSalaryJob,
                    sTypeJob = sTypeJob,
                    dPostedDate = DateTime.Now,
                    dExpiryDate = dExpiryDate,
                    sDescriptionJob = sDescriptionJob,

                };

                // Lưu vào cơ sở dữ liệu
                await _context.tblJob.AddAsync(job);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm việc thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra.", error = ex.Message });
            }
        }
    }
}