using System;
using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class CategoryControl : ButtonControl
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private bool _disableRestoreDefaultsButton;

        public Action OnSubmit;

        public GameObject Content => _content;
        public bool DisableRestoreDefaultsButton => _disableRestoreDefaultsButton;

        public static CategoryControl ActiveCategory { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!Application.isPlaying) return;

            SetContentActive(false);
        }

        public void SetContentActive(bool active)
        {
            _content.SetActive(active);
        }

        public override void Submit()
        {
            base.Submit();

            if (ActiveCategory != null)
            {
                ActiveCategory.SetContentActive(false);
            }

            ActiveCategory = this;

            if (ActiveCategory != null)
            {
                ActiveCategory.SetContentActive(true);
                OnSubmit?.Invoke();
            }

            Select();
        }
    }
}