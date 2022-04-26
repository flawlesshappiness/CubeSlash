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
        EnemyController.Instance.Spawning = true;

        ViewController.Instance.ShowView<GameView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
        CameraController.Instance.Target = Player.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        InputUpdate();
    }

    private void InputUpdate()
    {
        if(Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.Tab))
        {
            Reload();
        }
    }

    private void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
