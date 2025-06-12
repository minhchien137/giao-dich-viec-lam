using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebGiaoDichViecLam.Data;
using WebGiaoDichViecLam.Models;

namespace WebGiaoDichViecLam.Controllers
{
    public class AccountController : Controller
    {
        private const int MaxFailedAttempts = 3;
        private const int LockoutMinutes = 30;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("LockoutTime") != null)
            {
                DateTime lockoutTime = DateTime.Parse(HttpContext.Session.GetString("LockoutTime"));
                if (lockoutTime > DateTime.Now)
                {
                    ModelState.AddModelError("", $"Tài khoản bị khóa. Vui lòng thử lại sau {lockoutTime - DateTime.Now:mm\\:ss} phút.");
                    ViewData["IsLockedOut"] = true;
                    return View();
                }
                else
                {
                    // Xóa thời gian khóa nếu đã hết hạn
                    HttpContext.Session.Remove("LockoutTime");
                    HttpContext.Session.SetInt32("FailedAttempts", 0);
                    ViewData["IsLockedOut"] = false;
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (HttpContext.Session.GetInt32("FailedAttempts") == null)
            {
                HttpContext.Session.SetInt32("FailedAttempts", 0);
            }
            // Giả sử phương thức ValidateUser để kiểm tra thông tin đăng nhập
            var user = ValidateUser(username, password);
            if (user != null)
            {
                // Đăng nhập thành công
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, user.iAccountID.ToString()),
                    new Claim(ClaimTypes.Name, user.sUserName.ToString()),
                    new Claim(ClaimTypes.Role, user.iRoleID.ToString()) // Hoặc vai trò phù hợp
                };

                // Tạo ClaimsIdentity và Cookie Authentication
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30), // Hết hạn sau 30 phút
                    IsPersistent = true // Giữ đăng nhập sau khi đóng trình duyệt
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetInt32("FailedAttempts", 0);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Đăng nhập thất bại, tăng số lần thử
                int failedAttempts = HttpContext.Session.GetInt32("FailedAttempts").Value;
                failedAttempts++;
                HttpContext.Session.SetInt32("FailedAttempts", failedAttempts);

