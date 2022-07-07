using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnlockAbilityButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private Button btn;

    public Button.ButtonClickedEvent OnClick { get { return btn.onClick; } }
    public System.Action<Ability> OnHighlighted { get; set; }
    public bool Interactable { set { btn.interactable = value; } }
    private bool Highlighted { get; set; }
    public Sprite Icon { set { img_icon.sprite = value; } }
    public Ability Ability { get; set; }

    public void Select()
    {
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }

    private void Update()
    {
        HighlightUpdate();
    }

    private void HighlightUpdate()
    {
        var highlight = EventSystemController.Instance.EventSystem.currentSelectedGameObject == btn.gameObject;
        if (Highlighted != highlight)
        {
            Highlighted = highlight;
            if (Highlighted)
            {
                OnHighlighted?.Invoke(Ability);
            }
        }
    }
}