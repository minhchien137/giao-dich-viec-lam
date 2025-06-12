using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebGiaoDichViecLam.Data;
using WebGiaoDichViecLam.Models;

//Diep
namespace WebGiaoDichViecLam.Controllers
{
    public class JobClientController : Controller
    {
        private readonly ILogger<JobClientController> _logger;
        private readonly ApplicationDbContext _context;

        public JobClientController(ILogger<JobClientController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> SearchJob()
        {
            try
            {
                var jobs = await _context.tblJob.Include(j => j.TblCategory).Include(j => j.TblCompany).ToListAsync();
                var categories = await _context.tblCategory.ToArrayAsync();
                ViewBag.Jobs = jobs;
                ViewBag.Categories = categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn dữ liệu từ tblJob.");
                throw;
            }
            return View();
        }
        public async Task<IActionResult> DetailJob(int id)
        {
            var job = await _context.tblJob.Include(j => j.TblCategory).Include(j => j.TblCompany).FirstOrDefaultAsync(item => item.iJobID == id);

            // Kiểm tra xem người dùng có đăng nhập hay không
            var accountId = User.FindFirstValue(ClaimTypes.Sid);

            if (!string.IsNullOrEmpty(accountId))
            {
                // Lấy thông tin hồ sơ của người dùng
                var profile = await _context.tblProfileUser
                    .FirstOrDefaultAsync(p => p.iAccountID.ToString() == accountId);

                if (profile != null)
                {
                    // Kiểm tra xem công việc này đã được người dùng lưu chưa
                    var isJobSaved = await _context.tblSavedJob
                        .AnyAsync(sj => sj.iProfileID == profile.iProfileID && sj.iJobID == id);

                    // Truyền thông tin vào ViewBag để hiển thị trạng thái nút "Lưu công việc"
                    ViewBag.IsJobSaved = isJobSaved;
                }
            }

            // Truyền thông tin công việc vào View
            ViewBag.job = job;

            return View();
        }

        public async Task<IActionResult> MyApplyJob()
        {
            var accountId = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(accountId))
            {
                return View(); // Hoặc xử lý phù hợp nếu người dùng không đăng nhập
            }
            var profile = await _context.tblProfileUser
                .FirstOrDefaultAsync(p => p.iAccountID.ToString() == accountId);

            if (profile == null)
            {
                return View(); // Trả về lỗi hoặc redirect nếu không tìm thấy profile
            }

            ViewBag.jobUserApply = await _context.tblJobUser
                    .Include(j => j.TblProfileUser) // Include bảng tblProfileUser
                    .Include(j => j.TblJob)        // Include bảng tblJob
                        .ThenInclude(job => job.TblCategory) // Include thêm bảng tblCategory từ TblJob
                    .Include(j => j.TblJob.TblCompany) // Include thêm bảng tblCompany từ TblJob
                    .Where(j => j.iProfileID == profile.iProfileID)
                    .ToListAsync();

            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> JobInCompany()
        {
            if (int.Parse(User.FindFirstValue(ClaimTypes.Role)) != 2)
            {
                return RedirectToAction("Index", "Home");
            }
            var employer = await _context.tblEmployer.FirstOrDefaultAsync(item => item.iAccountID == int.Parse(User.FindFirstValue(ClaimTypes.Sid)));
            if (employer == null)
            {
                return NotFound("Employer not found");
            }

            var listJobByCompany = await _context.tblJob
                .Where(item => item.iCompanyID == employer.iCompanyID)
                .Include(job => job.TblCategory)
                .ToListAsync();
            ViewBag.jobInCompany = listJobByCompany;
            ViewBag.Categories = await _context.tblCategory.ToListAsync();
            return View();
        }
        public async Task<IActionResult> MySaveJob()
        {
            var accountId = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(accountId))
            {
                return View(); // Hoặc xử lý phù hợp nếu người dùng không đăng nhập
            }

            // Lấy thông tin hồ sơ người dùng
            var profile = await _context.tblProfileUser
                .FirstOrDefaultAsync(p => p.iAccountID.ToString() == accountId);

            if (profile == null)
            {
                return View(); // Trả về lỗi hoặc redirect nếu không tìm thấy profile
            }

            // Lấy danh sách công việc đã lưu của người dùng
            var savedJobs = await _context.tblSavedJob
                .Include(sj => sj.TblJob) // Bao gồm thông tin công việc
                .Include(sj => sj.TblJob.TblCategory) // Bao gồm thông tin danh mục công việc
                .Include(sj => sj.TblJob.TblCompany) // Bao gồm thông tin công ty
                .Where(sj => sj.iProfileID == profile.iProfileID) // Lọc theo profile người dùng
                .ToListAsync();

            ViewBag.SavedJobs = savedJobs;

            return View();
        }
    }
}
