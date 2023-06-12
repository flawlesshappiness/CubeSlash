using System.Collections;
using UnityEngine;

public class AI_BossRootPull : BossAI
{
    [SerializeField] private RootPullVine _vine;

    private BossRootBody root_body;
    private Vector3 destination;
    private bool attached;
    private bool can_attach = true;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        root_body = enemy.Body.GetComponent<BossRootBody>();
        Body.OnDudKilled += dud => OnDudKilled();
    }

    private void Update()
    {
        VineUpdate();
    }

    private void FixedUpdate()
    {
        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        if (IsPlayerAlive())
        {
            destination = PlayerPosition;
        }

        var dir = (destination - Position).normalized;
        var offset = dir * Self.Settings.size * 0.7f;
        var t = (Vector3.Distance(Position + offset, destination)) / (CameraController.Instance.Width * 0.5f);
        Self.AngularAcceleration = Mathf.Lerp(Self.Settings.angular_acceleration * 0.25f, Self.Settings.angular_acceleration, t);
        Self.AngularVelocity = Mathf.Lerp(Self.Settings.angular_velocity * 0.25f, Self.Settings.angular_velocity, t);
        MoveTowards(destination);
        TurnTowards(destination);
    }

    private void VineUpdate()
    {
        var width = CameraController.Instance.Width;
        var max_dist = width * 0.4f;
        var dist = DistanceToPlayer();

        if(dist > max_dist)
        {
            AttachVine();
        }
        else
        {
            UnattachVine();
        }
    }

    private void AttachVine()
    {
        if (attached) return;
        if (!can_attach) return;
        attached = true;
        _vine.target = Player.Instance.Rigidbody;
        _vine.AnimateToTarget();
    }

    private void UnattachVine()
    {
        if (!attached) return;
        attached = false;
        _vine.AnimateFromTarget();
        _vine.target = null;
    }

    private void OnDudKilled()
    {
        if (Self.IsDead) return;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            can_attach = false;
            UnattachVine();

            var time = Random.Range(2f, 3f);
            HideDuds();
            yield return new WaitForSeconds(time);
            TeleportHide();
            yield return new WaitForSeconds(0.5f);
            //yield return GetAttackCr();
            yield return new WaitForSeconds(1f);
            TeleportAppear();
            yield return new WaitForSeconds(1f);
            ShowDuds();

            can_attach = true;
        }
    }

    private void TeleportHide()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_teleport_disappear);
        PlayTeleportPS();
        Self.Body.gameObject.SetActive(false);
    }

    private void TeleportAppear()
    {
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        var width = CameraController.Instance.Width;
        Self.transform.position = Player.Instance.transform.position + dir * (width * 0.5f);

        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_teleport_appear);
        PlayTeleportPS();
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.2f);
            Self.Body.gameObject.SetActive(true);
        }
    }

    private void PlayTeleportPS()
    {
        root_body.ps_teleport.Duplicate()
            .Parent(GameController.Instance.world)
            .Scale(root_body.ps_teleport.transform.lossyScale)
            .Play()
            .Destroy(5f);
    }
}