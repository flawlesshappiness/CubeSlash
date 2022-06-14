using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiLock
{
    public bool IsLocked { get { return locks.Count > 0; } }
    public bool IsFree { get { return locks.Count == 0; } }

    public System.Action<bool> OnLockChanged { get; set; }

    private List<string> locks = new List<string>();

    public void AddLock(string id)
    {
        if (!locks.Contains(id))
        {
            locks.Add(id);
            OnLockChanged?.Invoke(IsLocked);
        }
    }

    public void RemoveLock(string id)
    {
        if (locks.Contains(id))
        {
            locks.Remove(id);
            OnLockChanged?.Invoke(IsLocked);
        }
    }

    public void ToggleLock(string id)
    {
        if (locks.Contains(id))
        {
            RemoveLock(id);
        }
        else
        {
            AddLock(id);
        }
    }
}
