namespace MHEventBus.Tests;

[EventBusSubscriber("TEST")]
public class StaticEventSelf
{
    static EventBus eventBus;
    [SetUp]
    public void Setup()
    {
        eventBus = new EventBus("TEST");
        eventBus.StartUp();
    }

    [Test]
    public void FireStaticEvent()
    {
        eventBus.PushEvent(new TestEvent());
        Task.Delay(2000).Wait();
        if (!received)
        {
            Assert.Fail();
        }
    }

    static bool received = false;
    
    [SubscribeEvent]
    public static void onEvent(TestEvent testEvent)
    {
        received = true;
        Assert.Pass();
    }
}

public class TestEvent : Event
{
    public int V;
    public TestEvent()
    {
        
    }

    public TestEvent(int i)
    {
        V = i;
    }
}