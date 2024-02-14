using Flawliz.VisualConsole;
using Steamworks;
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
        if (!Application.isEditor) return;

        if (VisualConsoleController.Instance.ToggleView(out view))
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
            window.CreateButton("Gain Ability", ClickGainAbility);
            window.CreateButton("Level up", ClickLevelUp);
            window.CreateButton("Level up (Ability)", ClickLevelUpAbility);
            //window.CreateButton("Equipment", ClickEquipment);
            window.CreateButton(GameController.DAMAGE_DISABLED ? "Enable damage" : "Disable damage", ClickToggleDamage);
            window.CreateButton("Set Area", ClickSetArea);
            window.CreateButton("Next Area", ClickNextArea);
            window.CreateButton("Final Area", ClickFinalArea);
            window.CreateButton("Suicide", ClickSuicide);
            window.CreateButton("Win", ClickWin);
            window.CreateButton("Spawn Boss", ClickSpawnBoss);
            window.CreateButton(EnemyController.Instance.EnemySpawnEnabled ? "Disable enemy spawn" : "Enable enemy spawn", ClickToggleEnemySpawn);
            window.CreateButton("Kill Enemies", ClickKillEnemies);
            window.CreateButton("Fill mana", ClickFillMana);
        }
        else
        {
            window.CreateButton("Unlock Ability", ClickUnlockAbility);
            window.CreateButton("Unlock Bodypart", ClickUnlockBodypart);
            window.CreateButton("Test unlock item", ClickTestUnlockItem);
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
        var infos = UpgradeController.Instance.GetUpgradeInfos()
            .OrderBy(info => info.upgrade.id.id);
        foreach (var info in infos)
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

    private void ClickGainAbility()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        var types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToList();
        foreach (var type in types)
        {
            var ability = AbilityController.Instance.GetAbilityPrefab(type);
            var btn = window.CreateButton(type.ToString());
            btn.onClick.AddListener(() => Click(btn, type));
            var is_unlocked = AbilityController.Instance.HasGainedAbility(type);

            btn.TextRight = is_unlocked ? "Gained" : "";
        }

        void Click(ListButton btn, Ability.Type type)
        {
            if (AbilityController.Instance.HasGainedAbility(type)) return;
            AbilityController.Instance.GainAbility(type);
            btn.TextRight = "Gained";
        }
    }

    private void ClickUnlockAbility()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        var types = System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>().ToList();
        foreach (var type in types)
        {
            var ability = AbilityController.Instance.GetAbilityPrefab(type);
            var btn = window.CreateButton(type.ToString());
            btn.onClick.AddListener(() => Click(btn, type));
            var is_unlocked = AbilityController.Instance.IsAbilityUnlocked(type);
            btn.TextRight = is_unlocked ? "Gained" : "";
        }

        void Click(ListButton btn, Ability.Type type)
        {
            if (AbilityController.Instance.IsAbilityUnlocked(type)) return;
            AbilityController.Instance.UnlockAbility(type);
            btn.TextRight = "Gained";
        }
    }

    private void ClickUnlockBodypart()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        var db = Database.Load<BodypartDatabase>();
        foreach (var info in db.collection)
        {
            if (info.is_ability_part) continue;

            var btn = window.CreateButton(info.name);
            btn.onClick.AddListener(() => Click(btn, info));
            var is_unlocked = Save.Game.unlocked_bodyparts.Contains(info.type);
            btn.TextRight = is_unlocked ? "Gained" : "";
        }

        void Click(ListButton btn, BodypartInfo info)
        {
            BodypartController.Instance.UnlockPart(info);
            btn.TextRight = "Unlocked";
        }
    }

    private void ClickLog()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        foreach (var log in LogController.Instance.LoggedMessages)
        {
            var message = log.GetDebugMessage();
            message = log.log_type == LogType.Exception ? message.Color(new Color(0.9f, 0.3f, 0.3f)) : message;
            window.CreateText(message);
        }
    }

    private void ClickLevelUp()
    {
        CloseView();
        Player.Instance.Experience.Value = Player.Instance.Experience.Max;
    }

    private void ClickLevelUpAbility()
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

    private void ClickSetArea()
    {
        var window = view.ShowList();
        window.Clear();

        var db = Database.Load<AreaDatabase>();
        foreach (var area in db.collection)
        {
            var btn = window.CreateButton(area.name);
            btn.onClick.AddListener(() => SetArea(area));
        }

        void SetArea(Area area)
        {
            AreaController.Instance.SetArea(area);
            CloseView();
        }
    }

    private void ClickNextArea()
    {
        AreaController.Instance.ForceNextArea();
        CloseView();
    }

    private void ClickFinalArea()
    {
        AreaController.Instance.ForceFinalArea();
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

    private void ClickToggleEnemySpawn()
    {
        EnemyController.Instance.EnemySpawnEnabled = !EnemyController.Instance.EnemySpawnEnabled;
        CloseView();
    }

    private void ClickKillEnemies()
    {
        EnemyController.Instance.KillActiveEnemies();
        CloseView();
    }

    private void ClickTestUnlockItem()
    {
        var db = Database.Load<BodypartDatabase>();
        var item = db.collection.FirstOrDefault(info => info.type == BodypartType.eyestalk_A);
        var view = ViewController.Instance.ShowView<UnlockItemView>(0f, nameof(UnlockItemView));
        view.SetTitle("Bodypart unlocked!");
        view.SetSprite(item.preview);
        view.OnSubmit += () => Debug.Log("Completed");
        view.Animate();
        CloseView();
    }

    private void ClickFillMana()
    {
        Player.Instance.heal.SetMax();
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
            CreateText(() => $"Spawn count: {EnemyController.Instance.GetSpawnCount()}");
        }

        CreateText(() => $"GameSaveData from cloud: {SaveDataController.Instance.Get<GameSaveData>().from_cloud}");
        CreateText(() => $"PlayerBodySaveData from cloud: {SaveDataController.Instance.Get<PlayerBodySaveData>().from_cloud}");

        CreateText(() => "");
        CreateText(() => $"Steam IsValid: {SteamClient.IsValid}");
        CreateText(() => $"Steam Username: {SteamClient.Name}");
        CreateText(() => $"Steam Achievements: {SteamUserStats.Achievements.Count()}");
        CreateText(() => $"Steam Overlay Enabled: {SteamUtils.IsOverlayEnabled}");

        CreateText(() => $"Steam Cloud Files: {SteamRemoteStorage.FileCount}");
        var files = SteamRemoteStorage.Files;
        foreach (var file in files)
        {
            CreateText(() => $"Steam Cloud File: {file}");
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
