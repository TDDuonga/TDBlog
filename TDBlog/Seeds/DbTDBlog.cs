using Microsoft.AspNetCore.Identity;
using TDBlog.Data;
using TDBlog.Models;

namespace TDBlog.Seeds
{
    public class DbTDBlog : IDbTDBlog
    {
        public readonly ApplicationDbContext _Context;
        public readonly UserManager<ApplicationUser> _usermanager;
        public readonly RoleManager<IdentityRole> _rolemanager;

        public DbTDBlog(ApplicationDbContext context,
                            UserManager<ApplicationUser> userManager,
                            RoleManager<IdentityRole> rolemanager)
        {
            _Context = context;
            _usermanager = userManager;
            _rolemanager = rolemanager;
        }
        public void SeedsTDBlog()
        {
            if (!_rolemanager.RoleExistsAsync(WebsiteRoles.WebsiteAdmin).GetAwaiter().GetResult())
            {
                _rolemanager.CreateAsync(new IdentityRole(WebsiteRoles.WebsiteAdmin)).GetAwaiter().GetResult();
                _rolemanager.CreateAsync(new IdentityRole(WebsiteRoles.WebsiteAuthor)).GetAwaiter().GetResult();
                _usermanager.CreateAsync(new ApplicationUser()
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    FirstName = "Super",
                    LastName = "Admin"
                }, "Admin@0011").Wait();
                var appUser = _Context.ApplicationUsers!.FirstOrDefault(x => x.Email == "admin@gmail.com");
                if (appUser != null)
                {
                    _usermanager.AddToRoleAsync(appUser, WebsiteRoles.WebsiteAdmin).GetAwaiter().GetResult();
                }

                var listOfPages = new List<Page>()
                {
                    new Page()
                    {
                        Title = "About us",
                        Slug = "About"
                    },
                    new Page()
                    {
                        Title = "Contact us",
                        Slug = "Contact"
                    },
                    new Page()
                    {
                        Title = "Privacy Policy",
                        Slug = "Privacy"
                    }
                };
                _Context.Pages!.AddRange(listOfPages);
                _Context.SaveChanges();

            }

        }
    }
}
