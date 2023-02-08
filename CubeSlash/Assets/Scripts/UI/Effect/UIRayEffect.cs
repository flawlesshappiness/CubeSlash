using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRayEffect : MonoBehaviour
{
    [SerializeField] private RectTransform template_ray;
    public int count;
    public AnimationCurve curve_size;
    public Gradient gradient;

    private List<RectTransform> rays = new List<RectTransform>();

    private void Start()
    {
        template_ray.gameObject.SetActive(false);
        for (int i = 0; i < count; i++)
        {
            var t = (float)i / count;
            var ray = Instantiate(template_ray, template_ray.transform.parent);
            ray.gameObject.SetActive(true);
            var angle = Mathf.Lerp(0, 360, t);
            ray.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            rays.Add(ray);
            this.StartCoroutineWithID(AnimateRayCr(ray), ray);
        }
    }

    IEnumerator AnimateRayCr(RectTransform ray)
    {
        var duration = Random.Range(2f, 8f);
        var img = ray.GetComponentInChildren<Image>();
        img.color = gradient.Evaluate(Random.Range(0f, 1f));
        while (true)
        {
            yield return LerpEnumerator.Value(duration, f =>
            {
                var y = curve_size.Evaluate(f);
                ray.localScale = ray.localScale.SetY(y);
            }).UnscaledTime();
        }
    }
}