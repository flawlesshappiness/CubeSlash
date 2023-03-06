using Flawliz.VisualConsole;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

        if (GameController.Instance.IsGameStarted)
        {
            window.CreateButton("Unlock Upgrade", ClickUnlockUpgrade);
            window.CreateButton("Unlock Ability", ClickUnlockAbility);
            window.CreateButton("Level up", ClickLevelUp);
            window.CreateButton("Gain Ability", ClickGainAbility);
            window.CreateButton("Equipment", ClickEquipment);
            window.CreateButton(GameController.DAMAGE_DISABLED ? "Enable damage" : "Disable damage", ClickToggleDamage);
            window.CreateButton("Next Area", ClickNextArea);
            window.CreateButton("Suicide", ClickSuicide);
            window.CreateButton("Win", ClickWin);
            window.CreateButton("Spawn Boss", ClickSpawnBoss);
            window.CreateButton("Kill Enemies", ClickKillEnemies);
        }

        window.CreateButton("Give money", ClickGiveCurrency);
        window.CreateButton("Log", ClickLog);
        window.CreateButton("Game Values", ClickGameValues);
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
            var btn = window.CreateButton(name);
            btn.onClick.AddListener(() => UnlockUpgrade(btn, info));
            btn.TextRight = info.is_unlocked ? "Unlocked" : "";

            if (info.upgrade.require_ability)
            {
                btn.TextLeft = info.upgrade.ability_required.ToString();
            }
        }

        void UnlockUpgrade(ListButton btn, UpgradeInfo info)
        {
            if (!info.is_unlocked)
            {
                btn.TextRight = "Unlocked";
                UpgradeController.Instance.CheatUnlockUpgrade(info);
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
            var ability = AbilityController.Instance.GetAbilityPrefab(type);
            var btn = window.CreateButton(type.ToString());
            btn.onClick.AddListener(() => UnlockAbility(btn, type));
            var is_unlocked = AbilityController.Instance.HasGainedAbility(type);

            btn.TextRight = is_unlocked ? "Unlocked" : "";
        }

        void UnlockAbility(ListButton btn, Ability.Type type)
        {
            if (!AbilityController.Instance.HasGainedAbility(type))
            {
                AbilityController.Instance.GainAbility(type);
                btn.TextRight = "Unlocked";
            }
        }
    }

    private void ClickLog()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        foreach(var log in LogController.Instance.LoggedMessages)
        {
            window.CreateText(log.message);
        }
    }

    private void ClickLevelUp()
    {
        CloseView();
        Player.Instance.Experience.Value = Player.Instance.Experience.Max;
    }

    private void ClickGainAbility()
    {
        Player.Instance.CheatLevelsUntilNextAbility(1);
        ClickLevelUp();
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

    private void ClickSuicide()
    {
        Player.Instance.Suicide();
        CloseView();
    }

    private void ClickNextArea()
    {
        AreaController.Instance.ForceNextArea();
        CloseView();
    }

    private void ClickWin()
    {
        GameController.Instance.Win();
        CloseView();
    }

    private void ClickGiveCurrency()
    {
        CurrencyController.Instance.Gain(CurrencyType.DNA, 100000);
        CloseView();
    }

    private void ClickSpawnBoss()
    {
        EnemyController.Instance.DebugSpawnBoss();
        CloseView();
    }

    private void ClickKillEnemies()
    {
        EnemyController.Instance.KillActiveEnemies();
        CloseView();
    }

    private void ClickGameValues()
    {
        var window = view.ShowList();
        window.Clear();

        // Create texts
        var texts = new List<GameValueText>();

        if (GameController.Instance.IsGameStarted)
        {
            CreateText(() => $"Spawn frequency: {EnemyController.Instance.GetSpawnFrequency()}");
            CreateText(() => $"Spawn frequency (Difficulty): {EnemyController.Instance.GetSpawnFrequencyDifficulty()}");
            CreateText(() => $"Spawn frequency (Game): {EnemyController.Instance.GetSpawnFrequencyGame()}");
            CreateText(() => $"Spawn frequency (Endless): {EnemyController.Instance.GetSpawnFrequencyEndless()}");
            CreateText(() => $"Spawn count: {EnemyController.Instance.GetSpawnCount()}");
            CreateText(() => $"Area progress locked: {AreaController.Instance.NextAreaLock.IsLocked}");
        }

        // Start
        var cr = StartCoroutine(Cr());
        view.ShowBackButton(Back);
        
        void Back()
        {
            StopCoroutine(cr);
            ShowFunctionsWindow();
        }

        void CreateText(System.Func<string> getString)
        {
            var text = new GameValueText
            {
                tmp = window.CreateText(""),
                getString = getString,
            };
            texts.Add(text);
        }
        
        IEnumerator Cr()
        {
            while (true)
            {
                texts.ForEach(t => t.UpdateText());
                yield return null;
            }
        }
    }

    private class GameValueText
    {
        public TMP_Text tmp;
        public System.Func<string> getString;
        public void UpdateText() => tmp.text = getString();
    }
}
