using UnityEngine;

public class ScaleWithSizeDelta : MonoBehaviour
{
    [SerializeField] private RectTransform scale_target;
    [SerializeField] private RectTransform size_delta_target;

    private Vector2 size_delta_init;
    private Vector3 scale_init;

    private void OnValidate()
    {
        if(scale_target == null)
        {
            scale_target = GetComponent<RectTransform>();
        }

        if(size_delta_target == null)
        {
            if(transform.parent != null)
            {
                size_delta_target = transform.parent.GetComponent<RectTransform>();
            }
            else
            {
                size_delta_target = GetComponent<RectTransform>();
            }
        }
    }

    private void Start()
    {
        scale_init = scale_target.localScale;
        size_delta_init = size_delta_target.sizeDelta;
    }

    private void Update()
    {
        var size_delta = size_delta_target.sizeDelta;
        var t_x = size_delta.x / size_delta_init.x;
        var t_y = size_delta.y / size_delta_init.y;
        scale_target.localScale = new Vector3(scale_init.x * t_x, scale_init.y * t_y, scale_init.z);
    }
}