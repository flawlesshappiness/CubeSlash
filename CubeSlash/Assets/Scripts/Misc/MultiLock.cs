using System.Collections.Generic;

public class MultiLock
{
    public bool IsLocked { get; private set; }
    public bool IsFree { get; private set; }

    public System.Action<bool> OnLockChanged { get; set; }

    private List<string> locks = new List<string>();

    public MultiLock()
    {
        LockUpdated();
    }

    public void ClearLock()
    {
        locks.Clear();
        LockUpdated();
    }

    public void AddLock(string id)
    {
        if (!locks.Contains(id))
        {
            locks.Add(id);
            LockUpdated();
        }
    }

    public void RemoveLock(string id)
    {
        if (locks.Contains(id))
        {
            locks.Remove(id);
            LockUpdated();
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

    private void LockUpdated()
    {
        IsLocked = locks.Count > 0;
        IsFree = locks.Count == 0;
        OnLockChanged?.Invoke(IsLocked);
    }
}
