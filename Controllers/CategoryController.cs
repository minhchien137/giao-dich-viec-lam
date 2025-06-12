using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebGiaoDichViecLam.Data;
using WebGiaoDichViecLam.Models;

namespace WebGiaoDichViecLam.Controllers
{
    public class CategoryController : Controller
    {

        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            this._context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> CategoryManage()
        {
            var category = await _context.tblCategory.ToListAsync();
            return View(category);
        }



        [HttpGet]
        public async Task<IActionResult> AddCategoryJob()
        {
            return View();
        }



        [HttpPost]
        // [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> AddCategoryJob(tblCategory category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem tên ngành nghề đã tồn tại chưa
                    var existingCategory = await _context.tblCategory
                        .FirstOrDefaultAsync(c => c.sCategoryName == category.sCategoryName);

                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("sCategoryName", "Tên ngành nghề này đã tồn tại!");
                        return View(category); // Quay lại form nếu có lỗi
                    }

                    // Thêm mới ngành nghề vào cơ sở dữ liệu
                    _context.tblCategory.Add(category);
                    await _context.SaveChangesAsync();


                    return RedirectToAction("CategoryManage"); // Chuyển hướng sau khi thêm thành công
                }
                catch (DbUpdateException ex)
                {
                    // In thông báo lỗi chi tiết từ DbUpdateException
                    Console.WriteLine($"DbUpdateException: {ex.Message}");

                    // Kiểm tra và in thông báo lỗi từ InnerException
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                        // In chi tiết về InnerException (nếu có)
                        Console.WriteLine($"Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }

                    // Thêm chi tiết vào ModelState để hiển thị lỗi cho người dùng
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi thêm ngành nghề vào cơ sở dữ liệu: {ex.Message}");
                }


                catch (Exception ex)
                {
                    // Log lỗi không phải DbUpdateException
                    ModelState.AddModelError("", "Có lỗi không xác định xảy ra. Vui lòng thử lại.");
                    Console.WriteLine($"Exception: {ex.Message}");
                }

            }
            return View(category); // Nếu ModelState không hợp lệ hoặc có lỗi
        }




        [HttpPost]
        // [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteCategoryJob(int categoryId)
        {
            try
            {
                var category = await _context.tblCategory.FindAsync(categoryId);
                if (category == null)
                {
                    return Json(new { success = false, message = "Ngành nghề không tồn tại." });
                }

                _context.tblCategory.Remove(category);
                await _context.SaveChangesAsync();

                // Reseed Identity nếu cần
                var lastCategory = await _context.tblCategory
                                                .OrderByDescending(c => c.iCategoryID)
                                                .FirstOrDefaultAsync();
                int newIdentityValue = lastCategory?.iCategoryID ?? 0;
                await _context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('tblCategory', RESEED, {newIdentityValue});");

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình xóa ngành nghề này." });
            }
        }






    }
}
