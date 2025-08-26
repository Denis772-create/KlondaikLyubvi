using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LoveNote> LoveNotes { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<BucketItem> BucketItems { get; set; }
        public DbSet<StoreItem> StoreItems { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<LoveCoinTransaction> LoveCoinTransactions { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Seed users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "denis", DisplayName = "Денис", PasswordHash = "denis" },
                new User { Id = 2, UserName = "liza", DisplayName = "Лиза", PasswordHash = "liza" }
            );
            // Seed store items
            modelBuilder.Entity<StoreItem>().HasData(
                new StoreItem { Id = 1, Name = "💆‍♀️ Массаж на 20 минут", Description = "Расслабляющий массаж от вашего любимого человека", Price = 5, Emoji = "💆‍♀️", UserId = 1 },
                new StoreItem { Id = 2, Name = "🍳 Завтрак в постель", Description = "Вкусный завтрак и кофе, приготовленные с любовью", Price = 4, Emoji = "🍳", UserId = 1 },
                new StoreItem { Id = 3, Name = "🎥 Вечер фильмов", Description = "Выбор фильма, плед и объятия", Price = 3, Emoji = "🎥", UserId = 2 },
                new StoreItem { Id = 4, Name = "🛁 Совместная ванна", Description = "Свечи, музыка и расслабление вдвоём", Price = 7, Emoji = "🛁", UserId = 2 }
            );
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int LovePoints { get; set; } = 12; // Баланс поцелуев
        public DateTime? LastVisit { get; set; }
    }

    public class LoveNote
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public class BucketItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public int Progress { get; set; } // 0-100
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public class StoreItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Price { get; set; } // В поцелуях
        public string Emoji { get; set; } = string.Empty;
        public int UserId { get; set; } // Владелец товара
        public User? User { get; set; }
    }

    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public class LoveCoinTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int StoreItemId { get; set; }
        public StoreItem? StoreItem { get; set; }
        public DateTime Date { get; set; }
        public bool IsGift { get; set; }
        public int? ToUserId { get; set; }
        public User? ToUser { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public bool IsExecuted { get; set; } = false;
        public DateTime? GiftStartDate { get; set; }
        public DateTime? GiftEndDate { get; set; }
        public int GiftCount { get; set; } = 1;
    }

    public class WishlistItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Occasion { get; set; } = string.Empty; // e.g., "День рождения", "Новый год"
        public string Url { get; set; } = string.Empty;
        public string? Title { get; set; } // custom title by user
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Resolved metadata
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaImage { get; set; }
        public string? MetaSiteName { get; set; }
        public string? MetaUrl { get; set; }
    }
} 