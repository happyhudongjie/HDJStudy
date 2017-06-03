using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T:class ,new()
{

    private Stack<T> _stack = new Stack<T>();

    public T New(string path,out bool isNew)
    {
        return (_stack.Count == 0) ? NewT(path,out isNew) : Pop(out isNew);
        
    }

    T NewT(string path,out bool isNew)
    {
        T obj = Resources.Load(path) as T;
        isNew = true;
        return obj;
    }

    T Pop(out bool isNew)
    {
        isNew = false;
        return _stack.Pop();
    }

    public void Store(T t)
    {
        Debug.Log("StoreCount: " + _stack.Count);
    }
	
}
