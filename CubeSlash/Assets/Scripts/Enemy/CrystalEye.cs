using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class CrystalEye : MonoBehaviour
{
    [SerializeField] private Transform pivot_pupil;
    [SerializeField] private SpriteRenderer spr_pupil_charged;
    [SerializeField] private EnemyBody body;
    [SerializeField] private ParticleSystem ps_charge, ps_shoot;

    public float radius_pupil;

    public float PupilRadius { get { return radius_pupil * transform.lossyScale.x; } }
    public bool LookingAtPlayer { get; set; }
    public bool Open { set { body.animator_main.SetBool("open", value); } }
    public bool Shielded { set { body.animator_main.SetBool("shielded", value); } }
    public bool ShowPupil { set { pivot_pupil.localScale = value ? Vector3.one : Vector3.zero; } }

    private void Start()
    {
        spr_pupil_charged.SetAlpha(0);
    }

    private void Update()
    {
        var position = GetTargetPosition();
        var dir = position - transform.position;
        var clamped_position = transform.position + dir.normalized * PupilRadius;
        pivot_pupil.position = Vector3.Lerp(pivot_pupil.position, clamped_position, Time.deltaTime * 5f);

        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        ps_charge.transform.rotation = rotation;
        ps_shoot.transform.rotation = rotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, PupilRadius);
    }

    private Vector3 GetTargetPosition()
    {
        return LookingAtPlayer ? Player.Instance.transform.position : transform.position;
    }

    public Lerp AnimatePupilCharged(float duration, bool charged) => Lerp.Alpha(spr_pupil_charged, duration, charged ? 1 : 0);

    public Coroutine AnimateOpen()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            LookingAtPlayer = false;
            Open = true;
            yield return LerpEnumerator.Value(0.4f, f =>
            {
                pivot_pupil.localScale = Vector3.one * Mathf.Lerp(0f, 1f, f);
            });
            LookingAtPlayer = true;
        }
    }

    public Coroutine AnimateClose()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            LookingAtPlayer = false;
            Open = false;
            yield return LerpEnumerator.Value(0.4f, f =>
            {
                pivot_pupil.localScale = Vector3.one * Mathf.Lerp(1f, 0f, f);
            });
        }
    }

    public void PlayChargeFX()
    {
        ps_charge.Play();
    }

    public void PlayShootFX()
    {
        ps_shoot.Play();
    }
}