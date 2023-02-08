using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Flawliz.Lerp;

public class UIIconButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private SelectableMenuItem btn;
    [SerializeField] private RectTransform pivot_animation;
    [SerializeField] private ParticleSystem ps_glow;

    public bool glowing;

    public SelectableMenuItem Button { get { return btn; } }
    public Sprite Icon { set { img_icon.sprite = value; } }
    public RectTransform AnimationPivot { get { return pivot_animation; } }

    private void Start()
    {
        ps_glow.gameObject.SetActive(glowing);
    }
}