using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private UIHealthPoint prefab_hp;

    private List<UIHealthPoint> points = new List<UIHealthPoint>();

    private void Start()
    {
        prefab_hp.gameObject.SetActive(false);
        ResetHealthPoints();
    }

    private void OnEnable()
    {
        Player.Instance.Health.onAddHealthPoint += OnAddHealthPoint;
    }

    private void OnDisable()
    {
        Player.Instance.Health.onAddHealthPoint -= OnAddHealthPoint;
    }

    private void Update()
    {
        if (Player.Instance.Health.ResetUI)
        {
            ResetHealthPoints();
        }
    }

    private void ResetHealthPoints()
    {
        // Clear
        foreach(var p in points)
        {
            Destroy(p.gameObject);
        }
        points.Clear();

        // Create
        Player.Instance.Health.GetHealthList(HealthPoint.Type.FULL).ForEach(hp => OnAddHealthPoint(hp));
        Player.Instance.Health.GetHealthList(HealthPoint.Type.EMPTY).ForEach(hp => OnAddHealthPoint(hp));
        Player.Instance.Health.GetHealthList(HealthPoint.Type.TEMPORARY).ForEach(hp => OnAddHealthPoint(hp));

        // Reset
        Player.Instance.Health.OnUIReset();
    }

    private UIHealthPoint CreateHealthPoint(HealthPoint hp)
    {
        var ui = Instantiate(prefab_hp, prefab_hp.transform.parent);
        ui.gameObject.SetActive(true);
        ui.Initialize(this, hp);
        return ui;
    }

    public void RemoveHealthPoint(UIHealthPoint ui)
    {
        points.Remove(ui);
    }

    private void OnAddHealthPoint(HealthPoint hp)
    {
        var ui = CreateHealthPoint(hp);
        var idx = 0;

        if(hp.HealthType == HealthPoint.Type.FULL)
        {
            var target = points.LastOrDefault(p => p.Type == HealthPoint.Type.FULL);
            idx = target == null ? 0 : points.IndexOf(target) + 1;
        }
        else if(hp.HealthType == HealthPoint.Type.EMPTY)
        {
            var target = points.LastOrDefault(p => p.Type == HealthPoint.Type.EMPTY) ?? points.LastOrDefault(p => p.Type == HealthPoint.Type.FULL);
            idx = target == null ? 0 : points.IndexOf(target) + 1;
        }
        else if(hp.HealthType == HealthPoint.Type.TEMPORARY)
        {
            idx = points.Count;
        }

        ui.transform.SetSiblingIndex(idx);
        points.Insert(idx, ui);
    }
}
