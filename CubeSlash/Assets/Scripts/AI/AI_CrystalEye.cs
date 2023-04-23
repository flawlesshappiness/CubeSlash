using System.Collections;
using UnityEngine;

public class AI_CrystalEye : AI_EnemyShield
{
    [SerializeField] private CrystalEye eye;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        Self.CanReposition = false;
        eye = enemy.Body.GetComponent<CrystalEye>();

        Self.transform.localScale = Vector3.one * Random.Range(1.5f, 4f);

        StartCoroutine(StateCr());
    }

    private IEnumerator StateCr()
    {
        while (true)
        {
            Self.transform.position = GetPositionNearPlayer();
            Self.Body.Collider.enabled = false;
            Self.Body.Trigger.enabled = false;
            yield return eye.AnimateOpen();

            Self.Body.Collider.enabled = true;
            Self.Body.Trigger.enabled = true;

            yield return new WaitForSeconds(Random.Range(5f, 8f));

            Self.Body.Collider.enabled = false;
            Self.Body.Trigger.enabled = false;

            eye.ShowPupil = true;
            Unshield();
            yield return eye.AnimateClose();
        }
    }

    protected override void SetState(int state)
    {
        base.SetState(state);

        eye.ShowPupil = state != 1;

        if(state == 2)
        {
            Self.Kill();
        }
    }

    private Vector3 GetPositionNearPlayer()
    {
        var rnd = Random.insideUnitCircle.ToVector3().normalized;
        return Player.Instance.transform.position + rnd * Random.Range(10, 20);
    }
}