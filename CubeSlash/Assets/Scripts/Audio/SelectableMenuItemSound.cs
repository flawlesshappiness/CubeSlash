using UnityEngine;

public class SelectableMenuItemSound : MonoBehaviour
{
    [SerializeField] private SelectableMenuItem item;

    [SerializeField] private SoundEffectType sfx_select;
    [SerializeField] private SoundEffectType sfx_submit;

    private void Reset()
    {
        item = item ?? GetComponent<SelectableMenuItem>();
    }

    private void Start()
    {
        item.onSelect += OnSelect;
        item.onSubmit += OnSubmit;
    }

    private void OnSelect()
    {
        SoundController.Instance.Play(sfx_select);
    }

    private void OnSubmit()
    {
        SoundController.Instance.Play(sfx_submit);
    }
}