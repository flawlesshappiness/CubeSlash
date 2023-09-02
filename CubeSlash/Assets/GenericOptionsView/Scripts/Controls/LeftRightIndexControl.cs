using System.Linq;
using UnityEngine;

namespace Flawliz.GenericOptions
{
    public abstract class LeftRightIndexControl<T> : MonoBehaviour
    {
        [SerializeField] private LeftRightControl _control;

        protected LeftRightControl Control => _control;
        public abstract T[] Values { get; }

        private int _idx_selected;

        public event System.Action<int> OnIndexChanged;
        public event System.Action<T> OnValueChanged;

        protected virtual void Awake()
        {
            _control.OnLeft += OnLeft;
            _control.OnRight += OnRight;
        }

        protected virtual void OnValidate()
        {
            _control ??= GetComponentInChildren<LeftRightControl>();
        }

        private void OnRight()
        {
            AdjustIndex(1);
        }

        private void OnLeft()
        {
            AdjustIndex(-1);
        }

        private void AdjustIndex(int i)
        {
            SetIndex(_idx_selected + i);
        }

        public void SetIndex(int i)
        {
            _idx_selected = Mathf.Clamp(i, 0, Values.Length - 1);
            OnValueChanged?.Invoke(GetSelectedValue());
            OnIndexChanged?.Invoke(_idx_selected);
        }

        public void SetIndexFromValue(T value)
        {
            var idx = Values.ToList().IndexOf(value);
            SetIndex(idx);
        }

        public T GetSelectedValue() => Values[Mathf.Clamp(_idx_selected, 0, Values.Length - 1)];
    }
}