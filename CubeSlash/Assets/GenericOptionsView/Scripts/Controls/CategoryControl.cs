using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class CategoryControl : ButtonControl
    {
        [SerializeField] private GameObject _content;

        public GameObject Content => _content;

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
            }

            Select();
        }
    }
}