using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;

public class RebindSaver : MonoBehaviour
{
    protected void OnEnable()
    {
        var rebindUIComponents = transform.GetComponentsInChildren<RebindActionUI>();
        foreach (var component in rebindUIComponents)
        {
            component.stopRebindEvent.AddListener(StopRebind);
            component.resetEvent.AddListener(ResetBinding);
        }
    }

    private void ResetBinding(RebindActionUI control, InputAction action)
    {
        RebindData.Data.Remove(action);
        RebindData.Data.Save();
    }

    private void StopRebind(RebindActionUI control, InputActionRebindingExtensions.RebindingOperation operation)
    {
        if (operation.canceled) return;

        var candidate = operation.candidates.FirstOrDefault();
        if (candidate == null) return;

        RebindData.Data.Add(operation.action, candidate.path);
        RebindData.Data.Save();
    }
}

public class RebindData
{
    public static RebindData Data => _data ??= Load();
    private static RebindData _data;

    public Dictionary<Guid, string> Rebinds { get; set; } = new Dictionary<Guid, string>();

    private const string key_rebinds = "bindings";

    public void Add(InputAction action, string path)
    {
        if (Rebinds.ContainsKey(action.id))
        {
            Rebinds[action.id] = path;
        }
        else
        {
            Rebinds.Add(action.id, path);
        }
    }

    public void Remove(InputAction action)
    {
        if (Rebinds.ContainsKey(action.id))
        {
            Rebinds.Remove(action.id);
        }
    }

    public void Save()
    {
        PlayerPrefs.SetString(key_rebinds, ToJson());
    }

    private static RebindData Load()
    {
        try
        {
            var json = PlayerPrefs.GetString(key_rebinds);
            if (string.IsNullOrEmpty(json)) return new RebindData();
            return FromJson(json);
        }
        catch
        {
            return new RebindData();
        }
    }

    public string ToJson()
    {
        try
        {
            return JsonConvert.SerializeObject(this);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static RebindData FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<RebindData>(json);
        }
        catch
        {
            return new RebindData();
        }
    }
}