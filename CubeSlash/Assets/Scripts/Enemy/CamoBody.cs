using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class CamoBody : EnemyBody
{
    public SpriteRenderer spr_camo;

    public void SetCamoColor(Color color)
    {
        spr_camo.color = color;
    }

    public CustomCoroutine AnimateCamoColor(Color color, float duration = 0.5f)
    {
        return this.StartCoroutineWithID(Cr(), gameObject);
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Color(spr_camo, duration, color);
        }
    }

    public CustomCoroutine AnimateRemoveCamoColor(float duration = 0.5f)
    {
        return AnimateCamoColor(spr_camo.color.SetA(0), duration);
    }
}