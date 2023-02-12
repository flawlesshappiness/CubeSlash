using Flawliz.Lerp;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnlockAbilityView : View
{
    [SerializeField] private UIIconButton temp_btn_ability;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private RectTransform pivot_ability_bar;
    [SerializeField] private CanvasGroup cvg_background, cvg_abilities, cvg_description;
    [SerializeField] private UIUnlockAbilityBar ability_bar;
    [SerializeField] private FMODEventReference sfx_unlock_ability;

    public event System.Action OnAbilitySelected;

    private List<UIIconButton> btns_ability = new List<UIIconButton>();

    private void Start()
    {
        temp_btn_ability.gameObject.SetActive(false);
        DisplayAbility(null);

        var is_first = AbilityController.Instance.GetGainedAbilities().Count == 0;
        var abilities = is_first ?
            AbilityController.Instance.GetAvailableAbilities()
            : AbilityController.Instance.GetAvailableAbilities().TakeRandom(2);

        ClearButtons();
        foreach(var ability in abilities)
        {
            var btn = CreateButton();
            btn.Icon = ability.Info.sprite_icon;
            btn.Button.onSelect += () => DisplayAbility(ability);
            btn.Button.onSubmit += () => Click(btn, ability);
        }

        // Animate
        StartCoroutine(AnimateStartCr());

        void Click(UIIconButton btn, Ability ability)
        {
            FMODButtonEvent.PreviousSelected = null;
            AbilityController.Instance.GainAbility(ability.Info.type);
            sfx_unlock_ability.Play();
            OnAbilitySelected?.Invoke();
            Close(0);
        }
    }

    private UIIconButton CreateButton()
    {
        var btn = Instantiate(temp_btn_ability, temp_btn_ability.transform.parent);
        btn.gameObject.SetActive(true);
        btns_ability.Add(btn);
        return btn;
    }

    private void ClearButtons()
    {
        btns_ability.ForEach(b => Destroy(b.gameObject));
        btns_ability.Clear();
    }

    private void DisplayAbility(Ability ability)
    {
        var text = "";

        if(ability != null)
        {
            text += ability.Info.name_ability;
            text += "\n";
            text += ability.Info.desc_ability;
        }

        tmp_desc.text = text;
    }

    IEnumerator AnimateStartCr()
    {
        Interactable = false;
        ability_bar.CanvasGroup.alpha = 0;
        cvg_background.alpha = 0;
        cvg_abilities.alpha = 0;
        cvg_description.alpha = 0;

        Lerp.Alpha(ability_bar.CanvasGroup, 0.25f, 1).UnscaledTime();
        Lerp.Alpha(cvg_background, 0.25f, 1).UnscaledTime();

        ability_bar.SetPreviousValue();
        yield return new WaitForSecondsRealtime(0.25f);
        yield return ability_bar.AnimateLevelsUntilAbility(1.5f, EasingCurves.EaseOutQuad);
        yield return LerpEnumerator.AnchoredPosition(pivot_ability_bar, 0.5f, pivot_ability_bar.anchoredPosition.AddY(300))
            .Curve(EasingCurves.EaseOutQuad)
            .UnscaledTime();

        cvg_abilities.alpha = 1;
        yield return AnimateButtons();

        cvg_description.alpha = 1;
        Interactable = true;

        // Set selection
        EventSystem.current.SetSelectedGameObject(btns_ability[0].gameObject);
    }

    private Coroutine AnimateButtons()
    {
        return StartCoroutine(AnimateButtonsCr());
        IEnumerator AnimateButtonsCr()
        {
            var crs = new List<Lerp>();
            btns_ability.ForEach(btn => btn.AnimationPivot.localScale = Vector3.zero);
            foreach (var btn in btns_ability)
            {
                var pivot = btn.AnimationPivot;
                pivot.transform.position = btn.transform.position;
                var lerp = Lerp.LocalScale(pivot, 0.25f, Vector3.zero, Vector3.one).Curve(EasingCurves.EaseOutQuad).UnscaledTime();
                crs.Add(lerp);
                yield return new WaitForSecondsRealtime(0.1f);
            }

            yield return StartCoroutine(WaitForCoroutinesCr(crs));
        }

        IEnumerator WaitForCoroutinesCr(List<Lerp> crs)
        {
            foreach (var cr in crs)
            {
                yield return cr;
            }
        }
    }
}