using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BossJellyBody : BossBody
{
    [Header("JELLY")]
    public SpriteRenderer spr_shield;

    public float T_Health { get; set; } = 1f;

    private float TimeShield => Mathf.Lerp(time_shield_min, time_shield_max, T_Health);

    private float time_shield_max = 1.5f;
    private float time_shield_min = 0.5f;

    private void Start()
    {
        AnimateShieldSprite();
    }

    private void AnimateShieldSprite()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            while (true)
            {
                yield return LerpEnumerator.Alpha(spr_shield, TimeShield, 0f).Curve(EasingCurves.EaseInOutQuad);
                yield return LerpEnumerator.Alpha(spr_shield, TimeShield, 1f).Curve(EasingCurves.EaseInOutQuad);
            }
        }
    }
}