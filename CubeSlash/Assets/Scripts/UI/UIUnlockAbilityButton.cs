using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnlockAbilityButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private TMP_Text tmp_header;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private Button btn;

    public Button.ButtonClickedEvent OnClick { get { return btn.onClick; } }
    public bool Interactable { set { btn.interactable = value; } }

    public string Header { set { tmp_header.text = value; } }
    public string Description { set { tmp_desc.text = value; } }
    public Sprite Icon { set { img_icon.sprite = value; } }

    public void Select()
    {
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }
}