using UnityEngine;

public class ProgressController : Singleton
{
    public static ProgressController Instance { get { return Instance<ProgressController>(); } }

    public int ProgressCounter { get; private set; }

    public event System.Action<int> OnProgress;

    private void Start()
    {
        GameController.Instance.OnNextLevel += IncrementProgressCounter;
        Player.Instance.onLevelUp += IncrementProgressCounter;
    }

    private void IncrementProgressCounter()
    {
        ProgressCounter++;
        OnProgress?.Invoke(ProgressCounter);
    }
}