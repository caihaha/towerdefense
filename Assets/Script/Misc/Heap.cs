using System;
using System.Collections.Generic;

/*
public class Heap<T>
{
    #region 数据成员
    private int _capacity;
    private int _size;
    private T[] _items;
    #endregion

    #region 对外接口
    public bool Enqueue(T value)
    {
        if(_capacity == _size)
        {
            ResizeItemStore(_size * 2);
        }

        _items[_count++] = value;
        int position = BubbleUp(_count - 1);

        return (position == 0);
    }

    public T Dequeue()
    {
        return Dequeue(true);
    }
    #endregion

    #region 内部接口
    void ReizeItemStore(int newSize)
    {
        if(_count >= newSize)
        {
            return;
        }

        T[] temp = new T[newSize];
        Array.Copy(_items, 0, temp, 0, _count);
        _items = temp;
    }

    T Dequeue(bool shrink)
    {
        if(_count == 0)
        {
            throw new InvalidOperationException();
        }

        T result = _items[0];
        if(_count == 1)
        {
            _count = 0;
            _items[0] = default(T);
        }
        else
        {

        }
    }
    #endregion
}
*/
