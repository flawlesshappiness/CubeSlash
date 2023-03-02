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

        layout_unlocked_upgrades.OnUpgradeLevelSelected += level => DisplayUpgradeText(level);

        // Upgrades
        //CreateUpgrades();
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

        //yield return AnimateShowUpgrade();
        yield return AnimateShowUpgradeTree();

        // Show other elements
        Interactable = true;
        var first_tree = trees[0];
        first_tree.SetChildUpgradesVisible(true);
        first_tree.Select();
        DisplayUpgradeText(first_tree.MainInfo.upgrade);

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

    private void CreateUpgrades()
    {
        // Fetch upgrades
        var upgrades = GetRandomUpgrades();

        // Create upgrade buttons
        ClearUpgradeButtons();
        foreach (var info in upgrades)
        {
            var btn = CreateUpgradeButton();
            btn.Icon = info.upgrade.icon;
            btn.Button.onSelect += () => OnSelect(btn, info.upgrade);
            btn.Button.onSubmit += () => ClickUpgradeButton(info.upgrade);
        }

        void OnSelect(UIIconButton button, Upgrade upgrade)
        {
            DisplayUpgradeText(upgrade);
        }
    }

    private void ClickUpgradeButton(Upgrade upgrade)
    {
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
        UpgradeController.Instance.UnlockUpgrade(upgrade.id);
        SoundController.Instance.Play(SoundEffectType.sfx_ui_unlock_upgrade);
        OnUpgradeSelected?.Invoke(upgrade);
        Close(0);
    }

    private void CreateUpgradeTrees()
    {
        var upgrades = GetRandomUpgrades();
        foreach(var info in upgrades)
        {
            var tree = Instantiate(template_upgrade_tree, template_upgrade_tree.transform.parent);
            tree.gameObject.SetActive(true);
            tree.Initialize(info);
            tree.SetChildUpgradesVisible(false, 0);
            trees.Add(tree);

            tree.onMainButtonSelected += () => OnMainButtonSelected(tree);
            tree.onUpgradeSelected += u => DisplayUpgradeText(u.upgrade);
            tree.MainButton.Button.onSubmit += () => ClickUpgradeButton(info.upgrade);
        }

        void OnMainButtonSelected(UIScrollableUpgradeTree tree)
        {
            if(selected_tree != null)
            {
                selected_tree.ScrollToPosition(selected_tree.MainButton.transform.position);
                selected_tree.SetChildUpgradesVisible(false);
            }

            selected_tree = tree;

            if(selected_tree != null)
            {
                selected_tree.SetChildUpgradesVisible(true);
            }
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

    private Coroutine AnimateShowUpgradeTree()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Lerp.Alpha(cvg_background, 0.5f, 1).UnscaledTime();

            foreach(var tree in trees)
            {
                tree.AnimateShowMainButton();
                yield return new WaitForSecondsRealtime(0.1f);
            }

            foreach(var tree in trees)
            {
                tree.AnimateShowParentButtons();
                tree.AnimateShowChildButtons();
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
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
        ClearUpgradeTexts();
        foreach(var stat in upgrade.stats)
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