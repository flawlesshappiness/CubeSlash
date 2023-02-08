using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Flawliz.Lerp;

public class UIIconButton : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private SelectableMenuItem btn;

    public SelectableMenuItem Button { get { return btn; } }
    public Sprite Icon { set { img_icon.sprite = value; } }
}