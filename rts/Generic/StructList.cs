using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


public class StructList <T> where T : struct
{
    public T[] Array;
    public int Count;
    int currentCapacity;

    public StructList(int initialCapacity = 10)
    {
        currentCapacity = initialCapacity;
        Array = new T[initialCapacity];
        Count = 0;
    }

    private void Grow()
    {
        currentCapacity *= 2;
        var newMem = new T[currentCapacity];
        Array.CopyTo(newMem, 0);
        Array = newMem;
    }

    public void Add(ref T add)
    {
        if (Count == currentCapacity)
            Grow();
        Array[Count++] = add;
    }

    public void RemoveAt(int index)
    {
        for(int i = index+1; i < Count; i++)
        {
            Array[i - 1] = Array[i];
        }
        Count--;
    }

    // TODO: optimize
    public void RemoveAll(Func<T, bool> filter)
    {
        for(int i = 0; i < Count; i++)
        {
            if(filter(Array[i]))
            {
                RemoveAt(i);
                i--; // we removed the current, so we have to process the current index again
            }
        }
    }
}
