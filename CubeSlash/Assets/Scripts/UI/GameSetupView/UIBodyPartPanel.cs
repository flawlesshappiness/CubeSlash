using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBodyPartPanel : MonoBehaviour
{
    [SerializeField] private Image img_part;
    [SerializeField] private TMP_Text tmp_name, tmp_desc;

    public void SetSettings(PlayerBodySettings settings)
    {
        // Lock
        var is_locked = !settings.IsUnlocked();

        // Ability
        var ability = AbilityDatabase.LoadAsset().GetAbility(settings.ability_type);
        img_part.sprite = ability.Info.sprite_icon;
        img_part.enabled = !is_locked;
        tmp_name.text = ability.Info.name_ability;
        tmp_desc.text = ability.Info.desc_ability;
    }
}