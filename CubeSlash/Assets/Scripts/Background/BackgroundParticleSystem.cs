using System.Collections;
using UnityEngine;

public class BackgroundParticleSystem : BackgroundObject
{
    private ParticleSystem Prefab { get; set; }
    private ParticleSystem ParticleSystem { get; set; }
    private Camera Camera { get; set; }
    private float MaxWidth { get; set; }

    private Vector3 offset_ps;

    public override void Initialize(Area area)
    {
        base.Initialize(area);
        Camera = CameraController.Instance.Camera;
        MaxWidth = CameraController.Instance.Width;
    }

    private Vector3 GetPSPosition() => transform.position + offset_ps;
    private Vector3 GetCameraPosition() => Camera.transform.position.SetZ(0);
    private float GetDistanceToCamera() => Vector3.Distance(GetCameraPosition(), GetPSPosition());
    private Vector3 GetCameraDirection() => GetCameraPosition() - GetPSPosition();

    public override void UpdateParallax(Vector3 camera_position)
    {
        base.UpdateParallax(camera_position);

        var dist = GetDistanceToCamera();
        if(dist > MaxWidth)
        {
            DestroyPS(ParticleSystem);
            offset_ps += GetCameraDirection();
            ParticleSystem = CreatePS();
        }
    }

    public override void Destroy()
    {
        this.StartCoroutineWithID(Cr(), "destroy_" + GetInstanceID());
        IEnumerator Cr()
        {
            yield return DestroyPS(ParticleSystem);
            Destroy(gameObject);
        }
    }

    public override void DestroyImmediate()
    {
        Destroy(ParticleSystem.gameObject);
        Destroy(gameObject);
    }

    private CustomCoroutine DestroyPS(ParticleSystem ps)
    {
        return this.StartCoroutineWithID(StopAndDestroyCr(), "destroy_" + ps.GetInstanceID());
        IEnumerator StopAndDestroyCr()
        {
            ps.SetEmissionEnabled(false);
            var psm = ps.main;
            var lifetime = Mathf.Max(psm.startLifetime.constant, psm.startLifetime.constantMax);
            yield return new WaitForSeconds(lifetime);
            Destroy(ps.gameObject);
        }
    }

    public void SetPrefab(ParticleSystem prefab)
    {
        Prefab = prefab;
        ParticleSystem = CreatePS();
    }

    private ParticleSystem CreatePS()
    {
        var ps = Instantiate(Prefab, transform);
        ps.transform.localPosition = offset_ps + Vector3.forward * (1 + 20 + 10 * Layer);
        return ps;
    }
}