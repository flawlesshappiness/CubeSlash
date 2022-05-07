using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAbilityCard : MonoBehaviour
{
    [SerializeField] private CanvasGroup cvg;
    [SerializeField] private TMP_Text tmp_name;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private Image img_icon;
    [SerializeField] private Image img_bg;
    [SerializeField] private Button btn_ability;
    [SerializeField] private UIInputButton button;
    [SerializeField] private UIAbilityList ability_list;
    [SerializeField] private UIAbilityModifier prefab_modifier;
    [SerializeField] private AbilityMap empty_ability;
    [SerializeField] private AbilityMap active_ability;

    public int Index { get; set; }
    public bool Interactable { set { cvg.interactable = value; cvg.blocksRaycasts = value; } }
    public UIInputButton InputButton { get { return button; } }
    public List<UIAbilityModifier> Modifiers { get; private set; } = new List<UIAbilityModifier>();
    public Button.ButtonClickedEvent OnClickAbility { get { return btn_ability.onClick; } }
    public System.Action<int> OnClickModifier;

    [System.Serializable]
    public class AbilityMap
    {
        public string name;
        public string desc;
        public Sprite sprite_icon;
        public Color color_bg;
    }

    public void Initialize()
    {
        ability_list.gameObject.SetActive(false);

        // Modifiers
        for (int i = 0; i < ConstVars.COUNT_MODIFIERS; i++)
        {
            var modifier = CreateModifier();
            modifier.Index = i;
            var idx = i;
            modifier.OnClick.AddListener(() => OnClickModifier(idx));
        }
    }

    private UIAbilityModifier CreateModifier()
    {
        prefab_modifier.gameObject.SetActive(true);
        var inst = Instantiate(prefab_modifier.gameObject, prefab_modifier.transform.parent).GetComponent<UIAbilityModifier>();
        Modifiers.Add(inst);
        prefab_modifier.gameObject.SetActive(false);
        return inst;
    }

    public void SelectAbilityButton()
    {
        EventSystem.current.SetSelectedGameObject(btn_ability.gameObject);
    }

    public void SelectModifierButton(int idx)
    {
        EventSystem.current.SetSelectedGameObject(Modifiers[idx].Button.gameObject);
    }

    public void UpdateUI()
    {
        var ability = Player.Instance.AbilitiesEquipped[Index];
        SetAbility(ability);

        for (int i = 0; i < ConstVars.COUNT_MODIFIERS; i++)
        {
            var modifier = ability ? ability.Modifiers[i] : null;
            SetModifier(Modifiers[i], modifier);
        }
    }

    private void SetAbility(Ability ability)
    {
        var active = ability != null;
        tmp_name.text = active ? ability.name_ability : empty_ability.name;
        tmp_desc.text = active ? ability.desc_ability : empty_ability.desc;
        img_icon.sprite = active ? ability.sprite_icon : empty_ability.sprite_icon;

        img_bg.color = active ? active_ability.color_bg : empty_ability.color_bg;
    }

    private void SetModifier(UIAbilityModifier modifier, Ability ability)
    {
        modifier.Name = ability ? ability.name_ability : empty_ability.name;
        modifier.Desc = ability ? ability.desc_ability : empty_ability.desc;
        modifier.Sprite =  ability ? ability.sprite_icon : empty_ability.sprite_icon;
    }

    #region SELECT ABILITY
    public void ShowSelectAbility(System.Action<Ability> onSelectAbility, System.Action onCancel, bool can_unequip = false)
    {
        ability_list.gameObject.SetActive(true);
        ability_list.Show(can_unequip, onSelectAbility, onCancel);
    }

    public void HideSelectAbility()
    {
        ability_list.gameObject.SetActive(false);
    }
    #endregion
}
