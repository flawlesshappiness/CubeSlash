using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnlockUpgradeView : View
{
    [SerializeField] private UIUnlockedUpgradesLayout layout_unlocked_upgrades;
    [SerializeField] private UIIconButton temp_btn_upgrade;
    [SerializeField] private TMP_Text tmp_upgrade;
    [SerializeField] private RectTransform rt_upgrades, rt_desc;
    [SerializeField] private UIInputLayout input_layout;
    [SerializeField] private UIUnlockAbilityBar ability_bar;
    [SerializeField] private Image img_fg_refund;
    [SerializeField] private CanvasGroup cvg_upgrades, cvg_background, cvg_description, cvg_past_upgrades;

    public event System.Action<Upgrade> OnUpgradeSelected;

    private List<UIIconButton> btns_upgrade = new List<UIIconButton>();
    private List<TMP_Text> tmps_upgrade = new List<TMP_Text>();

    private UIIconButton button_selected;
    private Upgrade upgrade_selected;

    private Coroutine cr_refund;

    private FMODEventInstance sfx_refund_hold;

    private void Start()
    {
        // Initialize
        temp_btn_upgrade.gameObject.SetActive(false);
        tmp_upgrade.gameObject.SetActive(false);

        layout_unlocked_upgrades.OnUpgradeLevelSelected += level => DisplayUpgradeText(level);

        // Upgrades
        CreateUpgrades();

        // Input
        DisplayInput();

        StartCoroutine(AnimateStartCr());
    }

    private void OnEnable()
    {
        PlayerInput.Controls.Player.North.started += OnNorthPressed;
        PlayerInput.Controls.Player.North.canceled += OnNorthReleased;

        PlayerInput.Controls.Player.West.started += RefundPressed;
        PlayerInput.Controls.Player.West.canceled += RefundReleased;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.Player.North.started -= OnNorthPressed;
        PlayerInput.Controls.Player.North.canceled -= OnNorthReleased;

        PlayerInput.Controls.Player.West.started -= RefundPressed;
        PlayerInput.Controls.Player.West.canceled -= RefundReleased;
    }

    private IEnumerator AnimateStartCr()
    {
        cvg_background.alpha = 0;
        cvg_description.alpha = 0;
        cvg_past_upgrades.alpha = 0;
        input_layout.CanvasGroup.alpha = 0;
        ability_bar.CanvasGroup.alpha = 0;
        cvg_upgrades.alpha = 1;
        Interactable = false;

        yield return AnimateShowUpgrade();

        // Show other elements
        Interactable = true;
        EventSystem.current.SetSelectedGameObject(btns_upgrade[0].gameObject);

        cvg_description.alpha = 1;
        cvg_past_upgrades.alpha = 1;
        input_layout.CanvasGroup.alpha = 1;
        ability_bar.CanvasGroup.alpha = 1;

        ability_bar.AnimateLevelsUntilAbility(1f, EasingCurves.EaseOutQuad);
    }

    private void OnNorthPressed(InputAction.CallbackContext context)
    {
        if(upgrade_selected != null)
        {
            ShowUpgradeTree();
        }
    }

    private void OnNorthReleased(InputAction.CallbackContext context)
    {
        HideUpgradeTree();
    }

    private void CreateUpgrades()
    {
        // Fetch upgrades
        var count_upgrades = Mathf.Min(4, AbilityController.Instance.GetEquippedAbilities().Count + 2);
        var upgrades = UpgradeController.Instance.GetUnlockableUpgrades()
            .TakeRandom(count_upgrades);

        // Create upgrade buttons
        ClearUpgradeButtons();
        foreach (var info in upgrades)
        {
            var btn = CreateUpgradeButton();
            btn.Icon = info.upgrade.icon;
            btn.Button.onSelect += () => OnSelect(btn, info.upgrade);
            btn.Button.onSubmit += () => OnClick(btn, info.upgrade);
        }

        void OnSelect(UIIconButton button, Upgrade upgrade)
        {
            button_selected = button;
            upgrade_selected = upgrade;
            DisplayUpgradeText(upgrade);
        }

        void OnClick(UIIconButton btn, Upgrade upgrade)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            UpgradeController.Instance.UnlockUpgrade(upgrade.id);
            SoundController.Instance.Play(SoundEffectType.sfx_ui_unlock_upgrade);
            OnUpgradeSelected?.Invoke(upgrade);
            Close(0);
        }
    }

    private UIIconButton CreateUpgradeButton()
    {
        var btn = Instantiate(temp_btn_upgrade, temp_btn_upgrade.transform.parent);
        btn.gameObject.SetActive(true);
        btns_upgrade.Add(btn);
        return btn;
    }

    private void ClearUpgradeButtons()
    {
        btns_upgrade.ForEach(b => Destroy(b.gameObject));
        btns_upgrade.Clear();
    }

    private Coroutine AnimateShowUpgrade()
    {
        return StartCoroutine(AnimateButtonsCr());
        IEnumerator AnimateButtonsCr()
        {
            Lerp.Alpha(cvg_background, 0.5f, 1).UnscaledTime();

            var crs = new List<Lerp>();
            btns_upgrade.ForEach(btn => btn.AnimationPivot.localScale = Vector3.zero);
            foreach (var btn in btns_upgrade)
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
            foreach(var cr in crs)
            {
                yield return cr;
            }
        }
    }

    private TMP_Text CreateUpgradeText(string text)
    {
        var tmp = Instantiate(tmp_upgrade, tmp_upgrade.transform.parent);
        tmp.gameObject.SetActive(true);
        tmp.text = text;
        tmps_upgrade.Add(tmp);
        return tmp;
    }

    private void ClearUpgradeTexts()
    {
        tmps_upgrade.ForEach(t => Destroy(t.gameObject));
        tmps_upgrade.Clear();
    }

    private void DisplayUpgradeText(Upgrade upgrade)
    {
        var db_color = ColorDatabase.Load();

        ClearUpgradeTexts();
        //CreateUpgradeText(upgrade.name, db_color.text_normal.GetColor());
        foreach(var stat in upgrade.stats)
        {
            var text = stat.GetDisplayString();
            CreateUpgradeText(text);
        }
    }

    private void ShowUpgradeTree()
    {
        if (upgrade_selected == null) return;
        if (button_selected == null) return;

        Interactable = false;

        rt_upgrades.gameObject.SetActive(false);
        rt_desc.gameObject.SetActive(false);

        /*
        var view = ViewController.Instance.ShowView<UpgradeTreeView>(0, "UpgradeTree");
        var tree = UpgradeController.Instance.GetUpgradeTree(upgrade_selected.id);
        view.SetTree(tree);
        view.SetSelectedUpgrade(upgrade_selected);
        */
    }

    private void HideUpgradeTree()
    {
        if (upgrade_selected == null) return;
        if (button_selected == null) return;

        ViewController.Instance.CloseView(0, "UpgradeTree");

        rt_upgrades.gameObject.SetActive(true);
        rt_desc.gameObject.SetActive(true);

        Interactable = true;
        EventSystem.current.SetSelectedGameObject(button_selected.Button.gameObject);
    }

    private void DisplayInput()
    {
        input_layout.Clear();
        input_layout.AddInput(PlayerInput.UIButtonType.SOUTH, "Select");
        input_layout.AddInput(PlayerInput.UIButtonType.NORTH, "Upgrade tree");
        input_layout.AddInput(PlayerInput.UIButtonType.WEST, "(HOLD) Refund 25% exp");
    }

    private void RefundPressed(InputAction.CallbackContext context)
    {
        HoldRefund(true);
    }

    private void RefundReleased(InputAction.CallbackContext context)
    {
        HoldRefund(false);
    }

    private void HoldRefund(bool holding)
    {
        if (holding)
        {
            if (cr_refund != null) return;
            cr_refund = StartCoroutine(RefundCr());
        }
        else
        {
            if (cr_refund == null) return;
            StopCoroutine(cr_refund);
            cr_refund = null;
            img_fg_refund.SetAlpha(0);
            sfx_refund_hold?.Stop();
        }

        IEnumerator RefundCr()
        {
            sfx_refund_hold = SoundController.Instance.Play(SoundEffectType.sfx_ui_refund_hold);

            var anim = LerpEnumerator.Alpha(img_fg_refund, 2f, 0f, 0.25f);
            anim.AnimationCurve = EasingCurves.EaseInQuad;
            anim.UseUnscaledTime = true;
            yield return anim;

            sfx_refund_hold?.Stop();

            Refund();
        }
    }

    private void Refund()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_ui_refund);

        Player.Instance.Experience.Value += Player.Instance.Experience.Max * 0.25f;

        var view = ViewController.Instance.ShowView<AbilityView>(0, GameController.TAG_ABILITY_VIEW);
        view.OnContinue += GameController.Instance.ResumeLevel;
        Close(0);
    }
}