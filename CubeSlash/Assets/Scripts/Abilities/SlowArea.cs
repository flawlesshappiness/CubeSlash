using Flawliz.Lerp;
using System.Collections;
using System.Linq;
using UnityEngine;

public class SlowArea : MonoBehaviour
{
    [SerializeField] private Transform pivot_sprite;

    private bool _active = true;
    private float _radius;
    private float _slow_perc;

    public static SlowArea Create()
    {
        var inst = Instantiate(Resources.Load<SlowArea>("Prefabs/Abilities/Objects/SlowArea"));
        return inst;
    }

    private void Update()
    {
        if (!_active) return;
        SlowEnemiesInArea(transform.position, _radius, _slow_perc);
    }

    public void SetRadius(float radius)
    {
        _radius = radius;
        pivot_sprite.localScale = Vector3.one * radius;
    }

    public void SetSlowPercentage(float slow_perc)
    {
        _slow_perc = slow_perc;
    }

    public void Destroy(float delay)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            StopParticleSystems(Mathf.Clamp(delay - 4, 0, delay));
            yield return new WaitForSeconds(delay);
            _active = false;
            FadeOut(4f);
            yield return new WaitForSeconds(4f);
            Destroy(gameObject);
        }
    }

    private void FadeOut(float duration)
    {
        var sprs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spr in sprs)
        {
            StartCoroutine(FadeOutSpr(spr));
        }

        IEnumerator FadeOutSpr(SpriteRenderer spr)
        {
            yield return LerpEnumerator.Alpha(spr, duration, 0f);
        }
    }

    private void StopParticleSystems(float delay)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);

            var pss = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss)
            {
                ps.transform.parent = null;
                ps.SetEmissionEnabled(false);
            }
        }
    }

    public static void SlowEnemiesInArea(Vector3 position, float radius, float slow_perc)
    {
        var enemies = Physics2D.OverlapCircleAll(position, radius)
            .Select(hit => hit.GetComponentInParent<Enemy>())
            .Where(enemy => enemy != null)
            .Distinct();

        foreach (var enemy in enemies)
        {
            enemy.Rigidbody.velocity = Vector3.ClampMagnitude(enemy.Rigidbody.velocity, enemy.LinearVelocity * slow_perc);
        }
    }
}