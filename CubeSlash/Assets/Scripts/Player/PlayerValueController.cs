using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerValueController : Singleton
{
    public static PlayerValueController Instance { get { return Instance<PlayerValueController>(); } }
    private StatValueCollection stats = new StatValueCollection();

    public event System.Action onValuesUpdated;

    protected override void Initialize()
    {
        base.Initialize();
        ResetValues();
    }

    public void ResetValues()
    {
        stats.ResetValues();
    }

    public void UpdateValues()
    {
        ResetValues();
        UpdateUpgradeValues();
        onValuesUpdated?.Invoke();
    }

    private void UpdateUpgradeValues()
    {
        foreach(var info in UpgradeController.Instance.GetUnlockedUpgrades())
        {
            stats.ApplyUpgrade(info.upgrade);
        }
    }

    public int GetIntValue(StatID id) => stats.GetIntValue(id);
    public float GetFloatValue(StatID id) => stats.GetFloatValue(id);
    public bool GetBoolValue(StatID id) => stats.GetBoolValue(id);
}
