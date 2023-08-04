using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class AI_BossRootPull : BossAI
{
    [SerializeField] private RootPullVine _vine;
    [SerializeField] private Transform _pivot_walls_rotation;
    [SerializeField] private Transform[] _pivot_walls;
    [SerializeField] private Obstacle[] _walls;

    private BossRootBody root_body;
    private Vector3 destination;
    private bool attached;
    private bool can_attach = true;

    private int hits_taken;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        root_body = enemy.Body.GetComponent<BossRootBody>();
        Body.OnDudKilled += dud => OnDudKilled();

        _pivot_walls_rotation.gameObject.SetActive(false);
    }

    private void Update()
    {
        VineUpdate();
        UpdateWallsRotation();
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

        if (dist > max_dist)
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
        _vine.Attach();
        _vine.AnimateToTarget();

        var sfx = SoundController.Instance.Play(SoundEffectType.sfx_enemy_root);
        sfx.SetPitch(-1);
    }

    private void UnattachVine()
    {
        if (!attached) return;
        attached = false;
        _vine.AnimateFromTarget();
        _vine.Unattach();
    }

    private void OnDudKilled()
    {
        if (Self.IsDead) return;

        hits_taken++;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            can_attach = false;
            UnattachVine();

            var time = Random.Range(2f, 3f);
            HideDuds();
            yield return new WaitForSeconds(time);
            TeleportHide();
            yield return new WaitForSeconds(3f);
            /*
            SetupWalls();
            yield return ShrinkWallsCr();
            yield return new WaitForSeconds(1f);
            */
            TeleportAppear();
            yield return new WaitForSeconds(1f);
            can_attach = true;
            yield return new WaitForSeconds(4f);

            ShowDuds();
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

    private void SetupWalls()
    {
        ObjectController.Instance.Add(_pivot_walls_rotation.gameObject);
        _pivot_walls_rotation.parent = GameController.Instance.world;
        _pivot_walls_rotation.transform.localScale = Vector3.one;
        _pivot_walls_rotation.transform.position = Player.Instance.transform.position;

        var width = CameraController.Instance.Width;
        var delta_angle = 360f / _pivot_walls.Length;

        for (int i = 0; i < _pivot_walls.Length; i++)
        {
            var pivot = _pivot_walls[i];
            var angle = delta_angle * i;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            pivot.rotation = rotation;
        }

        for (int i = 0; i < _walls.Length; i++)
        {
            var wall = _walls[i];
            wall.transform.localPosition = new Vector3(width * 2, 0);
        }

        _pivot_walls_rotation.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
        _pivot_walls_rotation.gameObject.SetActive(true);
    }

    private IEnumerator ShrinkWallsCr()
    {
        var duration_move = 2f;
        var width_max = CameraController.Instance.Width * 0.4f;
        var width_min = CameraController.Instance.Width * 0.2f - (0.025f * hits_taken);
        var count_move = 3 + hits_taken;

        for (int i = 0; i < count_move; i++)
        {
            var ti = (float)i / (count_move - 1);

            var sfx = SoundController.Instance.Play(SoundEffectType.sfx_enemy_root);
            sfx.SetPitch(-2);

            foreach (var wall in _walls)
            {
                var w = Mathf.Lerp(width_max, width_min, ti);
                Lerp.LocalPosition(wall.transform, duration_move, new Vector3(w, 0))
                    .Curve(EasingCurves.EaseOutQuad);
            }

            yield return new WaitForSeconds(duration_move);
            yield return new WaitForSeconds(0.2f);
        }

        foreach (var wall in _walls)
        {
            var w = CameraController.Instance.Width * 2;
            Lerp.LocalPosition(wall.transform, duration_move, new Vector3(w, 0))
                .Curve(EasingCurves.EaseInOutQuad);

            var sfx = SoundController.Instance.Play(SoundEffectType.sfx_enemy_root);
            sfx.SetPitch(1);
        }

        yield return new WaitForSeconds(duration_move);

        _pivot_walls_rotation.gameObject.SetActive(false);
    }

    private void UpdateWallsRotation()
    {
        if (GameController.Instance.IsPaused) return;
        _pivot_walls_rotation.rotation *= Quaternion.AngleAxis(10f * Time.deltaTime, Vector3.forward);
    }
}