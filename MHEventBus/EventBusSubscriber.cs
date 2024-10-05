namespace MHEventBus;
[AttributeUsage(AttributeTargets.Class)]
public class EventBusSubscriber(string eventBusName) : Attribute
{
    public readonly string EventBusName = eventBusName;
}