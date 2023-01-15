using Flawliz.VisualConsole;
using System.Linq;
using UnityEngine;

public class DebugConsoleHandler : Singleton
{
    public static DebugConsoleHandler Instance { get { return Instance<DebugConsoleHandler>(); } }

    private VisualConsoleView view;

    protected override void Initialize()
    {
        base.Initialize();

        // Input
        var ui = PlayerInput.Controls.UI;
        ui.DebugConsole.started += c => ToggleView();
    }

    private void ToggleView()
    {
        if(VisualConsoleController.Instance.ToggleView(out view))
        {
            ShowFunctionsWindow();
            GameController.Instance.PauseLock.AddLock(nameof(DebugConsoleHandler));
        }
        else
        {
            CloseView();
        }
    }

    private void CloseView()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(DebugConsoleHandler));
        VisualConsoleController.Instance.HideView();
    }

    private void ShowFunctionsWindow()
    {
        var window = view.ShowGrid();
        window.Clear();
        window.CreateButton("Unlock Upgrade", ClickUnlockUpgrade);
        window.CreateButton("Unlock Ability", ClickUnlockAbility);
        window.CreateButton("Level up", ClickLevelUp);
        window.CreateButton("Equipment", ClickEquipment);
        window.CreateButton(GameController.DAMAGE_DISABLED ? "Enable damage" : "Disable damage", ClickToggleDamage);
    }

    private void ClickUnlockUpgrade()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);

        window.Clear();
        var infos = UpgradeController.Instance.GetUpgradeInfos();
        foreach(var info in infos)
        {
            var name = $"{info.upgrade.id}";
            var btn = window.CreateButton(name, () => UnlockUpgrade(info));
            btn.TextRight = info.isUnlocked ? "Unlocked" : "";

            if (info.require_ability)
            {
                btn.TextLeft = info.type_ability_required.ToString();
            }
        }

        void UnlockUpgrade(UpgradeInfo info)
        {
            if (!info.isUnlocked)
            {
                UpgradeController.Instance.CheatUnlockUpgrade(info);
                ShowFunctionsWindow();
            }
        }
    }

    private void ClickUnlockAbility()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        var types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToList();
        foreach(var type in types)
        {
            var ability = AbilityController.Instance.GetAbility(type);
            var btn = window.CreateButton(type.ToString(), () => UnlockAbility(ability));
            var is_unlocked = AbilityController.Instance.IsAbilityUnlocked(type);

            btn.TextRight = is_unlocked ? "Unlocked" : "";
        }

        void UnlockAbility(Ability ability)
        {
            AbilityController.Instance.UnlockAbility(ability.Info.type);
            ShowFunctionsWindow();
        }
    }

    private void ClickLevelUp()
    {
        CloseView();
        Player.Instance.Experience.Value = Player.Instance.Experience.Max;
    }

    private void ClickEquipment()
    {
        CloseView();

        var view = ViewController.Instance.ShowView<AbilityView>(0, GameController.TAG_ABILITY_VIEW);
        view.OnContinue += () =>
        {
            GameController.Instance.ResumeLevel();
        };
    }

    private void ClickToggleDamage()
    {
        GameController.DAMAGE_DISABLED = !GameController.DAMAGE_DISABLED;
        CloseView();
    }
}