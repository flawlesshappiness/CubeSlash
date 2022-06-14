using UnityEngine;

public class Experience : MonoBehaviour
{
    private void Update()
    {
        DespawnUpdate();
    }

    private void DespawnUpdate()
    {
        if (Vector3.Distance(transform.position, Player.Instance.transform.position) > CameraController.Instance.Width * 2)
        {
            // Despawn
        }
    }
}