using Flawliz.Console;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;
    public static bool DAMAGE_DISABLED = false;

    public System.Action OnResume { get; set; }
    public bool IsGameStarted { get; private set; }
    public bool IsPaused { get { return PauseLock.IsLocked; } }
    public MultiLock PauseLock { get; private set; } = new MultiLock();
    public int LevelIndex { get; set; }

    public System.Action OnNextLevel { get; set; }

    public const string TAG_ABILITY_VIEW = "Ability";

    private void Awake()
    {
        Instance = this;
        InitializePlayer();
        InitializeData();
        InitializeController();
        ViewController.Instance.ShowView<StartView>(0);

        PauseLock.OnLockChanged += OnPauseChanged;

        ConsoleController.Instance.RegisterCommand("UnlockAllAbilities", AbilityController.Instance.UnlockAllAbilities);
        ConsoleController.Instance.RegisterCommand("Kill", EnemyController.Instance.KillActiveEnemies);
        ConsoleController.Instance.RegisterCommand("LevelUp", CheatLevelUp);
        ConsoleController.Instance.RegisterCommand("AbilityPoints", CheatAbilityPoints);
        ConsoleController.Instance.RegisterCommand("NextLevel", () => SetLevel(LevelIndex + 1));
        ConsoleController.Instance.RegisterCommand("GainAbility", OnPlayerGainAbility);
        ConsoleController.Instance.RegisterCommand("Equipment", CheatOpenEquipment);
        ConsoleController.Instance.RegisterCommand("ToggleDamage", () => DAMAGE_DISABLED = !DAMAGE_DISABLED);
        ConsoleController.Instance.RegisterCommand("Suicide", () => Player.Instance.Kill());
        ConsoleController.Instance.onToggle += OnToggleConsole;
    }

    private void OnToggleConsole(bool toggle)
    {
        if (toggle)
        {
            PauseLock.AddLock("Console");
        }
        else
        {
            PauseLock.RemoveLock("Console");
        }
    }

    private void InitializePlayer()
    {
        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.onDeath += OnPlayerDeath;
        Player.Instance.onLevelUp += OnPlayerLevelUp;
        CameraController.Instance.Target = Player.Instance.transform;
        Player.Instance.gameObject.SetActive(false);
    }

    private void InitializeData()
    {
        Data.SaveGameData();
    }

    private void InitializeController()
    {
        Singleton.EnsureExistence<EnemyController>();
        Singleton.EnsureExistence<ItemController>();
        Singleton.EnsureExistence<PlayerInputController>();
        Singleton.EnsureExistence<BackgroundController>();
    }

    private void OnPauseChanged(bool paused)
    {
        SetTimeScale(paused ? 0 : 1);
    }

    private void SetTimeScale(float time)
    {
        Time.timeScale = PauseLock.IsLocked ? 0 : time;
    }

    private Coroutine LerpTimeScale(float duration, float end)
    {
        return Lerp.Value(duration, Time.timeScale, end, "TimeScale", f =>
        {
            SetTimeScale(f);
        }).UnscaledTime().GetCoroutine();
    }

    private void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartGame()
    {
        IsGameStarted = true;
        Player.Instance.Experience.Value = 0;
        Player.Instance.Health.Value = Player.Instance.Health.Max;
        Player.Instance.ReapplyAbilities();

        PauseLevel();
        var view_unlock = ViewController.Instance.ShowView<UnlockAbilityView>(0);
        view_unlock.OnAbilitySelected += () =>
        {
            var view_ability = ViewController.Instance.ShowView<AbilityView>(0);
            view_ability.OnContinue += () =>
            {
                ViewController.Instance.ShowView<GameView>(1);
                Player.Instance.gameObject.SetActive(true);
                SetLevel(0);
                ResumeLevel();
            };
        };
    }

    private void PauseLevel()
    {
        PauseLock.AddLock(nameof(GameController));
    }

    private void ResumeLevel()
    {
        PauseLock.RemoveLock(nameof(GameController));
        Player.Instance.ReapplyAbilities();
        OnResume?.Invoke();
    }

    private Coroutine _cr_next_level;
    private IEnumerator NextLevelCr()
    {
        while (true)
        {
            yield return new WaitForSeconds(Level.Current.duration);
            Level.Completed();
            OnNextLevel?.Invoke();
        }
    }

    private void SetLevel(int idx)
    {
        if(_cr_next_level != null)
        {
            StopCoroutine(_cr_next_level);
        }

        LevelIndex = idx;
        _cr_next_level = StartCoroutine(NextLevelCr());
    }

    private void OnPlayerLevelUp()
    {
        Player.Instance.ResetExperience();
        if (UpgradeController.Instance.CanUnlockUpgrade())
        {
            UnlockUpgrade();
        }
    }

    public void OnPlayerGainAbility()
    {
        if (AbilityController.Instance.CanUnlockAbility())
        {
            UnlockAbility();
        }
        else if (UpgradeController.Instance.CanUnlockUpgrade())
        {
            UnlockUpgrade();
        }
    }

    private void UnlockUpgrade()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpTimeScale(1f, 0f);
            PauseLevel();
            var view = ViewController.Instance.ShowView<UnlockUpgradeView>(0, TAG_ABILITY_VIEW);
            view.OnUpgradeSelected += () =>
            {
                ResumeLevel();
            };
        }
    }

    private void UnlockAbility()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpTimeScale(1f, 0f);
            PauseLevel();
            var view_unlock = ViewController.Instance.ShowView<UnlockAbilityView>(0, TAG_ABILITY_VIEW);
            view_unlock.OnAbilitySelected += () =>
            {
                var view_ability = ViewController.Instance.ShowView<AbilityView>(0, TAG_ABILITY_VIEW);
                view_ability.OnContinue += () =>
                {
                    ResumeLevel();
                };
            };
        }
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(RestartGameCr());
    }

    private IEnumerator RestartGameCr()
    {
        IsGameStarted = false;
        yield return new WaitForSeconds(0.5f);
        ViewController.Instance.ShowView<DeathView>(2f);
        yield return new WaitForSeconds(2.5f);
        ViewController.Instance.CloseView(0.5f);
        yield return new WaitForSeconds(0.5f);
        EnemyController.Instance.KillActiveEnemies();
        ItemController.Instance.DespawnAllActiveItems();
        Player.Instance.gameObject.SetActive(false);
        ViewController.Instance.ShowView<StartView>(0.25f);
    }

    private void CheatLevelUp()
    {
        Player.Instance.Experience.Value = Player.Instance.Experience.Max;
    }

    private void CheatAbilityPoints()
    {
        Player.Instance.AbilityPoints += 999;
    }

    private void CheatOpenEquipment()
    {
        SetTimeScale(0);
        ViewController.Instance.ShowView<AbilityView>(0, TAG_ABILITY_VIEW);
    }
}
