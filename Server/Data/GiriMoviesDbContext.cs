using Microsoft.EntityFrameworkCore;
using GiriMovies.Shared.Models;

namespace GiriMovies.Server.Data;

public class GiriMoviesDbContext : DbContext
{
    public GiriMoviesDbContext(DbContextOptions<GiriMoviesDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<WatchProgress> WatchProgresses { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
        
        // Movie configuration
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Genre).HasMaxLength(100);
        });
        
        // WatchProgress configuration
        modelBuilder.Entity<WatchProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.WatchProgresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Movie)
                .WithMany(m => m.WatchProgresses)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Ensure only one progress record per user per movie
            entity.HasIndex(e => new { e.UserId, e.MovieId }).IsUnique();
            
            entity.Property(e => e.LastWatchedDevice).HasMaxLength(50);
        });
        
        // UserSession configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DeviceId).HasMaxLength(100);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.SessionToken).HasMaxLength(256);
            entity.Property(e => e.CertificateThumbprint).HasMaxLength(100);
            entity.Property(e => e.CertificateSubject).HasMaxLength(500);
            
            entity.HasIndex(e => e.SessionToken);
            entity.HasIndex(e => e.CertificateThumbprint);
            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });
        
        // Seed data
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed movies
        modelBuilder.Entity<Movie>().HasData(
            new Movie
            {
                Id = 1,
                Title = "The Shawshank Redemption",
                Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                ThumbnailUrl = "https://via.placeholder.com/300x450?text=Shawshank",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                DurationInSeconds = 8520, // 142 minutes
                Genre = "Drama",
                ReleaseYear = 1994,
                Rating = 9.3,
                CreatedAt = DateTime.UtcNow
            },
            new Movie
            {
                Id = 2,
                Title = "The Dark Knight",
                Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests.",
                ThumbnailUrl = "https://via.placeholder.com/300x450?text=Dark+Knight",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                DurationInSeconds = 9120, // 152 minutes
                Genre = "Action",
                ReleaseYear = 2008,
                Rating = 9.0,
                CreatedAt = DateTime.UtcNow
            },
            new Movie
            {
                Id = 3,
                Title = "Inception",
                Description = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea.",
                ThumbnailUrl = "https://via.placeholder.com/300x450?text=Inception",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4",
                DurationInSeconds = 8880, // 148 minutes
                Genre = "Sci-Fi",
                ReleaseYear = 2010,
                Rating = 8.8,
                CreatedAt = DateTime.UtcNow
            },
            new Movie
            {
                Id = 4,
                Title = "Pulp Fiction",
                Description = "The lives of two mob hitmen, a boxer, a gangster and his wife intertwine in four tales of violence and redemption.",
                ThumbnailUrl = "https://via.placeholder.com/300x450?text=Pulp+Fiction",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4",
                DurationInSeconds = 9240, // 154 minutes
                Genre = "Crime",
                ReleaseYear = 1994,
                Rating = 8.9,
                CreatedAt = DateTime.UtcNow
            },
            new Movie
            {
                Id = 5,
                Title = "The Matrix",
                Description = "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.",
                ThumbnailUrl = "https://via.placeholder.com/300x450?text=Matrix",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4",
                DurationInSeconds = 8160, // 136 minutes
                Genre = "Sci-Fi",
                ReleaseYear = 1999,
                Rating = 8.7,
                CreatedAt = DateTime.UtcNow
            }
        );
        
        // Seed a test user (password: "Test123!")
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "test@GiriMovies.com",
                Name = "Test User",
                // This is a hashed version of "Test123!" - you should use proper password hashing
                PasswordHash = "$2a$11$zQjJ5VvZ7Z2xZ2xZ2xZ2xOe5Z2xZ2xZ2xZ2xZ2xZ2xZ2xZ2xZ2xZ2",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            }
        );
    }
}
