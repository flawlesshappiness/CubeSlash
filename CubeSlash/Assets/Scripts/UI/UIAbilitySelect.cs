using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySelect : MonoBehaviour
{
    [SerializeField] private CanvasGroup cvg;
    [SerializeField] private RectTransform rt_main;
    [SerializeField] private Image img_scroll_current;
    [SerializeField] private Image img_scroll_next;
    [SerializeField] private Button btn;

    public System.Action OnSelected { get; set; }
    public System.Action OnDeselected { get; set; }
    public System.Action<Ability> OnAbilitySelected { get; set; }
    public System.Action<Ability> OnAbilityHighlighted { get; set; }

    public Ability Ability { get; private set; }
    public bool Interactable { set { cvg.interactable = value; cvg.blocksRaycasts = value; } }
    private bool Selected { get; set; }
    private bool Highlighted { get; set; }

    private bool has_scrolled;
    private float time_input;
    private int idx_ability;
    private List<Ability> abilities = new List<Ability>();

    private void Start()
    {
        btn.onClick.AddListener(ClickButton);
    }

    private void Update()
    {
        HighlightUpdate();
        InputUpdate();
        ScrollUpdate();
    }

    private void HighlightUpdate()
    {
        var highlight = EventSystemController.Instance.EventSystem.currentSelectedGameObject == btn.gameObject;
        if (Highlighted != highlight)
        {
            Highlighted = highlight;
            if (Highlighted)
            {
                OnAbilityHighlighted?.Invoke(Ability);
            }
        }
    }

    private void InputUpdate()
    {
        if (Selected)
        {
            if (PlayerInputController.Instance.GetJoystickButtonDown(PlayerInputController.JoystickButtonType.EAST))
            {
                Cancel();
            }
        }
    }

    private void ScrollUpdate()
    {
        if (Selected)
        {
            if (Time.unscaledTime < time_input) return;

            var ver = Input.GetAxisRaw("Vertical");
            if (ver.Abs() > 0.5f)
            {
                if (ver > 0)
                {
                    AdjustAbility(1);
                }
                else
                {
                    AdjustAbility(-1);
                }
                time_input = Time.unscaledTime + 0.25f;
            }
            else
            {
                time_input = Time.unscaledTime;
            }
        }
    }

    private void ClickButton()
    {
        SetSelected(!Selected);
    }

    public void SetSelected(bool selected)
    {
        Selected = selected;

        if (selected)
        {
            has_scrolled = false;
            abilities = Player.Instance.AbilitiesUnlocked.Where(a => !a.Equipped || a == Ability).ToList();
            idx_ability = Ability == null ? 0 : abilities.IndexOf(Ability);
            OnSelected?.Invoke();
        }
        else
        {
            SubmitAbility(has_scrolled ? abilities[idx_ability] : Ability);
            Deselect();
        }
    }

    private void Deselect()
    {
        Selected = false;
        OnDeselected?.Invoke();
    }

    private void SubmitAbility(Ability ability)
    {
        if (ability == Ability) return;
        SetAbility(ability);
        OnAbilitySelected?.Invoke(ability);
    }

    public void SetAbility(Ability ability)
    {
        Ability = ability;
        var has_ability = ability != null;

        var sprite = has_ability ? ability.sprite_icon : null;
        SetSprite(img_scroll_current, null);
        SetSprite(img_scroll_next, sprite);

        img_scroll_current.transform.localPosition = new Vector3(0, -rt_main.rect.height);
        img_scroll_next.transform.localPosition = Vector3.zero;
    }

    private void AdjustAbility(int adjust)
    {
        if (abilities.Count == 0) return;

        has_scrolled = true;

        idx_ability = (idx_ability + adjust).AbsMod(abilities.Count);
        var ability_next = abilities[idx_ability];

        SetSprite(img_scroll_current, img_scroll_next.sprite);
        SetSprite(img_scroll_next, ability_next.sprite_icon);

        var sign = adjust > 0 ? -1 : 1;
        Lerp.Position(img_scroll_current.transform, 0.25f, Vector3.one, new Vector3(0, rt_main.rect.height * sign), true)
            .UnscaledTime();
        Lerp.Position(img_scroll_next.transform, 0.25f, new Vector3(0, rt_main.rect.height * sign * -1), Vector3.one, true)
            .UnscaledTime();

        OnAbilityHighlighted?.Invoke(ability_next);
    }

    public void SelectEventSystem()
    {
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(btn.gameObject);
    }

    private void SetSprite(Image img, Sprite sprite)
    {
        img.sprite = sprite;
        img.color = img.color.SetA(img.sprite == null ? 0 : 1);
    }

    public void Unequip()
    {
        SubmitAbility(null);
        Deselect();
    }

    public void Cancel()
    {
        SetAbility(Ability);
        Deselect();
    }
}