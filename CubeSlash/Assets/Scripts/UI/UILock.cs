using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UILock : MonoBehaviour
{
    [SerializeField] private RectTransform pivot_anim_lock;
    [SerializeField] private Image img_lock;
    [SerializeField] private TMP_Text tmp_price;

    public int Price { set { tmp_price.text = CurrencyController.FormatCurrencyString(value); ; } }

    public Coroutine AnimateUnlock()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return null;
        }
    }

    public Coroutine AnimateLocked()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return null;
        }
    }
}