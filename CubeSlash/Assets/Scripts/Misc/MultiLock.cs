using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiLock
{
    public bool IsLocked { get { return locks.Count > 0; } }
    public bool IsFree { get { return locks.Count == 0; } }

    private List<string> locks = new List<string>();

    public void AddLock(string id)
    {
        if (!locks.Contains(id))
        {
            locks.Add(id);
        }
    }

    public void RemoveLock(string id)
    {
        if (locks.Contains(id))
        {
            locks.Remove(id);
        }
    }
}
