using UnityEngine;

public class UIUnlockAbilityBar : UIBar
{
    public CustomCoroutine AnimateLevelsUntilAbility(float duration, AnimationCurve curve = null)
    {
        SetPreviousValue();
        var value = GetValuePercentage();
        return AnimateValue(duration, value, curve);
    }

    public void SetPreviousValue()
    {
        var v = GetValuePercentage(1);
        SetValue(v);
    }

    public void SetCurrentValue()
    {
        var v = GetValuePercentage();
        SetValue(v);
    }

    private float GetValuePercentage(int value_delta = 0)
    {
        var i = Player.Instance.LevelsUntilAbility + value_delta;
        var max = Player.Instance.GetMaxLevelsUntilAbility();
        return 1f - ((float)i / max);
    }
}