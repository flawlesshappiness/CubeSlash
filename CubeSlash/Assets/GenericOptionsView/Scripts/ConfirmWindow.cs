using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Flawliz.GenericOptions
{
    public class ConfirmWindow : MonoBehaviour
    {
        [SerializeField] private ButtonControl _btn_confirm, _btn_cancel;
        [SerializeField] private TMP_Text _tmp_title;

        public event System.Action OnShow, OnHide;
        private event System.Action OnConfirm, OnCancel;

        public string TitleText { set { _tmp_title.text = value; } }

        public string ConfirmText { set { _btn_confirm.SetText(value); } }

        public string CancelText { set { _btn_cancel.SetText(value); } }

        private void Start()
        {
            _btn_confirm.OnSubmitEvent += ClickConfirm;
            _btn_cancel.OnSubmitEvent += ClickCancel;
        }

        private void ClickCancel()
        {
            OnCancel?.Invoke();
            Hide();
        }

        private void ClickConfirm()
        {
            OnConfirm?.Invoke();
            Hide();
        }

        public void Show(System.Action onConfirm, System.Action onCancel)
        {
            OnConfirm = onConfirm;
            OnCancel = onCancel;

            gameObject.SetActive(true);
            OnShow?.Invoke();

            EventSystem.current.SetSelectedGameObject(_btn_cancel.gameObject);
        }

        public void Hide()
        {
            EventSystem.current.SetSelectedGameObject(null);

            gameObject.SetActive(false);
            OnHide?.Invoke();
        }
    }
}