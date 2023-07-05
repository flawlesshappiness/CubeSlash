using UnityEngine;

public class UIFloatingPanel : MonoBehaviourExtended
{
    public RectTransform RectTransform { get { return GetComponentOnce<RectTransform>(); } }
    public ContentSizeFitterRefresh ContentSizeFitterRefresh { get { return GetComponentOnce<ContentSizeFitterRefresh>(); } }

    private Transform _target;
    private Vector3 _offset;

    private void Update()
    {
        if (_target == null) return;
        UpdatePosition();
    }

    public void SetTarget(Transform target, Vector3 offset)
    {
        _target = target;
        _offset = offset;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        var next_position = GetPosition();
        if (next_position != transform.position)
        {
            transform.position = next_position;
            ContentSizeFitterRefresh.RefreshContentFitters();
        }
    }

    private Vector3 GetPosition()
    {
        return _target.position + _offset;
    }
}