using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathView : View
{
    [SerializeField] private RectTransform rt_title;

    public void AnimateScaleTitle(float duration)
    {
        Lerp.Scale(rt_title, duration, Vector3.one, Vector3.one * 1.2f);
    }
}