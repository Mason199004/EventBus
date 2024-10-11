namespace MHEventBus.Tests;

[EventBusSubscriber("TESTPRIO")]
public class StaticEventsWithPriority
{
    static EventBus eventBus;
    [SetUp]
    public void SetUp()
    {
        eventBus = new EventBus("TESTPRIO");
        eventBus.StartUp();
    }

    static bool ev1Called = false;

    [Test]
    public void StaticEventWithPriority()
    {
        eventBus.PushEvent(new TestEvent());
    }
    
    [SubscribeEvent]
    public static void Handler2(TestEvent testEvent)
    {
        if (ev1Called)
        {
            Assert.Pass();
        }
        else
        {
            Assert.Fail();
        }
    }
    
    [SubscribeEvent(Priority = Priority.HIGHEST)]
    public static void Handler1(TestEvent testEvent)
    {
        ev1Called = true;
    }
    
}