using TMPro;
using UnityEngine;

public class UIFloatingTextBox : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private CanvasGroup cvg;

    public string Text { get { return tmp.text; } set { tmp.text = value; } }
    public CanvasGroup CanvasGroup { get { return cvg; } }

    public enum Orientation { Left, Right, Top, Bottom }

    private void OnValidate()
    {
        rectTransform = GetComponent<RectTransform>();
        tmp = GetComponentInChildren<TMP_Text>();
        cvg = GetComponentInChildren<CanvasGroup>();
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetPivot(Orientation orientation)
    {
        rectTransform.pivot = GetOrientationPivot(orientation);
    }

    public void SetPosition(RectTransform rt, Orientation orientation, Vector2 offset)
    {
        var dir = GetOrientationDirection(orientation);
        var rt_size = rt.rect.size;
        var position = rt.transform.position.ToVector2() + (rt_size * dir) + offset;
        SetPivot(orientation);
        SetPosition(position);
    }

    private Vector2 GetOrientationPivot(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.Top => new Vector2(0.5f, -1),
            Orientation.Bottom => new Vector2(0.5f, 1),
            Orientation.Left => new Vector2(1, 0.5f),
            Orientation.Right => new Vector2(-1, 0.5f),
        };
    }

    private Vector2 GetOrientationDirection(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.Top => new Vector2(0, 1),
            Orientation.Bottom => new Vector2(0, -1),
            Orientation.Left => new Vector2(-1, 0),
            Orientation.Right => new Vector2(1, 0),
        };
    }
}