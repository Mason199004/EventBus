namespace MHEventBus;

public interface IEvent
{
    public bool IsCancelled { get; }

    public void CancelEvent();
}

public abstract class Event : IEvent
{
    private bool _isCancelled;
    public bool IsCancelled => _isCancelled;
    public virtual void CancelEvent()
    {
        _isCancelled = true;
    }
}