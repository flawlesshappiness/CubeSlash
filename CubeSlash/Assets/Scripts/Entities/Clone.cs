using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone : MonoBehaviourExtended
{
    public Character Character { get; set; }
    public Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.CHILDREN); } }
    public System.Action<Enemy> OnEnemyCollision { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy)
        {
            OnEnemyCollision?.Invoke(enemy);
        }
    }
}
