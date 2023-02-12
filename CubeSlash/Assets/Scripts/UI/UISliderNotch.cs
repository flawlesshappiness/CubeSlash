using UnityEngine;
using UnityEngine.UI;

public class UISliderNotch : MonoBehaviour
{
    [SerializeField] private Image imgSelected, imgUnselected, imgHover;

    public int Index { get; set; }
    public bool Selected { get; private set; }

    private void Start()
    {
        imgHover.enabled = false;
    }

    public void SetSelected(bool selected)
    {
        imgSelected.enabled = selected;
        imgUnselected.enabled = !selected;
        if (selected) imgHover.enabled = false;
        Selected = selected;
    }
}