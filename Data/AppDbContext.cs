using Microsoft.EntityFrameworkCore;

namespace KlondaikLyubvi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LoveNote> LoveNotes { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<BucketItem> BucketItems { get; set; }
        public DbSet<ServiceOffer> ServiceOffers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<ServiceExchange> ServiceExchanges { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Настройка связей для ServiceExchange
            modelBuilder.Entity<ServiceExchange>()
                .HasOne(e => e.Requester)
                .WithMany()
                .HasForeignKey(e => e.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<ServiceExchange>()
                .HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<ServiceExchange>()
                .HasOne(e => e.RequestedService)
                .WithMany()
                .HasForeignKey(e => e.RequestedServiceId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<ServiceExchange>()
                .HasOne(e => e.OfferedService)
                .WithMany()
                .HasForeignKey(e => e.OfferedServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Seed users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "denis", DisplayName = "Денис", PasswordHash = "denis" },
                new User { Id = 2, UserName = "liza", DisplayName = "Лиза", PasswordHash = "liza" }
            );
            
            // Seed service offers
            modelBuilder.Entity<ServiceOffer>().HasData(
                new ServiceOffer { Id = 1, Name = "Массаж на 20 минут", Description = "Расслабляющий массаж спины и плеч", Emoji = "💆‍♀️", UserId = 1, Category = "Релакс", CreatedAt = DateTime.UtcNow },
                new ServiceOffer { Id = 2, Name = "Завтрак в постель", Description = "Вкусный завтрак и кофе, приготовленные с любовью", Emoji = "🍳", UserId = 1, Category = "Забота", CreatedAt = DateTime.UtcNow },
                new ServiceOffer { Id = 3, Name = "Вечер фильмов", Description = "Выбор фильма, плед и объятия", Emoji = "🎥", UserId = 2, Category = "Досуг", CreatedAt = DateTime.UtcNow },
                new ServiceOffer { Id = 4, Name = "Совместная ванна", Description = "Свечи, музыка и расслабление вдвоём", Emoji = "🛁", UserId = 2, Category = "Романтика", CreatedAt = DateTime.UtcNow },
                new ServiceOffer { Id = 5, Name = "Домашний ужин", Description = "Приготовлю твое любимое блюдо", Emoji = "🍽️", UserId = 2, Category = "Забота", CreatedAt = DateTime.UtcNow },
                new ServiceOffer { Id = 6, Name = "Прогулка под звездами", Description = "Романтическая прогулка в красивом месте", Emoji = "🌟", UserId = 1, Category = "Романтика", CreatedAt = DateTime.UtcNow }
            );
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
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

    public class ServiceOffer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public int UserId { get; set; } // Кто предлагает услугу
        public User? User { get; set; }
        public bool IsActive { get; set; } = true; // Доступна ли услуга для обмена
        public string Category { get; set; } = "Романтика"; // Категория услуги
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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

    public class ServiceExchange
    {
        public int Id { get; set; }
        public int RequesterId { get; set; } // Кто запросил услугу
        public User? Requester { get; set; }
        public int ProviderId { get; set; } // Кто предоставляет услугу
        public User? Provider { get; set; }
        public int RequestedServiceId { get; set; } // Запрашиваемая услуга
        public ServiceOffer? RequestedService { get; set; }
        public int? OfferedServiceId { get; set; } // Предлагаемая в обмен услуга
        public ServiceOffer? OfferedService { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ScheduledDate { get; set; } // Когда планируется исполнение
        public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;
        public string? RequestMessage { get; set; } // Сообщение при запросе
        public string? ResponseMessage { get; set; } // Ответ от предоставляющего
        public DateTime? CompletedDate { get; set; }
        public int? Rating { get; set; } // Оценка 1-5 после исполнения
        public string? Review { get; set; } // Отзыв
    }

    public enum ExchangeStatus
    {
        Pending,    // Ожидает подтверждения
        Accepted,   // Принято, ожидает исполнения
        Completed,  // Исполнено
        Declined,   // Отклонено
        Cancelled   // Отменено
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