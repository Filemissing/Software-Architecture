using System;

public class EventBus<T>
{
    public static event Action<T> onEvent;

    public static void Publish(T eventData)
    {
        onEvent?.Invoke(eventData);
    }
}
