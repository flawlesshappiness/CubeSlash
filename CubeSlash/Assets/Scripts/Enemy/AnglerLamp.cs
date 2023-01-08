using UnityEngine;

public class AnglerLamp : MonoBehaviour
{
    public float speed;
    public float max_distance;

    private Transform target;

    public void SetTarget(Transform target) => this.target = target;

    public void ResetPosition()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    private void Update()
    {
        if (target == null) return;

        var t = speed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, target.position, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, t);

        var dir = target.position - transform.position;
        if (dir.magnitude > max_distance) transform.position = target.position - dir.normalized * max_distance;
    }
}