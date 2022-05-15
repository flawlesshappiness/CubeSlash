using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private UIHealthPoint prefab_hp;

    private List<UIHealthPoint> points = new List<UIHealthPoint>();

    private void Start()
    {
        // Initialize points
        prefab_hp.gameObject.SetActive(true);
        for (int i = 0; i < Player.Instance.Health.Max; i++)
        {
            var inst = Instantiate(prefab_hp.gameObject, prefab_hp.transform.parent).GetComponentInParent<UIHealthPoint>();
            points.Add(inst);
        }
        prefab_hp.gameObject.SetActive(false);

        // Set events
        Player.Instance.Health.onValueChanged += OnHealthChanged;

        // Finalize
        UpdateHealth();
    }

    private void OnDestroy()
    {
        Player.Instance.Health.onValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged()
    {
        UpdateHealth();
    }

    private void UpdateHealth()
    {
        for (int i = 0; i < points.Count; i++)
        {
            var hp = points[i];
            hp.Full = i <= Player.Instance.Health.Value - 1;
        }
    }
}
