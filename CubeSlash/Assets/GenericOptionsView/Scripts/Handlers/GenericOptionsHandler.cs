using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class GenericOptionsHandler : MonoBehaviour
    {
        private GenericOptions _root;

        protected GenericOptions Root => _root ?? (_root = GetComponentInParent<GenericOptions>());

        public event System.Action OnRestoreDefault;
        public event System.Action OnApply;

        private void Start()
        {
            Root.OnDataChanged += OnDataChanged;
        }

        protected virtual void OnDataChanged(OptionsData data)
        {
        }

        public virtual void RestoreDefault()
        {
            OnRestoreDefault?.Invoke();
        }

        public virtual void Apply()
        {
            OnApply?.Invoke();
        }
    }
}