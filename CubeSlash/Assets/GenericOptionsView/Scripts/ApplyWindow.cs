using Flawliz.GenericOptions;
using UnityEngine;
using UnityEngine.EventSystems;

public class ApplyWindow : MonoBehaviour
{
    [SerializeField] private ButtonControl _btn_apply, _btn_cancel;

    public event System.Action OnApply, OnCancel, OnShow, OnHide;

    private void Start()
    {
        _btn_apply.OnSubmitEvent += ClickApply;
        _btn_cancel.OnSubmitEvent += ClickCancel;
    }

    private void ClickCancel()
    {
        OnCancel?.Invoke();
        Hide();
    }

    private void ClickApply()
    {
        OnApply?.Invoke();
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        OnShow?.Invoke();

        EventSystem.current.SetSelectedGameObject(_btn_cancel.gameObject);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        OnHide?.Invoke();
    }
}