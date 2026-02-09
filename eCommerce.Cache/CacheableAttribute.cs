namespace eCommerce.Cache;

[AttributeUsage(AttributeTargets.Method)]
public class CacheableAttribute : Attribute
{
    public int Seconds { get; set; } = 30;
    public bool Revoke { get; set; }
}