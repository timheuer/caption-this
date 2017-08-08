using CaptionThis.Models;
using Microsoft.EntityFrameworkCore;

namespace CaptionThis.Data
{
    public class VideoDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }

        public VideoDbContext(DbContextOptions<VideoDbContext> options)
            : base(options)
        {
            
        }
    }
}
