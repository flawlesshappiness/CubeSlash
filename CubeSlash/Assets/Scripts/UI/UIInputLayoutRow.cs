using TMPro;
using UnityEngine;

public class UIInputLayoutRow : MonoBehaviour
{
    [SerializeField] private ImageInput img;
    [SerializeField] private TMP_Text tmp;

    public ImageInput Image { get { return img; } }
    public string Text { set { tmp.text = value; } }
}