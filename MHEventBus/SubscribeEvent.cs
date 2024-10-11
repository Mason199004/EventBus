namespace MHEventBus;

[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEvent : Attribute
{
    public Priority Priority { get; set; } = Priority.NORMAL;
}
public enum Priority
{
    HIGHEST,
    HIGHER,
    HIGH,
    NORMAL,
    LOW,
    LOWER,
    LOWEST
}