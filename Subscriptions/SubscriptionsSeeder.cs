using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Subscriptions
{
    public static class SubscriptionsSeeder
    {
        public static void Seed(SubscriptionsDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.CatalogProducts.Any())
                return; // Already seeded

            // Catalog Products
            var catalogProducts = new List<CatalogProduct>
            {
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Office 365 E1", MontlyPrice = 8.00m, AnnualPrice = 96.00m },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Office 365 E3", MontlyPrice = 20.00m, AnnualPrice = 240.00m },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Office 365 E5", MontlyPrice = 35.00m, AnnualPrice = 420.00m },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 E3", MontlyPrice = 32.00m, AnnualPrice = 384.00m },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 E5", MontlyPrice = 57.00m, AnnualPrice = 684.00m },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 Business Basic", MontlyPrice = 6.00m, AnnualPrice = 72.00m },
                new CatalogProduct { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 Business Standard", MontlyPrice = 12.50m, AnnualPrice = 150.00m },
            };
            context.CatalogProducts.AddRange(catalogProducts);
            context.SaveChanges();

            // Helper to get product Id by name
            string GetProductId(string name) => catalogProducts.First(p => p.Name == name).Id;

            // Skus
            var skus = new List<Sku>
            {
                new Sku { Id = Guid.NewGuid().ToString(), Name = "Office 365 E3", AvailableUnits = 100, AssignedUnits = 60, CatalogProductId = GetProductId("Office 365 E3") },
                new Sku { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 E3", AvailableUnits = 80, AssignedUnits = 50, CatalogProductId = GetProductId("Microsoft 365 E3") },
                new Sku { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 E5", AvailableUnits = 40, AssignedUnits = 25, CatalogProductId = GetProductId("Microsoft 365 E5") },
                new Sku { Id = Guid.NewGuid().ToString(), Name = "Microsoft 365 Business Standard", AvailableUnits = 60, AssignedUnits = 30, CatalogProductId = GetProductId("Microsoft 365 Business Standard") },
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
                    UnitPrice = 240.00m,
                    Quantity = 30,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = GetProductId("Office 365 E3"),
                    SkuId = GetSkuId("Office 365 E3")
                },
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Office 365 E3 Monthly",
                    UnitPrice = 20.00m,
                    Quantity = 10,
                    BillingCycle = BillingCycle.Monthly,
                    Commitment = Commitment.Monthly,
                    CatalogProductId = GetProductId("Office 365 E3"),
                    SkuId = GetSkuId("Office 365 E3")
                },
                // Microsoft 365 E3: One annual
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Microsoft 365 E3 Annual",
                    UnitPrice = 384.00m,
                    Quantity = 20,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = GetProductId("Microsoft 365 E3"),
                    SkuId = GetSkuId("Microsoft 365 E3")
                },
                // Microsoft 365 E5: One annual
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Microsoft 365 E5 Annual",
                    UnitPrice = 684.00m,
                    Quantity = 10,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = GetProductId("Microsoft 365 E5"),
                    SkuId = GetSkuId("Microsoft 365 E5")
                },
                // Microsoft 365 Business Standard: One annual
                new Subscription {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Microsoft 365 Business Standard Annual",
                    UnitPrice = 150.00m,
                    Quantity = 15,
                    BillingCycle = BillingCycle.Annual,
                    Commitment = Commitment.Annual,
                    CatalogProductId = GetProductId("Microsoft 365 Business Standard"),
                    SkuId = GetSkuId("Microsoft 365 Business Standard")
                },
            };
            context.Subscriptions.AddRange(subscriptions);
            context.SaveChanges();
        }
    }
}
