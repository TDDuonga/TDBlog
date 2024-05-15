using AspNetCoreHero.ToastNotification.Abstractions;
using TDBlog.Data;
using TDBlog.Models;
using TDBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TDBlog.Seeds;

namespace FineBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotyfService _notitfication;
        private readonly IWebHostEnvironment _environment;
        public UserController(
                            ApplicationDbContext context,
                            UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            INotyfService notyfService,
                            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _notitfication = notyfService;  
            _environment = environment;

        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>  Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var vm = users.Select(x => new UserVM()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserName = x.UserName,
                Email = x.Email
            }).ToList();
            foreach(var user in vm)
            {
                var singleUser = await _userManager.FindByIdAsync(user.Id);
                var role = await _userManager.GetRolesAsync(singleUser);
                user.Role = role.FirstOrDefault();
            }
            return View(vm);
        }
        
        [HttpGet("Login")]
        public IActionResult Login()
        {
            if(!HttpContext.User.Identity!.IsAuthenticated)
            {
                return View(new LoginVM());
            }
            return RedirectToAction("Index","Post", new {area = "Admin"});
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginVM vm)
        {

            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            var exitstingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == vm.Username);
            if(exitstingUser == null)
            {
                _notitfication.Error("Tên đăng nhập không tồn tại");
                return View(vm);
            }
            var veryfipassword = await _userManager.CheckPasswordAsync(exitstingUser, vm.Password);
            if (!veryfipassword )
            {
                _notitfication.Error("Mật khẩu không hợp lệ");
                return View(vm);
            }
            await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, vm.RememberMe, true);
            _notitfication.Success("Đăng nhập thành công");
            return RedirectToAction("Index", "Home", new {area = ""});
        }
        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notitfication.Success("Bạn đã đăng xuất thành công");
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var checkUserByEmail = await _userManager.FindByEmailAsync(vm.Email);
            if (checkUserByEmail != null)
            {
                _notitfication.Error("Email đã tồn tại");

                return View(vm);
            }
            var checkUserByName = await _userManager.FindByNameAsync(vm.UserName);
            if (checkUserByName != null)
            {
                _notitfication.Error("User name đã tồn tại");
                return View(vm);
            }
            var applicationUser = new ApplicationUser()
            {
                Email = vm.Email,
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,

            };

            var result = await _userManager.CreateAsync(applicationUser, vm.Password);
            if (result.Succeeded)
            {
                if (vm.IsAdmin)
                {
                    await _userManager.AddToRoleAsync(applicationUser, WebsiteRoles.WebsiteAdmin);
                }
                else
                {
                    await _userManager.AddToRoleAsync(applicationUser, WebsiteRoles.WebsiteAuthor);
                }
                _notitfication.Success("Đăng ký người dùng thành công");
                return RedirectToAction("Index", "User", new { area = "Admin" });
            }
            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var profile = await _context.ApplicationUsers!.FirstOrDefaultAsync();
         
            var vm = new UserVM()
            {
                Id = profile!.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                UserName = profile.UserName,
                Email = profile.Email,
                ThumbnailUrl = profile.ThumbnailUrl,
            };
            //var role = await _userManager.GetRolesAsync(users);
            //vm.Role = role.FirstOrDefault();
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(UserVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var profile = await _context.ApplicationUsers!.FirstOrDefaultAsync();
            if (profile == null)
            {
                _notitfication.Error("User sai");
                return View(vm);
            }
            profile.Id = vm.Id;
            profile.FirstName = vm.FirstName;
            profile.LastName = vm.LastName;
            profile.UserName = vm.UserName;
            profile.Email = vm.Email;
            if (vm.ThumbnailUrl != null)
            {
                profile.ThumbnailUrl = UploadImage(vm.ThumbnailUrl);
            }
            await _context.SaveChangesAsync();
            _notitfication.Success("User page update successfully");
            return RedirectToAction("Profile", "User", new { area = "Admin" });
        }

        private string? UploadImage(string thumbnailUrl)
        {
            throw new NotImplementedException();
        }

        public string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderPath = Path.Combine(_environment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, uniqueFileName);
            using (FileStream filestream = System.IO.File.Create(filePath))
            {
                file.CopyTo(filestream);
            }
            return uniqueFileName;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var exitingUser = await _userManager.FindByIdAsync(id);
            if(exitingUser == null)
            {
                _notitfication.Error("Tên người dùng đã tồn tại");
                return View();
            }
            var vm = new ResetPassword()
            {
                Id = exitingUser.Id,
                UserName = exitingUser.UserName,
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword vm)
        {
            if(!ModelState.IsValid) { return View(vm); }
            var exitingUser = await _userManager.FindByIdAsync(vm.Id); 
            if(exitingUser == null)
            {
                _notitfication.Error("Tên người dùng đã tồn tại");
                return View(vm);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(exitingUser);
            var result = await _userManager.ResetPasswordAsync(exitingUser, token, vm.NewPassword);
            if(result.Succeeded)
            {
                _notitfication.Success("Reset password thành công");
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }


       

        //public async Task<IActionResult> Profile(string Id)
        //{
        //    var user = await _userManager.FindByIdAsync(Id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    var vm = new UserVM()
        //    {
        //        Id = user.Id,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        UserName = user.UserName,
        //        Email = user.Email
        //    };

        //    var roles = await _userManager.GetRolesAsync(user);
        //    vm.Role = roles.FirstOrDefault();

        //    return View(vm);
        //}

        //[HttpGet("AccessDenied")]
        //[Authorize]
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}

        //public async Task<IActionResult> Delete(int id)
        //{
        //    var user = await _context.Users!.FirstOrDefaultAsync(x => x.Id == id);
        //    var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
        //    var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
        //    if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin || loggedInUser?.Id == post?.ApplicationUserId)
        //    {
        //        _context.Posts!.Remove(post!);
        //        await _context.SaveChangesAsync();
        //        _notification.Success("Xoá bài đăng thành công");
        //        return RedirectToAction("Index", "Post", new { area = "Admin" });
        //    }
        //    return View();
        //}
    }
}
