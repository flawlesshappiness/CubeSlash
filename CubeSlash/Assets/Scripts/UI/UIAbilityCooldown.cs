using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityCooldown : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private Image img_cooldown;
    [SerializeField] private CanvasGroup cvg;

    private Ability ability;

    public void SetAbility(Ability ability)
    {
        this.ability = ability;
        img_icon.sprite = ability ? ability.sprite_icon : null;
        img_icon.enabled = img_icon.sprite != null;
        img_cooldown.fillAmount = 0;
        cvg.alpha = ability ? 1 : 0.1f;
    }

    private void Update()
    {
        if (ability)
        {
            img_cooldown.fillAmount = 
                Player.Instance.AbilityBlockCounter > 0 ? 1 :
                ability.OnCooldown ? ability.CooldownPercentage : 0;
        }
    }
}
