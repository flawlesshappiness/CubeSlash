using Flawliz.Lerp;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILock : MonoBehaviour
{
    [SerializeField] private RectTransform pivot_anim_lock;
    [SerializeField] private Image img_lock;
    [SerializeField] private TMP_Text tmp_price;
    [SerializeField] private CanvasGroup cvg;

    public int Price { set { tmp_price.text = CurrencyController.FormatCurrencyString(value); ; } }
    public string Text { set { tmp_price.text = value; } }

    public void SetLocked()
    {
        cvg.alpha = 1;
        tmp_price.enabled = true;
        pivot_anim_lock.localScale = Vector3.one;
    }

    public void SetUnlocked()
    {
        cvg.alpha = 0;
        tmp_price.enabled = false;
    }

    public Coroutine AnimateUnlock()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            tmp_price.enabled = false;
            yield return LerpEnumerator.Value(0.5f, f =>
            {
                pivot_anim_lock.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, f);
                cvg.alpha = Mathf.Lerp(1f, 0f, f);
            });
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