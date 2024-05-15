using AspNetCoreHero.ToastNotification.Abstractions;
using TDBlog.Data;
using TDBlog.Models;
using TDBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using TDBlog.Seeds;

namespace TDBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private INotyfService _notification { get; }
        private readonly IWebHostEnvironment _environment;   
        private readonly UserManager<ApplicationUser>  _userManager;
        public PostController(ApplicationDbContext context, 
                                INotyfService notification, 
                                IWebHostEnvironment environment, 
                                UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notification = notification;
            _environment = environment;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            var lisOfPosts = new List<Post>();

            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin)
            { 
                 lisOfPosts = await _context.Posts!.Include(x=>x.ApplicationUser).ToListAsync();
            }
            else
            {
                 lisOfPosts = await _context.Posts!.Include(x=>x.ApplicationUser).Where(x=>x.ApplicationUser!.Id==loggedInUser!.Id).ToListAsync();
            }
            var lisofPostsVM = lisOfPosts.Select(x => new PostVM()
            {
                Id = x.Id,
                Title = x.Title,
                CreatedDate = x.CreatedDate,
                ThumbnailUrl = x.ThumbnailUrl,
                AuthorName = x.ApplicationUser!.FirstName + " " + x.ApplicationUser.LastName
            }).ToList();

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(await lisofPostsVM.OrderByDescending(x => x.CreatedDate).ToPagedListAsync(pageNumber, pageSize));
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostVM());
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostVM vm)
        {
            if(!ModelState.IsValid) { return View(vm); }
            //đăng nhập id người dùng
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            
            var post = new Post();

            post.Title = vm.Title;
            post.Description = vm.Description;
            post.ShortDescription = vm.ShortDescription;
            post.ApplicationUserId = loggedInUser!.Id;
            if(vm.Title != null)
            {
                string slug = vm.Title!.Trim();
                slug = slug.Replace(" "," ");
                post.Slug = slug + "-" + Guid.NewGuid();

            }

            if(vm.Thumbnail != null)
            {
                post.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.Posts!.AddAsync(post);
            await _context.SaveChangesAsync();
            _notification.Success("Bài đăng được tạo thành công");
            return RedirectToAction("Index");
        }
        public string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderPath = Path.Combine(_environment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, uniqueFileName);
            using(FileStream filestream = System.IO.File.Create(filePath))
            {
                file.CopyTo(filestream);
            }
            return uniqueFileName;
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts!.FirstOrDefaultAsync(x => x.Id == id);
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin || loggedInUser?.Id == post?.ApplicationUserId)
            {
                _context.Posts!.Remove(post!);
                await _context.SaveChangesAsync();
                _notification.Success("Xoá bài đăng thành công");
                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts!.FirstOrDefaultAsync(x => x.Id == id);
            if(post == null)
            {
                _notification.Error("Bài đăng không tìm thấy");
                return View();
            }

            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] != WebsiteRoles.WebsiteAdmin && loggedInUser!.Id != post.ApplicationUserId)
            {
                _notification.Error("You are not authorized");
                return RedirectToAction("Index");
            }
            var vm = new CreatePostVM()
            {
                Id = post.Id,
                Title = post.Title,
                ShortDescription = post.ShortDescription,
                Description = post.Description,
                ThumbnailUrl = post.ThumbnailUrl
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CreatePostVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var post = await _context.Posts!.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (post == null)
            {
                _notification.Success("Bài đăng không tìm thấy");
                return View();
            }
            post.Title = vm.Title;
            post.ShortDescription = vm.ShortDescription;
            post.Description = vm.Description;
            if(vm.Thumbnail != null)
            {
                post.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }
            await _context.SaveChangesAsync();
            _notification.Success("Chỉnh sửa bài đăng thành công");
            return RedirectToAction("Index","Post", new {area = "Admin"});
        }
    }
}
