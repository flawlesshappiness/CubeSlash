using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class VignetteGlow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private Transform pivot;

    private float time_offset_scale;
    private float time_offset_alpha;
    private float speed_scale;
    private float size_min;
    private float size_max;
    private float alpha_min;
    private float alpha_max;

    private float current_size;
    private float current_alpha;
    private float master_alpha;
    private Color current_color;

    private void Start()
    {
        time_offset_scale = Random.Range(0f, Mathf.PI * 2);
        time_offset_alpha = Random.Range(0f, Mathf.PI * 2);
        speed_scale = Random.Range(0.05f, 0.5f);

        size_min = Random.Range(0.4f, 0.6f);
        size_max = Random.Range(1.0f, 2.5f);

        alpha_min = Random.Range(0f, 0.05f);
        alpha_max = Random.Range(0.05f, 0.1f);
    }

    public void FadeColor(Color end)
    {
        current_color = end;
        this.StartCoroutineWithID(Cr(), "color_" + GetInstanceID());
        IEnumerator Cr()
        {
            var start = spr.color.SetA(1);
            yield return LerpEnumerator.Value(2f, f =>
            {
                var color = Color.Lerp(start, end, f).SetA(current_color.a * current_alpha * master_alpha);
                spr.SetColor(color);
            });
        }
    }

    public void FadeAlpha(float duration, float end)
    {
        this.StartCoroutineWithID(Cr(), "master_alpha_" + GetInstanceID());
        IEnumerator Cr()
        {
            var start = master_alpha;
            yield return LerpEnumerator.Value(duration, f =>
            {
                master_alpha = Mathf.Lerp(start, end, f);
            });
        }
    }

    private void Update()
    {
        var sin_scale = (Mathf.Sin((Time.time * speed_scale) + time_offset_scale) * 0.5f) + 0.5f;
        current_size = Mathf.Lerp(size_min, size_max, sin_scale);
        pivot.localScale = Vector3.one * current_size;

        var sin_alpha = (Mathf.Sin((Time.time * speed_scale) + time_offset_alpha) * 0.5f) + 0.5f;
        current_alpha = Mathf.Lerp(alpha_min, alpha_max, sin_alpha);
        spr.SetAlpha(current_color.a * current_alpha * master_alpha);
    }
}