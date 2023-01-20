using System.Collections;
using UnityEngine;

public class AI_Volatile : EnemyAI
{
    public Color color_explosion;
    public FMODEventReference sfx_charge;

    private Vector3 pos_player_prev;

    private EnemyVolatileBody volatile_body;
    private bool exploding;

    private const float RADIUS_NEAR = 4f;
    private const float RADIUS_EXPLODE = 5f;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        volatile_body = Self.Body as EnemyVolatileBody;
        Self.OnDeath += OnDeath;
    }

    private void FixedUpdate()
    {
        if (exploding)
        {
            MoveToStop();
        }
        else
        {
            pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }

        if (IsPlayerAlive())
        {
            if(!exploding && DistanceToPlayer() < RADIUS_NEAR)
            {
                StartExploding();
                exploding = true;
            }
        }
    }

    private void StartExploding()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var delay = 0.25f;
            for (int i = 0; i < 3; i++)
            {
                sfx_charge.Play();
                volatile_body.PlayChargePS(RADIUS_EXPLODE);
                yield return new WaitForSeconds(delay);
            }

            Explode();
            Self.Kill();
        }
    }

    private void Explode()
    {
        AbilityExplode.CreateExplodeEffect(transform.position, RADIUS_EXPLODE, color_explosion);
        if(DistanceToPlayer() <= RADIUS_EXPLODE)
        {
            Player.Instance.Damage(transform.position);
        }
    }

    private void OnDeath()
    {
        
    }
}