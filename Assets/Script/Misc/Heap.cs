using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    IComparer<T> comparer;
    T[] heap;

    public int Count { get; private set; }
    public T[] Elements { get => heap; }

    public PriorityQueue() : this(null) { }
    public PriorityQueue(int capacity) : this(capacity, null) { }
    public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }
    public PriorityQueue(int capacity, IComparer<T> comparer)
    {
        this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
        this.heap = new T[capacity];
    }

    public void Push(T v)
    {
        if (Count >= heap.Length)
        {
            Array.Resize(ref heap, Count << 1);
        }
        heap[Count] = v;
        SiftUp(Count++);
    }

    public T Pop()
    {
        T v = Top();
        heap[0] = heap[--Count];
        if (Count > 0)
        {
            SiftDown(0);
        }
        return v;
    }

    public T Top()
    {
        return Count > 0 ? heap[Count - 1] : throw new InvalidOperationException("Priority is null");
    }

    private void SiftUp(int n)
    {
        T v = heap[n];
        for (int i = n / 2; n > 0 && comparer.Compare(v, heap[i]) > 0; n = i, i >>= 2)
        { 
            heap[n] = heap[i];
        }

        heap[n] = v;
    }

    private void SiftDown(int n)
    {
        T v = heap[n];
        for (int i = n * 2; i < Count; n = i, i <<= 1)
        {
            if (i + 1 < Count && comparer.Compare(heap[i + 1], heap[i]) > 0)
            {
                ++i;
            }
            if (comparer.Compare(v, heap[i]) >= 0)
            {
                break;
            }
            heap[n] = heap[i];
        }
        heap[n] = v;
    }

    public void Clear()
    {
        Array.Clear(heap, 0, Count);
        Count = 0;
    }
}