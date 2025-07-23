using Microsoft.EntityFrameworkCore;
using Dapper;
using Newtonsoft.Json;

namespace Subscriptions;

public class SubscriptinTestQueries
{
    /// <summary>
    /// Shows the contents of the Subscriptions database.
    /// </summary>
    public static void Run()
    {
        using var context = new SubscriptionsDbContext();
        // Subscriptions table
        var subscriptions = context.Subscriptions.ToList();
        Console.WriteLine("### Subscriptions");
        Console.WriteLine("| Product | Billing Cycle | Unit Price | Quantity | Commitment | CommitmentEndDate | AutoRenewEnabled | ID |");
        Console.WriteLine("|---------|--------------|------------|----------|------------|-------------------|------------------|----|");
        foreach (var subscription in subscriptions)
        {
            Console.WriteLine($"| {subscription.Name} | {subscription.BillingCycle} | {subscription.UnitPrice} | {subscription.Quantity} | {subscription.Commitment} | {subscription.CommitmentEndDate} | {subscription.AutoRenewEnabled} | {subscription.Id} |");
        }

        // Products table
        var products = context.CatalogProducts.ToList();
        Console.WriteLine("\n### CatalogProducts");
        Console.WriteLine("| Name | Monthly Price | Annual Price | ID |");
        Console.WriteLine("|------|--------------|--------------|----|");
        foreach (var product in products)
        {
            Console.WriteLine($"| {product.Name} | {product.MontlyPrice} | {product.AnnualPrice} | {product.Id} |");
        }

        // SKUs table
        var skus = context.Skus.ToList();
        Console.WriteLine("\n### Skus");
        Console.WriteLine("| Name | Available Units | Assigned Units | ID |");
        Console.WriteLine("|------|----------------|---------------|----|");
        foreach (var sku in skus)
        {
            Console.WriteLine($"| {sku.Name} | {sku.AvailableUnits} | {sku.AssignedUnits} | {sku.Id} |");
        }
    }

    public static async Task RunWithDapper()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=subscriptions.db");
        connection.Open();
        var sql = "SELECT * FROM Subscriptions";
        var subscriptions = await connection.QueryAsync(sql);
        Console.WriteLine("Subscriptions:");
        Console.WriteLine(JsonConvert.SerializeObject(subscriptions, Formatting.Indented));
    }

    public static void DescribeDatabase()
        => Console.WriteLine(SubscriptionAgentHelper.DescribeDatabase());
}