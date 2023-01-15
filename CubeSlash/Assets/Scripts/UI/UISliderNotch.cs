using UnityEngine;
using UnityEngine.UI;

public class UISliderNotch : MonoBehaviour
{
    [SerializeField] private Image imgSelected, imgUnselected, imgHover;
    [SerializeField] private ButtonExtended btn;

    public System.Action onClicked;

    public int Index { get; set; }
    public bool Selected { get; private set; }

    private void Start()
    {
        btn.onClick.AddListener(OnClick);
        btn.OnHoverChanged += OnHover;
        imgHover.enabled = false;
    }

    public void SetSelected(bool selected)
    {
        imgSelected.enabled = selected;
        imgUnselected.enabled = !selected;
        if (selected) imgHover.enabled = false;
        Selected = selected;
    }
    
    private void OnClick()
    {
        onClicked?.Invoke();
    }

    private void OnHover(bool hover)
    {
        imgHover.enabled = hover && !Selected;
        imgUnselected.enabled = !hover && !Selected;
    }
}