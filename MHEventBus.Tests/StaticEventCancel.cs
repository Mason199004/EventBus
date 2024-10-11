namespace MHEventBus.Tests;

[EventBusSubscriber("CANCEL")]
public class StaticEventCancel
{
    static EventBus eventBus;
    [SetUp]
    public void SetUp()
    {
        eventBus = new EventBus("CANCEL");
        eventBus.StartUp();
    }

    [Test]
    public void StaticCancelTest()
    {
        eventBus.PushEvent(new TestEvent()); 
        Task.Delay(2000).Wait();
        Assert.Pass();
    }

    [SubscribeEvent(Priority = Priority.HIGHEST)]
    public static void Handler1(TestEvent testEvent)
    {
        testEvent.CancelEvent();
    }

    [SubscribeEvent]
    public static void Handler2(TestEvent testEvent)
    {
        Assert.Fail();
    }
}