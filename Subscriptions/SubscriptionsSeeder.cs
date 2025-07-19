using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Subscriptions
{
    public static class SubscriptionsSeeder
    {
        /// <summary>
        /// Initializes the SQLite database at 'database.db' and seeds it with initial data.
        /// </summary>
        public static void InitDb()
        {
            Console.WriteLine("Initializing database...");
            using var context = new SubscriptionsDbContext();
            Seed(context);
            Console.WriteLine("Database initialized and seeded successfully.");
        }

        public static void Seed(SubscriptionsDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.CatalogProducts.Any())
                return; // Already seeded

            // Catalog Products
            var catalogProducts = new List<CatalogProduct>
            {
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Office 365 E1", MontlyPrice = 8.00, AnnualPrice = 96.00 },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Office 365 E3", MontlyPrice = 20.00, AnnualPrice = 240.00 },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Office 365 E5", MontlyPrice = 35.00, AnnualPrice = 420.00 },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 E3", MontlyPrice = 32.00, AnnualPrice = 384.00 },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 E5", MontlyPrice = 57.00, AnnualPrice = 684.00 },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 Business Basic", MontlyPrice = 6.00, AnnualPrice = 72.00 },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 Business Standard", MontlyPrice = 12.50, AnnualPrice = 150.00 },
            };
            context.CatalogProducts.AddRange(catalogProducts);
            context.SaveChanges();

            // Helper to get product by name
            CatalogProduct GetProduct(string name) => catalogProducts.First(p => p.Name == name);
            var office365E3CatalogProduct = GetProduct("Office 365 E3");
            var m365E3CatalogProduct = GetProduct("Microsoft 365 E3");
            var m365E5CatalogProduct = GetProduct("Microsoft 365 E5");
            var m365BussinessStandardCatalogProduct = GetProduct("Microsoft 365 Business Standard");

            // Skus
            var skus = new List<Sku>
            {
                new Sku { Id = Guid.NewGuid().ToString(), Name = office365E3CatalogProduct.Name, AvailableUnits = 100, AssignedUnits = 60, CatalogProductId = office365E3CatalogProduct.Id },
                new Sku { Id = Guid.NewGuid().ToString(), Name = m365E3CatalogProduct.Name, AvailableUnits = 80, AssignedUnits = 50, CatalogProductId = m365E3CatalogProduct.Id },
                new Sku { Id = Guid.NewGuid().ToString(), Name = m365E5CatalogProduct.Name, AvailableUnits = 40, AssignedUnits = 25, CatalogProductId = m365E5CatalogProduct.Id },
                new Sku { Id = Guid.NewGuid().ToString(), Name = m365BussinessStandardCatalogProduct.Name, AvailableUnits = 60, AssignedUnits = 30, CatalogProductId = m365BussinessStandardCatalogProduct.Id },
            };
            context.Skus.AddRange(skus);
            context.SaveChanges();

            // Helper to get sku Id by name
            string? GetSkuId(string name) => skus.FirstOrDefault(s => s.Name == name)?.Id;

            // Subscriptions
            var subscriptions = new List<Subscription>
            {
                // Office 365 E3: One annual, one monthly
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Office 365 E3 Annual",
                    UnitPrice = office365E3CatalogProduct.AnnualPrice,
                    Quantity = 30,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = office365E3CatalogProduct.Id,
                    SkuId = GetSkuId("Office 365 E3")
                },
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Office 365 E3 Monthly",
                    UnitPrice = office365E3CatalogProduct.MontlyPrice,
                    Quantity = 10,
                    BillingCycle = BillingCycle.Monthly,
                    Commitment = Commitment.Monthly,
                    CatalogProductId = office365E3CatalogProduct.Id,
                    SkuId = GetSkuId("Office 365 E3")
                },
                // Microsoft 365 E3: One annual
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Microsoft 365 E3 Annual",
                    UnitPrice = m365E3CatalogProduct.AnnualPrice,
                    Quantity = 20,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = m365E3CatalogProduct.Id,
                    SkuId = GetSkuId("Microsoft 365 E3")
                },
                // Microsoft 365 E5: One annual
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Microsoft 365 E5 Annual",
                    UnitPrice = m365E5CatalogProduct.AnnualPrice,
                    Quantity = 10,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = m365E5CatalogProduct.Id,
                    SkuId = GetSkuId("Microsoft 365 E5")
                },
                // Microsoft 365 Business Standard: One annual
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Microsoft 365 Business Standard Annual",
                    UnitPrice = m365BussinessStandardCatalogProduct.AnnualPrice,
                    Quantity = 15,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = m365BussinessStandardCatalogProduct.Id,
                    SkuId = GetSkuId("Microsoft 365 Business Standard")
                },
            };
            context.Subscriptions.AddRange(subscriptions);
            context.SaveChanges();
        }
    }
}
