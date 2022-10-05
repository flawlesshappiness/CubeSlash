using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIIconButton : MonoBehaviour
{
    public bool scale_on_selected;
    [SerializeField] private Image img_icon;
    [SerializeField] private ButtonExtended btn;

    public ButtonExtended Button { get { return btn; } }
    public bool Interactable { set { btn.interactable = value; } }
    public Sprite Icon { set { img_icon.sprite = value; } }

    private void Start()
    {
        btn.OnSelectedChanged += OnSelectedChanged;
        btn.SetSelectOnHover(true);
    }

    public void Select()
    {
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }
    
    private void OnSelectedChanged(bool selected)
    {
        if (scale_on_selected)
        {
            var end = selected ? Vector3.one * 1.25f : Vector3.one;
            Lerp.Scale(transform, 0.15f, end)
                .UnscaledTime();
        }
    }
}