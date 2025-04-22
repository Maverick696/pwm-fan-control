namespace FanCommander.Models;

public class HistoryBuffer<T>
{
    private readonly Queue<T> _queue;
    public int Capacity { get; }

    public HistoryBuffer(int capacity)
    {
        Capacity = capacity;
        _queue = new Queue<T>(capacity);
    }

    public void Add(T value)
    {
        if (_queue.Count >= Capacity)
            _queue.Dequeue();
        _queue.Enqueue(value);
    }

    public IReadOnlyCollection<T> GetAll() => _queue.ToArray();
    public int Count => _queue.Count;
    public void Clear() => _queue.Clear();
}
