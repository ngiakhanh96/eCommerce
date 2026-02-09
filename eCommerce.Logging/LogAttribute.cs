namespace eCommerce.Logging;

[AttributeUsage(AttributeTargets.Method)]
public class LogAttribute : Attribute
{
    public string? CustomMethodLogName { get; set; }
}