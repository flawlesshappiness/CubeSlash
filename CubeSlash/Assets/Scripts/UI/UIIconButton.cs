using UnityEngine;
using UnityEngine.UI;

public class UIIconButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private SelectableMenuItem btn;
    [SerializeField] private RectTransform pivot_animation;

    public SelectableMenuItem Button { get { return btn; } }
    public Sprite Icon { set { img_icon.sprite = value; } }
    public RectTransform AnimationPivot { get { return pivot_animation; } }
}