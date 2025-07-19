namespace Subscriptions;


public class SubscriptionsAgent
{
    public static async Task Run()
    {

    }
}

public class SubscriptionAgentHelper
{
    public static string DescribeDatabase()
    {
        using var context = new SubscriptionsDbContext();
        var model = context.Model;
        var sb = new System.Text.StringBuilder();
        foreach (var entityType in model.GetEntityTypes())
        {
            sb.AppendLine($"Entity: {entityType.Name}");
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType.IsEnum)
                {
                    var enumValues = Enum.GetNames(property.ClrType);
                    sb.AppendLine($"  Property: {property.Name}, Type: {property.ClrType.Name}, Values: {string.Join(";", enumValues)}");
                }
                else
                {
                    sb.AppendLine($"  Property: {property.Name}, Type: {property.ClrType.Name}");
                }
            }
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                sb.AppendLine($"  Foreign Key: {string.Join(", ", foreignKey.Properties.Select(p => p.Name))} -> {foreignKey.PrincipalEntityType.Name}");
            }
        }
        return sb.ToString();
    }
}