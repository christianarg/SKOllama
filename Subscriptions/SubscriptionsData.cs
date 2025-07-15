using Microsoft.EntityFrameworkCore;

namespace Subscriptions
{
    public class Subscription
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public Commitment Commitment { get; set; }

        // Foreign key and navigation
        public string CatalogProductId { get; set; } = string.Empty;
        public CatalogProduct? CatalogProduct { get; set; }

        // Optional: Link to Sku (if needed)
        public string? SkuId { get; set; }
        public Sku? Sku { get; set; }
    }

    public class Sku
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int AvailableUnits { get; set; }
        public int AssignedUnits { get; set; }

        // Foreign key and navigation
        public string CatalogProductId { get; set; } = string.Empty;
        public CatalogProduct? CatalogProduct { get; set; }

        // Navigation: A Sku can have many Subscriptions
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }

    public class CatalogProduct
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal MontlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }

        // Navigation
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Sku> Skus { get; set; } = new List<Sku>();
    }

    public enum BillingCycle
    {
        Monthly,
        Annual
    }

    public enum Commitment
    {
        Monthly,
        Annual
    }

    public class SubscriptionsDbContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<CatalogProduct> CatalogProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=subscriptions.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Subscription -> CatalogProduct (many-to-one)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.CatalogProduct)
                .WithMany(cp => cp.Subscriptions)
                .HasForeignKey(s => s.CatalogProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sku -> CatalogProduct (many-to-one)
            modelBuilder.Entity<Sku>()
                .HasOne(s => s.CatalogProduct)
                .WithMany(cp => cp.Skus)
                .HasForeignKey(s => s.CatalogProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sku -> Subscriptions (one-to-many, optional)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Sku)
                .WithMany(sku => sku.Subscriptions)
                .HasForeignKey(s => s.SkuId)
                .OnDelete(DeleteBehavior.SetNull);

            // Set string as key for all entities
            modelBuilder.Entity<Subscription>().HasKey(s => s.Id);
            modelBuilder.Entity<Sku>().HasKey(s => s.Id);
            modelBuilder.Entity<CatalogProduct>().HasKey(cp => cp.Id);
        }
    }
}

