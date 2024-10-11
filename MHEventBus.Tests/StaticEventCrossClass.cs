namespace MHEventBus.Tests;

[EventBusSubscriber("TESTCROSS1")]
public class StaticEventCrossClass
{
    static EventBus eventBus1;
    static EventBus eventBus2;
    
    [SetUp]
    public void SetUp()
    {
        eventBus1 = new EventBus("TESTCROSS1");
        eventBus2 = new EventBus("TESTCROSS2");
        eventBus1.StartUp();
        eventBus2.StartUp();
    }

    [Test]
    public void ClassToClass()
    {
        eventBus2.PushEvent(new TestEvent(0)); //should pass
        if (!StaticEventCrossClass2.t1)
        {
            Assert.Fail();
        }
    }

    [Test]
    public void EventToClass()
    {
        eventBus1.PushEvent(new TestEvent(1));
        if (!StaticEventCrossClass2.t2)
        {
            Assert.Fail();
        }
    }


    [SubscribeEvent]
    public static void onEvent(TestEvent testEvent)
    {
        switch (testEvent.V)
        {
            case 1:
            {
                eventBus2.PushEvent(new TestEvent(1));
                break;
            }
        }
    }
}

[EventBusSubscriber("TESTCROSS2")]
public class StaticEventCrossClass2
{
    public static bool t1 = false;
    public static bool t2 = false;
    [SubscribeEvent]
    public static void onEvent(TestEvent testEvent)
    {
        switch (testEvent.V)
        {
            case 0:
            {
                //1 class to another
                t1 = true;
                Assert.Pass();
                break;
            }
            case 1:
            {
                t2 = true;
                Assert.Pass();
                break;
            }
        }
    }
}