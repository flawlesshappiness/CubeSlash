using Flawliz.VisualConsole;
using System.Linq;
using UnityEngine;

public class DebugConsoleHandler : Singleton
{
    public static DebugConsoleHandler Instance { get { return Instance<DebugConsoleHandler>(); } }

    private VisualConsoleView view;
    private DebugInfoView info_view;

    protected override void Initialize()
    {
        base.Initialize();

        PlayerInputController.Instance.Debug.Pressed += ToggleView;
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
            window.CreateButton(GameController.DAMAGE_DISABLED ? "Enable damage" : "Disable damage", ClickToggleDamage);
            window.CreateButton("Set Area", ClickSetArea);
            window.CreateButton("Suicide", ClickSuicide);
            window.CreateButton("Win", ClickWin);
            window.CreateButton("Spawn Boss", ClickSpawnBoss);
            window.CreateButton(EnemyController.Instance.EnemySpawnEnabled ? "Disable enemy spawn" : "Enable enemy spawn", ClickToggleEnemySpawn);
            window.CreateButton("Kill Enemies", ClickKillEnemies);
            window.CreateButton("Fill mana", ClickFillMana);
            window.CreateButton("Toggle DebugInfoView", ClickToggleDebugInfoView);
        }
        else
        {
            window.CreateButton("Unlock Ability", ClickUnlockAbility);
            window.CreateButton("Unlock Bodypart", ClickUnlockBodypart);
            window.CreateButton("Unlock Body", ClickUnlockBody);
            window.CreateButton("Test unlock item", ClickTestUnlockItem);
        }

        window.CreateButton("Log", ClickLog);
        window.CreateButton("Test SaveData", ClickTestSaveData);
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
            btn.TextRight = is_unlocked ? "Unlocked" : "";
        }

        void Click(ListButton btn, BodypartInfo info)
        {
            BodypartController.Instance.UnlockPart(info);
            btn.TextRight = "Unlocked";
        }
    }

    private void ClickUnlockBody()
    {
        var window = view.ShowList();
        view.ShowBackButton(ShowFunctionsWindow);
        window.Clear();

        var db = Database.Load<PlayerBodyDatabase>();
        foreach (var info in db.collection)
        {
            var btn = window.CreateButton(info.name);
            btn.onClick.AddListener(() => Click(btn, info));
            var is_unlocked = Save.Game.unlocked_player_bodies.Contains(info.type);
            btn.TextRight = is_unlocked ? "Unlocked" : "";
        }

        void Click(ListButton btn, PlayerBodyInfo info)
        {
            if (Save.Game.unlocked_player_bodies.Contains(info.type)) return;

            Save.Game.unlocked_player_bodies.Add(info.type);
            Save.Game.new_player_bodies.Add(info.type);
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
            CreateButton(area);
        }
        CreateButton(db.final_area);

        void CreateButton(Area area)
        {
            var btn = window.CreateButton(area.name);
            btn.onClick.AddListener(() => SetArea(area));
        }

        void SetArea(Area area)
        {
            AreaController.Instance.DebugSetArea(area);
            CloseView();
        }
    }

    private void ClickWin()
    {
        GameController.Instance.Win();
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
        Player.Instance.heal.SetFull();
    }

    private void ClickToggleDebugInfoView()
    {
        if (info_view == null)
        {
            info_view = ViewController.Instance.ShowView<DebugInfoView>(0, nameof(DebugInfoView));
        }
        else
        {
            info_view.gameObject.SetActive(!info_view.gameObject.activeInHierarchy);
        }
    }

    private void ClickTestSaveData()
    {
        Save.PlayerBody.Clear();
        Player.Instance.Clear();

        var save = Save.Game;
        save.Clear();

        save.idx_difficulty_completed = 1;

        SaveDataController.Instance.SaveAll();

        CloseView();
    }
}
