using UnityEngine;

public class UICurrencyBarSound : MonoBehaviour
{
    [SerializeField] private UICurrencyBar currency_bar;
    private void Reset()
    {
        currency_bar = currency_bar ?? GetComponent<UICurrencyBar>();
    }

    private void Start()
    {
        currency_bar = currency_bar ?? GetComponent<UICurrencyBar>();
        currency_bar.onTally += (value, start, end) => SoundController.Instance.PlayGroup(SoundEffectType.sfx_ui_tally);
    }
}