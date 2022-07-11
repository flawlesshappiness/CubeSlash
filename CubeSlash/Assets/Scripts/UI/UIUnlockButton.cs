using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnlockButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private EventSystemButton btn;

    public EventSystemButton Button { get { return btn; } }
    public bool Interactable { set { btn.interactable = value; } }
    public Sprite Icon { set { img_icon.sprite = value; } }

    private void Start()
    {
        btn.OnSelectionChanged += OnSelectionChanged;
    }

    public void Select()
    {
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }
    
    private void OnSelectionChanged(bool selected)
    {
        var end = selected ? Vector3.one * 1.25f : Vector3.one;
        Lerp.Scale(transform, 0.15f, end)
            .UnscaledTime();
    }
}