using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] public Transform world;

    public static GameController Instance;

    private void Awake()
    {
        Instance = this;
        InitializeControllers();
        InitializePlayer();
        StartLevel();
    }

    private void InitializeControllers()
    {
        foreach(var controller in GetComponentsInChildren<IInitializable>())
        {
            controller.Initialize();
        }
    }

    private void InitializePlayer()
    {
        var prefab_player = Resources.Load<GameObject>("Prefabs/Entities/Player");
        Player.Instance = Instantiate(prefab_player, world).GetComponent<Player>();
        Player.Instance.Initialize();
        Player.Instance.InputEnabled = true;
        Player.Instance.Experience.onMax += CompleteLevel;
        Player.Instance.Health.onMin += OnPlayerDeath;
        CameraController.Instance.Target = Player.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        CheatUpdate();
    }

    private void CheatUpdate()
    {
        if(Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.Tab))
        {
            Reload();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.Tab))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.Tab))
        {
            Player.Instance.Experience.Value = Player.Instance.Experience.Max;
        }
    }

    private void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartLevel()
    {
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.InputEnabled = true;
        Player.Instance.Experience.Value = 0;
        Player.Instance.Health.Value = Player.Instance.Health.Max;
        EnemyController.Instance.Spawning = true;
        ViewController.Instance.ShowView<GameView>();
    }

    public void CompleteLevel()
    {
        StartCoroutine(CompleteLevelCr());
    }

    private IEnumerator CompleteLevelCr()
    {
        EnemyController.Instance.Spawning = false;
        EnemyController.Instance.KillActiveEnemies();
        Player.Instance.InputEnabled = false;
        yield return new WaitForSeconds(2f);
        Player.Instance.gameObject.SetActive(false);
        ViewController.Instance.ShowView<AbilityView>();
    }

    private void OnPlayerDeath()
    {
        Player.Instance.Kill();
        StartCoroutine(RestartGameCr());
    }

    private IEnumerator RestartGameCr()
    {
        yield return new WaitForSeconds(1f);
        ViewController.Instance.ShowView<DeathView>(2f);
        yield return new WaitForSeconds(4f);
        Reload();
    }
}
