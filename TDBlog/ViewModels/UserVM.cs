namespace TDBlog.ViewModels
{
    public class UserVM
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? GithubUrl { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Role { get; set; }
    }
}
