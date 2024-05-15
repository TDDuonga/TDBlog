using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TDBlog.Models;

namespace TDBlog.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<ApplicationUser>? ApplicationUsers { get; set; }
        public DbSet<Post>? Posts { get; set; }
        public DbSet<Page>? Pages { get; set; }
        public DbSet<Setting>? Settings { get; set; }
        //public DbSet<Comment>? Comments { get; set; }
    }
}
