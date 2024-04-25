using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RadialMenuElementSelector : MonoBehaviour
{
    private RadialMenuLayout layout;

    private List<RadialMenuElement> elements = new List<RadialMenuElement>();

    public List<RadialMenuElement> Elements { get { return elements.ToList(); } }

    private void Awake()
    {
        layout = GetComponentInChildren<RadialMenuLayout>();
    }

    private void Start()
    {
        UpdateElements();
    }

    public void Clear()
    {
        elements.ForEach(e =>
        {
            e.gameObject.SetActive(false);
            Destroy(e.gameObject);
        });
        elements.Clear();
    }

    public void UpdateElements()
    {
        elements = GetComponentsInChildren<RadialMenuElement>().ToList();
    }

    public RadialMenuElement GetElement(Vector2 direction) => GetElement(direction.ToVector3());

    public RadialMenuElement GetElement(Vector3 direction)
    {
        var angle = Vector3.SignedAngle(direction, Vector3.up, Vector3.forward);
        return GetElement(angle);
    }

    public RadialMenuElement GetElement(float angle)
    {
        angle += 180 - layout.AngleOffset;
        var arc = 360f;
        angle = angle < 0 ? angle + arc : angle;
        var count = elements.Count;
        var delta = arc / count;
        var deltah = delta * 0.5f;
        var t = (angle + deltah) / arc;
        var i = ((int)Mathf.LerpUnclamped(0, count, t)) % count;
        return elements[i];
    }
}