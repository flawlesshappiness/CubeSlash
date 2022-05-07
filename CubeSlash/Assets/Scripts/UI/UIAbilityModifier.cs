using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityModifier : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp_name;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private Image img_icon;
    [SerializeField] private Button btn;

    public int Index { get; set; }
    public Button Button { get { return btn; } }

    public string Name { set { tmp_name.text = value; } }
    public string Desc { set { tmp_desc.text = value; } }
    public Sprite Sprite { set { img_icon.sprite = value; } }
    public Button.ButtonClickedEvent OnClick { get { return btn.onClick; } }
}
