namespace MHEventBus.Tests;

public class ObjectEvents
{
    EventBus eventBus;
    private OtherObject ob = new();
    [SetUp]
    public void SetUp()
    {
        eventBus = new EventBus("ObjectEvents");
        eventBus.Register(this);
        eventBus.Register(ob);
        eventBus.StartUp();
    }

    [Test]
    public void SendToSelf()
    {
        eventBus.PushEvent(new TestEvent());
        if (!received)
        {
            Assert.Fail();
        }
        else
        {
            Assert.Pass();
        }
    }

    [Test]
    public void SendToObject()
    {
        eventBus.PushEvent(new TestEvent(1));
        if (received2)
        {
            Assert.Pass();
        }
        else
        {
            Assert.Fail();
        }
    }
    
    bool received = false;
    public static bool received2 = false;
    [SubscribeEvent]
    public void onEvent(TestEvent testEvent)
    {
        
        switch (testEvent.V)
        {
            case 0:
            {
                received = true;
                break;
            }
            case 1:
            {
                break;
            }
        }
    }
}

class OtherObject
{
    [SubscribeEvent]
    public void onEvent(TestEvent testEvent)
    {
        switch (testEvent.V)
        {
            case 1:
            {
                ObjectEvents.received2 = true;
                break;
            }
        }
    }
}