using UnityEngine;
using UnityEngine.UI;

public class UIIconButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private SelectableMenuItem btn;
    [SerializeField] private Image img_wrong;
    [SerializeField] private RectTransform pivot_animation;

    public SelectableMenuItem Button { get { return btn; } }
    public Sprite Icon { set { img_icon.sprite = value; } }
    public bool IconEnabled { set { img_icon.enabled = value; } }
    public RectTransform AnimationPivot { get { return pivot_animation; } }
    public bool IsWrong { set { img_wrong.gameObject.SetActive(value); } }

    private void Awake()
    {
        IsWrong = false;
    }
}