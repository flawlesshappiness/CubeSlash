using TMPro;
using UnityEngine;

public class UITextObject : MonoBehaviour
{
    private TMP_Text tmp;

    public string Text { set { tmp.text = value; } }

    private void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>();
    }
}