                if (failedAttempts >= MaxFailedAttempts)
                {
                    // Khóa tài khoản trong 30 phút
                    DateTime lockoutTime = DateTime.Now.AddMinutes(LockoutMinutes);
                    HttpContext.Session.SetString("LockoutTime", lockoutTime.ToString());
                    ViewData["IsLockedOut"] = true;
                    ModelState.AddModelError("", "Bạn đã đăng nhập sai quá 3 lần. Vui lòng thử lại sau 30 phút.");
                }
                else
                {
                    ViewData["IsLockedOut"] = false;
                    ModelState.AddModelError("", $"Đăng nhập thất bại. Bạn còn {MaxFailedAttempts - failedAttempts} lần thử.");
                }
            }
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string username, string password, string email, string reg_role)
        {

            var existingAccount = _context.tblAccount.FirstOrDefault(a => a.sUserName == username || a.sEmailAccount == email);
            if (existingAccount != null)
            {
                ViewBag.ErrorMessage = "Username hoặc tài khoản email đã tồn tại.";
                return View();
            }

            // Tạo tài khoản mới
            var roleID = reg_role == "candidate" ? 1 : reg_role == "employer" ? 2 : 0;
            if (roleID == 0)
            {
                ViewBag.ErrorMessage = "Vai trò không hợp lệ.";
                return View();
            }

            // Đăng ký tài khoản
            var newAccount = new tblAccount
            {
                iAccountID = GenerateUniqueId(), // Gọi hàm tạo ID duy nhất
                sUserName = username,
                sPassword = password,
                sEmailAccount = email,
                iRoleID = roleID
            };

            _context.tblAccount.Add(newAccount);
            await _context.SaveChangesAsync();

            if (roleID == 1)
            {
                return RedirectToAction("SignUpUser", new { accountId = newAccount.iAccountID });

            }
            else if (roleID == 2)
            {
                return RedirectToAction("SignUpEmployer", new { accountId = newAccount.iAccountID });
            }
            else
            {
                ViewBag.ErrorMessage = "Không xác định được vai trò.";
                return View();
            }
        }


        [HttpGet]
        public IActionResult SignUpUser(int accountId)
        {
            _logger.LogInformation($"Received accountId in SignUpUser: {accountId}");
            ViewBag.AccountId = accountId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUpUser(int iAccountID, string fullname, string phone, string address, string skills, string experience, string education, IFormFile cv, IFormFile profile_picture)
        {


            // Log thông tin nhận được
            _logger.LogInformation($"Received data: AccountID = {iAccountID}, FullName = {fullname}, Phone = {phone}, Address = {address}");

            // Kiểm tra nếu accountID tồn tại trong bảng tblAccount
            var account = await _context.tblAccount.FindAsync(iAccountID);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                return View();
            }

            // Tạo profile cho ứng viên
            var profile = new tblProfileUser
            {
                iProfileID = GenerateUniquePU(),
                iAccountID = iAccountID,
                sFullName = fullname,
                sPhoneNumber = phone,
                sAddress = address,
                sSkills = skills,
                sExperience = experience,
                sEducation = education,
                sCV = cv != null ? cv.FileName : string.Empty,
                sPhoto = profile_picture != null ? profile_picture.FileName : string.Empty
            };

            _context.tblProfileUser.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        public IActionResult SignUpEmployer(int accountId)
        {
            ViewBag.AccountId = accountId;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SignUpEmployer(int iAccountID, string fullname, string phone, string address,
                                                string companyName, int companyPhone, string companyAddress,
                                                string companyDescription, int numberEmployees, IFormFile companyPhoto)
        {
            // Kiểm tra nếu accountID tồn tại trong bảng tblAccount
            var account = await _context.tblAccount.FindAsync(iAccountID);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                return View();
            }

            // Xử lý ảnh công ty nếu có
            string photoPath = "default.png"; // Giá trị mặc định nếu không có ảnh

            if (companyPhoto != null && companyPhoto.Length > 0)
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/companies");

                // Kiểm tra thư mục tồn tại, nếu không thì tạo mới
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName = Guid.NewGuid().ToString() + "_" + companyPhoto.FileName;
                string path = Path.Combine(directoryPath, fileName);

                // Lưu ảnh công ty vào thư mục
                try
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await companyPhoto.CopyToAsync(stream);
                    }

                    photoPath = "images/companies/" + fileName; // Cập nhật đường dẫn ảnh
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Không thể lưu ảnh công ty: " + ex.Message;
                    return View();
                }
            }

            // Tạo bản ghi công ty trước
            var company = new tblCompany
            {
                iCompanyID = GenerateUniqueCPN(),
                sNameCompany = companyName,
                iPhoneNumber = companyPhone,
                sAddressCompany = companyAddress,
                iNumberEmployees = numberEmployees,
                sDescriptionCompany = companyDescription,
                sPhotoCompany = photoPath
            };

            try
            {
                _context.tblCompany.Add(company);
                await _context.SaveChangesAsync(); // Lưu công ty vào database
            }
            catch (DbUpdateException dbEx)
            {
                // Log hoặc hiển thị thông tin chi tiết lỗi
                ViewBag.ErrorMessage = "Không thể lưu công ty: " + dbEx.Message;
                if (dbEx.InnerException != null)
                {
                    ViewBag.ErrorMessage += " - " + dbEx.InnerException.Message;
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi xảy ra: " + ex.Message;
                return View();
            }

            // Sau khi công ty được lưu, lấy giá trị iCompanyID và tạo bản ghi nhà tuyển dụng
            var employer = new tblEmployer
            {
                iEmployerID = GenerateUniqueEmployerId(), // Hàm tạo ID duy nhất
                iAccountID = iAccountID,
                sFullName = fullname,
                sPhoneNumber = phone,
                sAddress = address,
                iCompanyID = company.iCompanyID // Đảm bảo iCompanyID được gán giá trị hợp lệ
            };

            try
            {
                _context.tblEmployer.Add(employer);
                await _context.SaveChangesAsync(); // Lưu nhà tuyển dụng vào database
            }
            catch (DbUpdateException dbEx)
            {
                ViewBag.ErrorMessage = "Không thể lưu nhà tuyển dụng: " + dbEx.Message;
                if (dbEx.InnerException != null)
                {
                    ViewBag.ErrorMessage += " - " + dbEx.InnerException.Message;
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi xảy ra: " + ex.Message;
                return View();
            }

            return RedirectToAction("Login", "Account"); // Chuyển hướng sau khi đăng ký thành công
        }


        private tblAccount ValidateUser(string username, string password)
        {
            // Kiểm tra tên đăng nhập và mật khẩu, trả về true nếu đúng
            // Thay bằng logic kiểm tra thực tế, ví dụ từ cơ sở dữ liệu
            var user = _context.tblAccount.FirstOrDefault(u => (u.sEmailAccount == username || u.sUserName == username) && u.sPassword == password);
            return user;
        }

        private int GenerateUniqueId()
        {
            int newId = _context.tblAccount.Max(a => (int?)a.iAccountID) ?? 0;
            return newId + 1;
        }

        private int GenerateUniquePU()
        {
            // Lấy giá trị ID mới (hoặc từ cơ sở dữ liệu nếu cần)
            int newId = _context.tblProfileUser.Max(a => (int?)a.iProfileID) ?? 0;
            return newId + 1;
        }

        private int GenerateUniqueCPN()
        {
            int newId = _context.tblCompany.Max(a => (int?)a.iCompanyID) ?? 0;
            return newId + 1;
        }

        private int GenerateUniqueEmployerId()
        {
            int newId = _context.tblEmployer.Max(e => (int?)e.iEmployerID) ?? 0;
            return newId + 1;
        }
        public async Task<IActionResult> DetailAccount()
        {
            var accountId = User.FindFirstValue(ClaimTypes.Sid);
            ViewBag.user = await _context.tblProfileUser.Include(item => item.TblAccount)
                        .FirstOrDefaultAsync(item => item.iAccountID.ToString() == accountId);
            Console.WriteLine("Check: " + ViewBag.user.sAddress);
            return View();
        }
    }
}
