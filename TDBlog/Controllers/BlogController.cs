using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDBlog.Data;
using TDBlog.Models;
using TDBlog.ViewModels;

namespace TDBlog.Controllers
{
    public class BlogController : Controller
    {
        public readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public INotyfService _notification { get; }

        public BlogController(ApplicationDbContext context, INotyfService notification, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _notification = notification;
        }
        [HttpGet("[controller]/{slug}")]
        public IActionResult Post(string slug)
        {
            if (slug == "")
            {
                _notification.Error("không tìm thấy bài đăng");
                return View();
            }
            var post = _context.Posts!.Include(x => x.ApplicationUser).FirstOrDefault(p => p.Slug == slug);
            if (post == null)
            {
                _notification.Error("không tìm thấy bài đăng");
                return View();
            }
            var vm = new BlogPostVM()
            {
                Id = post.Id,
                Title = post.Title,
                AuthorName = post.ApplicationUser!.FirstName + " " + post.ApplicationUser.LastName,
                CreatedDate = post.CreatedDate,
                ThumbnailUrl = post.ThumbnailUrl,
                Description = post.Description,
                ShortDescription = post.ShortDescription,
            };
            return View(vm);
        }


    }
}
