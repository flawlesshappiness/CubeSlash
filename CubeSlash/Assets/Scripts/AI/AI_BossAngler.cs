using System.Collections;
using UnityEngine;

public class AI_BossAngler : EnemyAI
{
    [SerializeField] private AnglerLamp template_lamp;
    [SerializeField] private AnglerTeeth template_teeth;

    private BossAnglerBody angler_body;
    private AnglerLamp lamp;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        angler_body = enemy.Body as BossAnglerBody;
        var lamp_target = angler_body.GetLampTarget();

        lamp = Instantiate(template_lamp, GameController.Instance.world);
        lamp.transform.localScale = Vector3.one * Self.Settings.size;
        lamp.SetTarget(lamp_target);
        lamp.ResetPosition();

        this.StartCoroutineWithID(AttacksCr(), "attacks_" + GetInstanceID());
    }

    private IEnumerator AttacksCr()
    {
        while (true)
        {
            angler_body.gameObject.SetActive(true);
            lamp.gameObject.SetActive(true);
            yield return DashAttackCr();
            angler_body.gameObject.SetActive(false);
            lamp.gameObject.SetActive(false);
            //yield return SmallBiteAttackCr();
            yield return BigBiteAttackCr();
        }
    }

    private IEnumerator DashAttackCr()
    {
        var start_dir = Random.insideUnitCircle.normalized.ToVector3();
        var player_position = Player.Instance.transform.position;
        var level_size = (Level.Current.size * 0.5f) - 15;
        var start_position = player_position - start_dir * level_size;

        var angle = Vector3.SignedAngle(Vector3.up, start_dir, Vector3.forward);
        var start_rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        var has_accelerated = false;
        var time_accelerate = Time.time + 5f;

        LerpLinearVelocity(0.1f, 10f);
        Self.LinearAcceleration = 10f;
        Self.transform.position = start_position;
        Self.transform.rotation = start_rotation;
        lamp.ResetPosition();

        var start_dist_to_player = DistanceToPlayer();
        var dist_to_player = start_dist_to_player;
        var moving_to_player = true;
        while (true)
        {
            var dist = DistanceToPlayer();
            if (moving_to_player)
            {
                if(dist_to_player < start_dist_to_player * 0.75f && dist > dist_to_player)
                {
                    moving_to_player = false;
                }
                dist_to_player = dist;
            }
            else if(dist >= level_size)
            {
                break;
            }

            if (!has_accelerated && Time.time > time_accelerate)
            {
                has_accelerated = true;
                LerpLinearVelocity(5f, 30f);
            }

            var end_position = PlayerPosition + start_dir * level_size;
            MoveToPosition(end_position);
            yield return null;
        }

        void MoveToPosition(Vector3 position)
        {
            MoveTowards(position);
        }
    }

    private IEnumerator SmallBiteAttackCr()
    {
        Coroutine last = null;
        var player_position = Player.Instance.transform.position;
        var dist = 15f;
        for (int i = 0; i < 5; i++)
        {
            var pos = player_position + Random.insideUnitCircle.normalized.ToVector3() * Random.Range(-dist, dist);
            last = StartCoroutine(CreateBite(pos));
            yield return new WaitForSeconds(0.2f);
        }

        yield return last;

        IEnumerator CreateBite(Vector3 position)
        {
            var teeth = Instantiate(template_teeth, GameController.Instance.world);
            teeth.transform.position = position;
            teeth.SetHidden();
            teeth.AnimateAppear(1f, 10f);
            yield return teeth.AnimateSmallBite(2f);
            yield return new WaitForSeconds(0.5f);
            teeth.AnimateHide(0.25f);
            Destroy(teeth.gameObject, 0.25f);
        }
    }

    private IEnumerator BigBiteAttackCr()
    {
        var size = 20f;
        var teeth = Instantiate(template_teeth, GameController.Instance.world);
        teeth.SetHidden();
        teeth.AnimateAppear(4f, size);
        var anim_bite = teeth.AnimateBigBite(4f);
        teeth.transform.position = Player.Instance.transform.position;

        var time_end = Time.time + 3f;
        while(Time.time < time_end)
        {
            teeth.transform.position = Player.Instance.transform.position;
            yield return null;
        }

        yield return anim_bite;

        Bite(teeth.transform.position, size * 0.25f);

        yield return new WaitForSeconds(0.5f);

        teeth.AnimateHide(0.25f);
        Destroy(teeth.gameObject, 0.25f);
    }
    
    private void Bite(Vector3 position, float radius)
    {
        var hits = Physics2D.OverlapCircleAll(position, radius);
        foreach(var hit in hits)
        {
            var player = hit.GetComponentInParent<Player>();
            if (player == null) continue;

            player.Damage(position);
        }
    }
}