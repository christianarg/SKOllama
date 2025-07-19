using Microsoft.EntityFrameworkCore;

namespace Subscriptions;

public class SubscriptinTestQueries
{
    public static void Run()
    {
        using var context = new SubscriptionsDbContext();
        // Example: Get all subscriptions
        var subscriptions = context.Subscriptions.ToList();
        foreach (var subscription in subscriptions)
        {
            Console.WriteLine($"Subscription ID: {subscription.Id}, Product: {subscription.Name}, Billing Cycle: {subscription.BillingCycle}, Unit Price: {subscription.UnitPrice}, Quantity: {subscription.Quantity}, Quantity: {subscription.Quantity}, Commitment: {subscription.Commitment}");
        }
        // Example: Get all products
        var products = context.CatalogProducts.ToList();
        foreach (var product in products)
        {
            Console.WriteLine($"Product ID: {product.Id}, Name: {product.Name}, Monthly Price: {product.MontlyPrice}, Annual Price: {product.AnnualPrice}");
        }
        // Example: Get all SKUs
        var skus = context.Skus.ToList();
        foreach (var sku in skus)
        {
            Console.WriteLine($"SKU ID: {sku.Id}, Name: {sku.Name}, Available Units: {sku.AvailableUnits}, Assigned Units: {sku.AssignedUnits}");
        }
    }

    public static void DescribeDatabase()
        => Console.WriteLine(SubscriptionAgentHelper.DescribeDatabase());
}