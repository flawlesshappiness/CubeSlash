using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public Area Area { get; set; }

    protected virtual void OnEnable()
    {
        ObjectController.Instance.Add(gameObject);
    }

    protected virtual void OnDestroy()
    {
        ObjectController.Instance.Remove(gameObject);
    }

    protected virtual void Update()
    {
        RepositionCheck();
    }

    public void RepositionCheck()
    {
        if (Player.Instance == null) return;

        var size = GamemodeController.Instance.SelectedGameMode.area_size;
        var sh = size * 0.5f;
        var center = Player.Instance.transform.position;
        var pos = transform.position;

        var x_neg = pos.x < center.x - sh;
        var x_pos = pos.x > center.x + sh;
        var y_neg = pos.y < center.y - sh;
        var y_pos = pos.y > center.y + sh;

        var any = x_neg || x_pos || y_neg || y_pos;

        if (x_neg) transform.position += new Vector3(size, 0);
        if (x_pos) transform.position -= new Vector3(size, 0);
        if (y_neg) transform.position += new Vector3(0, size);
        if (y_pos) transform.position -= new Vector3(0, size);

        if (any)
        {
            OnReposition();
        }
    }

    protected virtual void OnReposition()
    {
        if (Area != null && AreaController.Instance.CurrentArea != Area)
        {
            Destroy(gameObject);
        }
    }
}