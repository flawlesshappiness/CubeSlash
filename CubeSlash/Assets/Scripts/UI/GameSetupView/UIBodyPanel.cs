using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBodyPanel : MonoBehaviour
{
    [SerializeField] private Image img_body;
    [SerializeField] private GameObject template_health, template_armor;
    [SerializeField] private GameObject template_spd, template_acc;

    private List<GameObject> health_points = new List<GameObject>();
    private List<GameObject> spd_points = new List<GameObject>();
    private List<GameObject> acc_points = new List<GameObject>();

    private void Start()
    {
        template_health.SetActive(false);
        template_armor.SetActive(false);
        template_spd.SetActive(false);
        template_acc.SetActive(false);
    }

    public void SetSettings(PlayerBodySettings settings)
    {
        // Lock
        var is_locked = !settings.IsUnlocked();

        // Sprite
        img_body.sprite = settings.body_sprite;
        img_body.enabled = !is_locked;

        // Stats
        ClearHealthPoints();
        ClearSpeedPoints();
        ClearAccelerationPoints();

        AddHealthPoints(settings.health);
        AddArmorPoints(settings.armor);

        var velocity_min = 4;
        var velocity = settings.linear_velocity - velocity_min;
        var velocity_points = (int)(velocity / 0.5f);
        AddSpeedPoints(velocity_points);

        var acc_min = 10;
        var acc = settings.linear_acceleration - acc_min;
        var acc_points = (int)(acc / 2f);
        AddAccelerationPoints(acc_points);
    }

    private void ClearHealthPoints()
    {
        health_points.ForEach(point => Destroy(point));
        health_points.Clear();
    }

    private void AddHealthPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_health, template_health.transform.parent);
            inst.gameObject.SetActive(true);
            health_points.Add(inst);
        }
    }

    private void AddArmorPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_armor, template_armor.transform.parent);
            inst.gameObject.SetActive(true);
            health_points.Add(inst);
        }
    }

    private void ClearSpeedPoints()
    {
        spd_points.ForEach(point => Destroy(point));
        spd_points.Clear();
    }

    private void ClearAccelerationPoints()
    {
        acc_points.ForEach(point => Destroy(point));
        acc_points.Clear();
    }

    private void AddSpeedPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_spd, template_spd.transform.parent);
            inst.gameObject.SetActive(true);
            spd_points.Add(inst);
        }
    }
    private void AddAccelerationPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_acc, template_acc.transform.parent);
            inst.gameObject.SetActive(true);
            acc_points.Add(inst);
        }
    }
}