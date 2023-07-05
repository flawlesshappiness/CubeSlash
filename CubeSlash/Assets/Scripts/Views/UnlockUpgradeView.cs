using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnlockUpgradeView : View
{
    [SerializeField] private UIUnlockedUpgradesLayout layout_unlocked_upgrades;
    [SerializeField] private UIIconButton temp_btn_upgrade;
    [SerializeField] private TMP_Text tmp_upgrade;
    [SerializeField] private RectTransform rt_upgrades, rt_desc;
    [SerializeField] private UIInputLayout input_layout_main, input_layout_refund;
    [SerializeField] private UIUnlockAbilityBar ability_bar;
    [SerializeField] private Image img_fg_refund;
    [SerializeField] private CanvasGroup cvg_upgrades, cvg_background, cvg_description, cvg_past_upgrades;
    [SerializeField] private UIScrollableUpgradeTree template_upgrade_tree;
    [SerializeField] private UIFloatingPanel floating_panel;
    [SerializeField] private UIFloatingPanelUpgrade floating_panel_upgrade;

    public event System.Action<Upgrade> OnUpgradeSelected;

    private List<UIIconButton> btns_upgrade = new List<UIIconButton>();
    private List<TMP_Text> tmps_upgrade = new List<TMP_Text>();
    private List<UIScrollableUpgradeTree> trees = new List<UIScrollableUpgradeTree>();

    private UIScrollableUpgradeTree selected_tree;

    private Coroutine cr_refund;

    private FMODEventInstance sfx_refund_hold;

    private void Start()
    {
        // Initialize
        temp_btn_upgrade.gameObject.SetActive(false);
        tmp_upgrade.gameObject.SetActive(false);
        template_upgrade_tree.gameObject.SetActive(false);

        layout_unlocked_upgrades.OnUpgradeLevelSelected += (btn, upgrade) => DisplayUpgradeText(btn, upgrade);

        // Upgrades
        CreateUpgradeTrees();

        // Input
        DisplayInput();

        StartCoroutine(AnimateStartCr());
    }

    private void OnEnable()
    {
        PlayerInput.Controls.Player.West.started += RefundPressed;
        PlayerInput.Controls.Player.West.canceled += RefundReleased;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.Player.West.started -= RefundPressed;
        PlayerInput.Controls.Player.West.canceled -= RefundReleased;
        sfx_refund_hold?.Stop();
    }

    private IEnumerator AnimateStartCr()
    {
        cvg_background.alpha = 0;
        cvg_description.alpha = 0;
        cvg_past_upgrades.alpha = 0;
        input_layout_main.CanvasGroup.alpha = 0;
        input_layout_refund.CanvasGroup.alpha = 0;
        ability_bar.CanvasGroup.alpha = 0;
        cvg_upgrades.alpha = 1;
        Interactable = false;

        yield return AnimateShowUpgradeTree();

        // Show other elements
        Interactable = true;
        var first_tree = trees[0];
        first_tree.SetChildUpgradesVisible(true);
        first_tree.Select();
        DisplayUpgradeText(first_tree.MainButton, first_tree.MainInfo.upgrade);

        cvg_description.alpha = 1;
        cvg_past_upgrades.alpha = 1;
        input_layout_main.CanvasGroup.alpha = 1;
        input_layout_refund.CanvasGroup.alpha = 1;
        ability_bar.CanvasGroup.alpha = 1;

        ability_bar.AnimateLevelsUntilAbility(1f, EasingCurves.EaseOutQuad);
    }

    private List<UpgradeInfo> GetRandomUpgrades()
    {
        var count_upgrades = Mathf.Min(4, AbilityController.Instance.GetEquippedAbilities().Count + 2);
        var upgrades = UpgradeController.Instance.GetUnlockableUpgrades()
            .TakeRandom(count_upgrades);
        return upgrades;
    }

    private void ClickUpgradeButton(Upgrade upgrade)
    {
        if (!Interactable) return;
        Interactable = false;
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
        UpgradeController.Instance.UnlockUpgrade(upgrade.id);
        SoundController.Instance.Play(SoundEffectType.sfx_ui_unlock_upgrade);

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            CanvasGroup.alpha = 0;
            yield return new WaitForSecondsRealtime(0.25f);
            GameController.Instance.LerpTimeScale(0.5f, 1);
            OnUpgradeSelected?.Invoke(upgrade);
            yield return new WaitForSecondsRealtime(0.5f);
            Close(0);
        }
    }

    private void CreateUpgradeTrees()
    {
        var upgrades = GetRandomUpgrades();
        foreach (var info in upgrades)
        {
            var tree = Instantiate(template_upgrade_tree, template_upgrade_tree.transform.parent);
            tree.gameObject.SetActive(true);
            tree.Initialize(info);
            tree.SetChildUpgradesVisible(false, 0);
            trees.Add(tree);

            tree.onMainButtonSelected += btn => OnMainButtonSelected(tree);
            tree.onUpgradeSelected += (btn, u) => DisplayUpgradeText(btn, u.upgrade);
            tree.MainButton.Button.onSubmit += () => ClickUpgradeButton(info.upgrade);
        }

        void OnMainButtonSelected(UIScrollableUpgradeTree tree)
        {
            if (selected_tree != null)
            {
                selected_tree.ScrollToPosition(selected_tree.MainButton.transform.position);
                selected_tree.SetChildUpgradesVisible(false);
            }

            selected_tree = tree;

            if (selected_tree != null)
            {
                selected_tree.SetChildUpgradesVisible(true);
            }
        }
    }

    private Coroutine AnimateShowUpgradeTree()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Lerp.Alpha(cvg_background, 0.5f, 1).UnscaledTime();

            foreach (var tree in trees)
            {
                tree.AnimateShowMainButton();
                yield return new WaitForSecondsRealtime(0.1f);
            }

            foreach (var tree in trees)
            {
                tree.AnimateShowParentButtons();
                tree.AnimateShowChildButtons();
                yield return new WaitForSecondsRealtime(0.1f);
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

    private void DisplayUpgradeText(UIIconButton btn, Upgrade upgrade)
    {
        floating_panel_upgrade.Clear();
        var modifiers = upgrade.modifiers;
        foreach (var modifier in modifiers)
        {
            var attribute = GameAttributeController.Instance.GetAttribute(modifier.attribute_type);
            floating_panel_upgrade.AddModifiedAttribute(attribute, modifier);
        }

        floating_panel.SetTarget(btn.transform, new Vector2(50, 50));
        floating_panel.ContentSizeFitterRefresh.RefreshContentFitters();

        ClearUpgradeTexts();
        foreach (var stat in upgrade.stats)
        {
            var text = stat.GetDisplayString();
            CreateUpgradeText(text);
        }
    }

    private void DisplayInput()
    {
        input_layout_main.Clear();
        input_layout_main.AddInput(PlayerInput.UIButtonType.SOUTH, "Select");
        input_layout_refund.Clear();
        input_layout_refund.AddInput(PlayerInput.UIButtonType.WEST, "(HOLD)");
    }

    private void RefundPressed(InputAction.CallbackContext context)
    {
        if (!Interactable) return;
        Interactable = false;
        HoldRefund(true);
    }

    private void RefundReleased(InputAction.CallbackContext context)
    {
        Interactable = true;
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