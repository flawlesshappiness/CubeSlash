using UnityEngine;

public class Obstacle : MonoBehaviour, IKillable, IHurt
{
    public bool hurts;
    public bool enemy_ai_ignore;
    public bool is_area_obstacle;

    public ParticleSystem ps_destroy;

    public bool CanHit() => false;
    public bool CanKill() => false;

    public Vector3 GetPosition() => transform.position;

    public virtual bool TryKill() => false;
    public bool CanHurt() => hurts;

    public void DisableDestroy()
    {
        if (ps_destroy != null)
        {
            ps_destroy
                .Duplicate()
                .Parent(GameController.Instance.world)
                .Position(transform.position)
                .Scale(transform.localScale)
                .Play()
                .Destroy(4f);
        }

        gameObject.SetActive(false);
    }
}