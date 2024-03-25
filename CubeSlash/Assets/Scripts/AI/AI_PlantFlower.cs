using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class AI_PlantFlower : EnemyAI
{
    public float dist_max_attack;
    public float pillar_size = 2f;
    public float attack_radius = 8f;

    public PlantPillar temp_pillar;

    public ParticleSystem ps_send_attack;

    private Vector3 pos_player_prev;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        temp_pillar.gameObject.SetActive(false);
        StartAttack();
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? PlayerPosition : pos_player_prev;
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        if (dist < dist_max_attack)
        {
            MoveToStop(0.5f);
            TurnTowards(pos_player_prev);
        }
        else
        {
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }
    }

    private Coroutine StartAttack()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            while (true)
            {
                // Move closer
                var dist_max = dist_max_attack;
                while (DistanceToPlayer() > dist_max)
                {
                    yield return null;
                }

                // Wait for a brief random amount of time
                yield return new WaitForSeconds(Random.Range(0f, 1f));

                // Attack
                var duration_send = 0.25f;
                var duration_telegraph = 0.75f;
                var duration_attack = 2f;
                var position = GetTargetPosition();

                Telegraph(duration_send);
                SendAttackFX(duration_send, position);

                SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_enemy_shoot, transform.position);
                SoundController.Instance.PlayGroup(SoundEffectType.sfx_enemy_shoot);

                yield return new WaitForSeconds(duration_send);

                SpawnPillar(duration_telegraph, duration_attack, position);

                // Wait for attack to finish
                yield return new WaitForSeconds(duration_attack);
            }
        }
    }

    private Coroutine Telegraph(float duration)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var speed = 50f;
            var size = 0.25f;
            var y_offset = 10f;
            var start = 1f;
            var end = 0f;
            yield return LerpEnumerator.Value(duration, f =>
            {
                var time = Time.time * speed;
                var mul = Mathf.Lerp(start, end, f);
                var x = Mathf.PerlinNoise(time, 0) * 2 - 1;
                var y = Mathf.PerlinNoise(time + y_offset, 0) * 2 - 1;
                var offset = new Vector3(x, y) * mul * size;
                Body.pivot_sprite.localPosition = offset;
            });
            Body.pivot_sprite.localPosition = Vector3.zero;
        }
    }

    private Coroutine SendAttackFX(float duration, Vector3 target_position)
    {
        var ps = ps_send_attack.Duplicate()
            .Parent(GameController.Instance.world)
            .Position(Position)
            .Play()
            .Destroy(1f + duration);

        ObjectController.Instance.Add(ps.ps.gameObject);

        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var start = Position;
            var end = target_position;
            var curve = EasingCurves.EaseOutQuad;
            yield return LerpEnumerator.Value(duration, f =>
            {
                ps.Position(Vector3.Lerp(start, end, curve.Evaluate(f)));
            });

            ps.ps.SetEmissionEnabled(false);
        }
    }

    private Vector3 GetTargetPosition()
    {
        var offset = (Random.insideUnitCircle * attack_radius).ToVector3();
        var dir = Player.Instance.MoveDirection;
        return PlayerPosition + dir * attack_radius + offset;
    }

    private void SpawnPillar(float duration_telegraph, float duration_attack, Vector3 position)
    {
        var pillar = CreatePillar();
        pillar.transform.position = position;
        pillar.AnimateAppear(duration_telegraph, duration_attack);
    }

    private PlantPillar CreatePillar()
    {
        var inst = Instantiate(temp_pillar);
        inst.SetHidden();
        inst.gameObject.SetActive(true);
        inst.transform.localScale = Vector3.one * pillar_size;
        ObjectController.Instance.Add(inst.gameObject);
        return inst;
    }
}