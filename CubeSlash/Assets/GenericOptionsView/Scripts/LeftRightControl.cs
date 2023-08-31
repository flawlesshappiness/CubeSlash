using UnityEngine;
using UnityEngine.UI;

namespace Flawliz.GenericOptions
{
    public class LeftRightControl : MonoBehaviour
    {
        [SerializeField] protected Button _btnLeft, _btnRight;

        public System.Action OnClickLeft, OnClickRight;

        protected virtual void Start()
        {
            _btnLeft.onClick.AddListener(ClickLeft);
            _btnRight.onClick.AddListener(ClickRight);
        }

        protected virtual void ClickLeft()
        {
            OnClickLeft?.Invoke();
        }

        protected virtual void ClickRight()
        {
            OnClickRight?.Invoke();
        }
    }
}