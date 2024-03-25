using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_RootTeleport : EnemyAI
{
    public Transform temp_roots;
    public ParticleSystem ps_dissolve;

    private List<Transform> active_roots = new List<Transform>();

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        Self.OnDeath += OnDeath;

        temp_roots.gameObject.SetActive(false);

        Self.Rigidbody.isKinematic = true;

        StartMoving();
    }

    private void OnDeath()
    {
        Self.Rigidbody.isKinematic = false;

        foreach (var root in active_roots)
        {
            DestroyRoot(root);
        }
        active_roots.Clear();
    }

    private Coroutine StartMoving()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var max_dist = 3f;
            Transform prev_root = null;
            while (true)
            {
                // Create next root
                var dir = GetOpenDirectionTowards(PlayerPosition);
                var next_position = transform.position + dir.normalized * max_dist;
                var next_root = CreateRoot();
                next_root.position = next_position;
                next_root.localScale = Vector3.zero;

                // Scale up next root
                ScaleUpRoot(next_root);

                yield return new WaitForSeconds(0.25f);

                // Scale down
                yield return LerpEnumerator.LocalScale(Self.transform, 0.5f, Vector3.zero)
                    .Curve(EasingCurves.EaseOutQuad);

                // Scale down previous root
                if (prev_root != null)
                {
                    ScaleDownAndDestroyRoot(prev_root);
                }

                prev_root = next_root;

                // Scale up at next root
                Self.transform.position = next_position;
                Self.transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);

                yield return LerpEnumerator.LocalScale(Self.transform, 0.5f, Vector3.one)
                    .Curve(EasingCurves.EaseOutQuad);
            }
        }
    }

    private Coroutine ScaleUpRoot(Transform root)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var angle_start = Random.Range(0f, 360f);
            var q_start = Quaternion.AngleAxis(angle_start, Vector3.forward);
            var q_end = Quaternion.AngleAxis(angle_start + 20f, Vector3.forward);

            var rotation_curve = EasingCurves.EaseOutQuad;
            var scale_curve = EasingCurves.EaseOutQuad;

            yield return LerpEnumerator.Value(1f, f =>
            {
                root.rotation = Quaternion.Lerp(q_start, q_end, rotation_curve.Evaluate(f));
                root.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scale_curve.Evaluate(f));
            });
        }
    }

    private void ScaleDownAndDestroyRoot(Transform root)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(root, 1f, Vector3.zero)
                .Curve(EasingCurves.EaseInQuad);

            active_roots.Remove(root);
            Destroy(root.gameObject);
        }
    }

    private void DestroyRoot(Transform root)
    {
        ps_dissolve.Duplicate()
            .Parent(GameController.Instance.world)
            .Position(root.position)
            .Scale(root.localScale)
            .Play()
            .Destroy(3);

        Destroy(root.gameObject);
    }

    private Transform CreateRoot()
    {
        var inst = Instantiate(temp_roots);
        inst.gameObject.SetActive(true);
        active_roots.Add(inst);
        ObjectController.Instance.Add(inst.gameObject);
        return inst;
    }
}