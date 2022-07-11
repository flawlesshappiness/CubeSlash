using Flawliz.Console;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;

    public System.Action OnResume { get; set; }
    public bool IsGameStarted { get; private set; }
    public bool IsPaused { get { return PauseLock.IsLocked; } }
    public MultiLock PauseLock { get; private set; } = new MultiLock();
    public int LevelIndex { get; set; }

    public System.Action OnNextLevel { get; set; }

    private void Awake()
    {
        Instance = this;
        InitializePlayer();
        InitializeData();
        InitializeController();
        StartGame();

        PauseLock.OnLockChanged += OnPauseChanged;

        ConsoleController.Instance.RegisterCommand("UnlockAllAbilities", Player.Instance.UnlockAllAbilities);
        ConsoleController.Instance.RegisterCommand("Kill", EnemyController.Instance.KillActiveEnemies);
        ConsoleController.Instance.RegisterCommand("LevelUp", CheatLevelUp);
        ConsoleController.Instance.RegisterCommand("AbilityPoints", CheatAbilityPoints);
        ConsoleController.Instance.RegisterCommand("NextLevel", NextLevel);
        ConsoleController.Instance.RegisterCommand("GainAbility", OnPlayerGainAbility);
    }

    private void InitializePlayer()
    {
        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.onDeath += OnPlayerDeath;
        Player.Instance.onLevelUp += OnPlayerLevelUp;
        CameraController.Instance.Target = Player.Instance.transform;
    }

    private void InitializeData()
    {
        Data.SaveGameData();
    }

    private void InitializeController()
    {
        Singleton.EnsureExistence<EnemyController>();
        Singleton.EnsureExistence<ExperienceController>();
        Singleton.EnsureExistence<PlayerInputController>();
    }

    // Update is called once per frame
    void Update()
    {
        LevelUpdate();
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
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.Experience.Value = 0;
        Player.Instance.Health.Value = Player.Instance.Health.Max;
        Player.Instance.InitializeAbilities();

        LevelIndex = 0;
        time_level = Time.time + Level.Current.duration;
        ViewController.Instance.ShowView<GameView>();
    }

    private float time_level;
    private void LevelUpdate()
    {
        if (!IsGameStarted) return;
        if (Time.time < time_level) return;
        NextLevel();
    }

    private void NextLevel()
    {
        Level.Completed();
        time_level = Time.time + Level.Current.duration;
        print("Next level " + LevelIndex);
        OnNextLevel?.Invoke();
    }

    private void OnPlayerLevelUp()
    {
        Player.Instance.ResetExperience();
        if (UnlockView.CanUnlockUpgrade())
        {
            UnlockUpgrade();
        }
    }

    public void OnPlayerGainAbility()
    {
        if (UnlockView.CanUnlockAbility())
        {
            UnlockAbility();
        }
        else if (UnlockView.CanUnlockUpgrade())
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
            PauseLock.AddLock(nameof(GameController));
            var view = ViewController.Instance.ShowView<UnlockView>(0, "Ability");
            view.UnlockUpgrade();
        }
    }

    private void UnlockAbility()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return LerpTimeScale(1f, 0f);
            PauseLock.AddLock(nameof(GameController));
            var view = ViewController.Instance.ShowView<UnlockView>(0, "Ability");
            view.UnlockAbility();
        }
    }

    public void ResumeLevel()
    {
        PauseLock.RemoveLock(nameof(GameController));
        Player.Instance.InitializeAbilities();
        OnResume?.Invoke();
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(RestartGameCr());
    }

    private IEnumerator RestartGameCr()
    {
        yield return new WaitForSeconds(1f);
        ViewController.Instance.ShowView<DeathView>(2f);
    }

    private void CheatLevelUp()
    {
        Player.Instance.Experience.Value = Player.Instance.Experience.Max;
    }

    private void CheatAbilityPoints()
    {
        Player.Instance.AbilityPoints += 999;
    }
}
