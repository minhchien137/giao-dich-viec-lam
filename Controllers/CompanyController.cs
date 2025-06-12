using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebGiaoDichViecLam.Data;
using WebGiaoDichViecLam.Models;

namespace WebGiaoDichViecLam.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly ApplicationDbContext _context;

        public CompanyController(ILogger<CompanyController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> ListCompany(string search_keywords)
        {
            var query = _context.tblCompany.AsQueryable();

            if (!string.IsNullOrEmpty(search_keywords))
            {
                query = query.Where(c => EF.Functions.Like(c.sNameCompany, $"%{search_keywords}%") ||
                                         EF.Functions.Like(c.sAddressCompany, $"%{search_keywords}%"));
            }

            ViewBag.companies = await query.ToListAsync();
            ViewBag.SearchKeywords = search_keywords; // Để giữ giá trị từ khóa trong input
            return View();
        }
        public async Task<IActionResult> DetailCompany(int id)
        {
            ViewBag.company = await _context.tblCompany.FirstOrDefaultAsync(item => item.iCompanyID == id);
            ViewBag.countJob = await _context.tblJob.Where(j => j.iCompanyID == id).CountAsync();
            ViewBag.jobs = await _context.tblJob.Include(item => item.TblCategory).Where(j => j.iCompanyID == id).ToListAsync();
            return View();
        }
        public async Task<IActionResult> ListUserApply()
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
                    .Select(job => job.iJobID) // Chỉ lấy JobID để tối ưu hóa
                    .ToListAsync();
            var listUserApply = await _context.tblJobUser
                    .Where(jobUser => listJobByCompany.Contains(jobUser.iJobID)) // Lọc theo JobID
                    .Include(jobUser => jobUser.TblProfileUser) // (Tùy chọn) Bao gồm thông tin chi tiết người dùng
                    .Include(jobUser => jobUser.TblJob)
                    .ToListAsync();

            // Truyền dữ liệu sang ViewBag để hiển thị trong giao diện
            ViewBag.listUserApply = listUserApply;
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
